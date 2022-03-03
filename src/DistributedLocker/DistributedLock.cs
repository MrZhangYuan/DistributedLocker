using DistributedLocker.Extensions;
using DistributedLocker.Internal;
using System;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public abstract class DistributedLock : IAsyncDistributedLock
    {
        private readonly ILockOptions _options = null;
        private readonly CoreLockOptionsExtension _coreOptionsExtension = null;

        protected DistributedLock(ILockOptions options)
        {
            UtilMethods.ThrowIfNull(options, nameof(options));

            _options = options;

            _coreOptionsExtension = this._options.FindExtension<CoreLockOptionsExtension>();

            UtilMethods.ThrowIfNull(_coreOptionsExtension, nameof(_coreOptionsExtension));
        }

        protected virtual LockParameter CreatLockParameter(Lockey lockey)
        {
            UtilMethods.ThrowIfNull(lockey, nameof(lockey));

            return new LockParameter
            {
                ConflictPloy = _coreOptionsExtension.DefaultConflictPloy,
                RetryInterval = _coreOptionsExtension.DefaultRetryInterval,
                RetryTimes = _coreOptionsExtension.DefaultRetryTimes,
                Duation = _coreOptionsExtension.DefaultDuation
            };
        }

        protected virtual Locker CreateLocker(Lockey lockey, LockParameter parameter)
        {
            UtilMethods.ThrowIfNull(parameter, nameof(parameter));

            return new Locker
            {
                BusinessCode = lockey.InternalCode,
                BusinessType = lockey.InternalType,
                Duation = parameter.Duation,
                IP = parameter.IP,
                Token = lockey.Token,
                DelayTimes = 0,
                LockMsg = parameter.LockMsg,
                ConflictMsg = parameter.ConflictMsg,
                HostName = Environment.MachineName,
                OperCode = parameter.OperCode,
                OperName = parameter.OperName,
                OperType = parameter.OperType
            };
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

        public abstract Locker Enter(Lockey lockey, LockParameter parameter);
        public abstract bool TryEnter(Lockey lockey, LockParameter parameter, out Locker locker);
        public abstract void Keep(Lockey lockey, TimeSpan span);
        public abstract void Exit(Lockey lockey);

        public abstract ValueTask<Locker> EnterAsync(Lockey lockey, LockParameter parameter);
        public abstract ValueTask<bool> TryEnterAsync(Lockey lockey, LockParameter parameter, out Locker locker);
        public abstract ValueTask KeepAsync(Lockey lockey, TimeSpan span);
        public abstract ValueTask ExitAsync(Lockey lockey);
    }
}
