using DistributedLocker.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public class MemoryDistributedLockCacher : IDistributedLockCacher
    {
        private static readonly ConcurrentDictionary<Lockey, Locker> _lockers = new ConcurrentDictionary<Lockey, Locker>();

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

        public Locker GetOrEnter(Lockey key, Func<Lockey, Locker> enter)
        {
            RemoveExpired();

            //  缓存的作用就在于此，每一次加锁之前先判断下内存
            //  通过删除或更新的时机
            if (_lockers.TryGetValue(key, out Locker exists))
            {
                return exists;
            }

            var locker = enter(key);

            //  enter 是互斥的且是原子的
            //  按理说过了 enter 之后不会出现缓存的 Update 只会出现 Add
            //  若出现 Update，可以认为 enter 的实现错误，发生了重入
            _lockers.AddOrUpdate(
                key,
                _k => locker,
                (_k, _kr) => throw new LockReentrantException(_k));

            return locker;
        }

        public void Update(Lockey key,
            TimeSpan span,
            Action<Lockey, Locker, TimeSpan> updater)
        {
            try
            {
                updater(key, null, span);

                if (_lockers.TryGetValue(key,out Locker locker))
                {
                    locker.EndTime += (long)span.TotalMilliseconds;
                }
            }
            catch (Exception)
            {
                _lockers.TryRemove(key, out _);

                throw;
            }
        }

        public void Exit(Lockey lockey, Action<Lockey, Locker> exiter)
        {
            _lockers.TryRemove(lockey, out _);

            exiter(lockey, null);
        }



        public async ValueTask<Locker> GetOrEnterAsync(Lockey key,
            Func<Lockey,
            ValueTask<Locker>> enter)
        {
            RemoveExpired();

            //  缓存的作用就在于此，每一次加锁之前先判断下内存
            //  通过删除或更新的时机
            if (_lockers.TryGetValue(key, out var exists))
            {
                return exists;
            }

            var locker = await enter(key);

            //  enter 是互斥的且是原子的
            //  按理说过了 enter 之后不会出现缓存的 Update 只会出现 Add
            //  若出现 Update，可以认为 enter 的实现错误，发生了重入
            _lockers.AddOrUpdate(
                key,
                _k => locker,
                (_k, _kr) => throw new LockReentrantException(_k));

            await UtilMethods.DefaultValueTask();

            return locker;
        }

        public async ValueTask UpdateAsync(Lockey key,
            TimeSpan span,
            Func<Lockey, Locker, TimeSpan, ValueTask> updater)
        {

            try
            {
                await updater(key, null, span);

                if (_lockers.TryGetValue(key, out Locker locker))
                {
                    locker.EndTime += (long)span.TotalMilliseconds;
                }

                await UtilMethods.DefaultValueTask();
            }
            catch (Exception)
            {
                _lockers.TryRemove(key, out _);

                throw;
            }
        }

        public async ValueTask ExitAsync(Lockey lockey, Func<Lockey, Locker, ValueTask> exiter)
        {
            _lockers.TryRemove(lockey, out _);

            await exiter(lockey, null);

            await UtilMethods.DefaultValueTask();
        }
    }
}
