using DistributedLocker.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedLocker.Memory
{
    public class MemoryDistributedLock : AsyncDistributedLock
    {
        private static readonly ConcurrentDictionary<Lockey, Locker> _lockers = new ConcurrentDictionary<Lockey, Locker>();

        public MemoryDistributedLock(ILockOptions options, IDistributedLockCacher cacher)
            : base(options, cacher)
        {

        }

        protected override bool CanUseCache() => false;

        protected override Locker Enter(Lockey lockey,
            Locker locker,
            LockParameter param)
        {
            if (this.TryEnter(lockey, locker, param))
            {
                return locker;
            }

            throw new LockConflictException(locker);
        }
        protected override ValueTask<Locker> EnterAsync(Lockey lockey,
            Locker locker,
            LockParameter parameter)
        {
            this.Enter(
                lockey,
                locker,
                parameter);

            return UtilMethods.ValueTaskFromResult(locker);
        }


        protected override bool TryEnter(Lockey lockey,
            Locker locker,
            LockParameter parameter)
        {
            bool entered = false;

            _lockers.GetOrAdd(
                lockey,
                _k =>
                {
                    entered = true;
                    return locker;
                });

            return entered;
        }
        protected override ValueTask<bool> TryEnterAsync(Lockey lockey,
            Locker locker,
            LockParameter parameter)
        {
            bool entered = this.TryEnter(
                            lockey,
                            locker,
                            parameter);

            return UtilMethods.ValueTaskFromResult(entered);
        }


        protected override void Keep(Lockey lockey,
            Locker locker,
            TimeSpan span)
        {
            _lockers.AddOrUpdate(
                lockey,
                _k => throw new LockExpiredException(lockey),
                (_k, _kr) =>
                {
                    _kr.EndTime += (long)span.TotalMilliseconds;
                    return _kr;
                });
        }
        public override ValueTask KeepAsync(Lockey lockey,
            Locker locker,
            TimeSpan span)
        {
            this.Keep(
                lockey,
                locker,
                span);

            return UtilMethods.DefaultValueTask();
        }


        protected override void Exit(Lockey lockey, Locker locker)
        {
            _lockers.TryRemove(lockey, out _);
        }
        public override ValueTask ExitAsync(Lockey lockey, Locker locker)
        {
            this.Exit(
                lockey,
                locker);

            return UtilMethods.DefaultValueTask();
        }
    }
}
