using DistributedLocker.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public abstract class CachedDistributedLock : DistributedLock
    {
        private static readonly ConcurrentDictionary<Lockey, Locker> _lockers = new ConcurrentDictionary<Lockey, Locker>();

        protected CachedDistributedLock(ILockOptions options)
            : base(options)
        {
        }

        private static void RemoveExpired()
        {
            var nowstamp = UtilMethods.GetTimeStamp();

            var lockeys = new List<Lockey>();

            foreach (var item in _lockers)
            {
                if (item.Value.EndTime < nowstamp)
                {
                    lockeys.Add(item.Key);
                }
            }

            if (lockeys.Count > 0)
            {
                for (int i = 0; i < lockeys.Count; i++)
                {
                    _lockers.TryRemove(lockeys[i], out _);
                }
            }
        }

        protected virtual void ThrowIfConflicted(Locker exists,
            LockParameter param,
            ref int retrys)
        {
            switch (param.ConflictPloy)
            {
                case ConflictPloy.Exception:
                    throw new LockConflictException(exists, param.ConflictMsg);

                case ConflictPloy.Wait:
                    if (retrys < param.RetryTimes)
                    {
                        retrys++;

                        break;
                    }
                    throw new LockConflictException(exists, param.ConflictMsg);

                case ConflictPloy.Execute:
                    {
                        param.OnConflict?.Invoke(exists);
                    }
                    throw new LockConflictException(exists, param.ConflictMsg);

                default:
                    throw new InvalidOperationException();
            }
        }


        protected virtual async ValueTask<Locker> EnterAsync(Lockey lockey,
            Func<Lockey, LockParameter, ValueTask<Locker>> enter,
            LockParameter param)
        {
            int retrys = 0;

            //  如何校准每一个节点与介质的时间
            //  可以不用校准 缓存中的锁虽然与介质中的锁信息稍微有误，如：起止时间
            //  但是时间跨度都是一致的
            //  TODO 重试次数有问题

            do
            {
                RemoveExpired();

                bool flag = false;

                if (!_lockers.TryGetValue(lockey, out var exists))
                {
                    var newlocker = await enter(lockey, param);
                    _lockers.TryAdd(lockey, newlocker);
                    exists = newlocker;
                    flag = true;
                }

                if (!flag)
                {
                    this.ThrowIfConflicted(
                        exists,
                        param,
                        ref retrys);

                    await Task.Delay(param.RetryInterval);

                    continue;
                }

                return exists;
            }
            while (true);
        }

        protected virtual async ValueTask KeepAsync(Lockey lockey,
            Func<Lockey, TimeSpan, ValueTask> keeper,
            TimeSpan span)
        {
            if (_lockers.TryGetValue(lockey, out var exists))
            {         
                await keeper(lockey, span);

                exists.EndTime += (long)span.TotalMilliseconds;

                return;
            }

            throw new LockExpiredException();
        }

        protected virtual async ValueTask ExitAsync(Lockey lockey, Func<Lockey, ValueTask> exiter)
        {
            _lockers.TryRemove(lockey, out _);

            await exiter(lockey);
        }



        protected virtual Locker Enter(Lockey lockey,
            Func<Lockey, LockParameter, Locker> enter,
            LockParameter param)
        {
            int retrys = 0;

            //  如何校准每一个节点与介质的时间
            //  可以不用校准 缓存中的锁虽然与介质中的锁信息稍微有误，如：起止时间
            //  但是时间跨度都是一致的
            //  TODO 重试次数有问题

            do
            {
                RemoveExpired();

                Locker newlocker = null;

                bool flag = false;

                var exists = _lockers.GetOrAdd(
                                lockey,
                                _k =>
                                {
                                    newlocker = enter(_k, param);
                                    flag = true;
                                    return newlocker;
                                });

                if (!flag)
                {
                    this.ThrowIfConflicted(
                        exists,
                        param,
                        ref retrys);

                    Thread.Sleep(param.RetryInterval);

                    continue;
                }

                return exists;
            }
            while (true);
        }

        protected virtual void Keep(Lockey lockey,
            Action<Lockey, TimeSpan> keeper,
            TimeSpan span)
        {
            _lockers.AddOrUpdate(
                lockey,
                _k => throw new LockExpiredException(),
                (_k, _kr) =>
                {     
                    keeper(_k, span);
                    _kr.EndTime += (long)span.TotalMilliseconds;
                    return _kr;
                });
        }

        protected virtual void Exit(Lockey lockey, Action<Lockey> exiter)
        {
            _lockers.TryRemove(lockey, out _);

            exiter(lockey);
        }
    }
}
