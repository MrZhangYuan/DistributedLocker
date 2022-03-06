using DistributedLocker.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedLocker.Memory
{
    public class MemoryDistributedLock : CachedDistributedLock
    {
        public MemoryDistributedLock(ILockOptions options)
            : base(options)
        {
        }

        public override Locker Enter(Lockey lockey, LockParameter parameter)
        {
            return this.Enter(
                    lockey,
                    (_k, _p) => this.CreateLocker(_k, _p),
                    parameter);
        }

        public override ValueTask<Locker> EnterAsync(Lockey lockey, LockParameter parameter)
        {
            return UtilMethods.ValueTaskFromResult(
                    this.Enter(lockey, parameter));
        }

        public override void Exit(Lockey lockey)
        {
            this.Exit(lockey,
                _k => { });
        }

        public override ValueTask ExitAsync(Lockey lockey)
        {
            this.Exit(lockey);

            return UtilMethods.DefaultValueTask();
        }

        public override void Keep(Lockey lockey, TimeSpan span)
        {
            this.Keep(
                lockey,
                (_k, _s) => { },
                span);
        }

        public override ValueTask KeepAsync(Lockey lockey, TimeSpan span)
        {
            this.Keep(lockey, span);

            return UtilMethods.DefaultValueTask();
        }

        public override bool TryEnter(Lockey lockey, LockParameter parameter, out Locker locker)
        {
            return this.TryEnter(
                    lockey,
                    (_k, _p) => this.CreateLocker(_k, _p),
                    parameter,
                    out locker);
        }

        public override ValueTask<bool> TryEnterAsync(Lockey lockey, LockParameter parameter, out Locker locker)
        {
            var result = this.TryEnter(
                            lockey,
                            parameter,
                            out locker);

            return UtilMethods.ValueTaskFromResult(result);
        }
    }
}
