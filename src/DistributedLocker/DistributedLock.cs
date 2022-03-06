using DistributedLocker.Extensions;
using DistributedLocker.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public abstract class DistributedLock : DisposableObject, IAsyncDistributedLock
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


        protected abstract Locker Enter(Lockey lockey, Locker locker, LockParameter param);
        public virtual Locker Enter(Lockey lockey, LockParameter param)
        {
            if (param == null)
            {
                param = this.CreatLockParameter(lockey);
            }

            var locker = this.CreateLocker(lockey, param);

            int retrys = 0;

            do
            {
                Locker newlocker = null;
                LockConflictException cfe = null;

                try
                {
                    newlocker = this.Enter(lockey, locker, param);

                    UtilMethods.ThrowIfNull(
                        newlocker,
                        $"对类型 {this.GetType()} 的方法 Enter(Lockey lockey, Locker locker, LockParameter param) 的调用返回了意料之外的 null 值。");
                }
                catch (LockConflictException e)
                {
                    cfe = e;
                }

                if (cfe != null)
                {
                    this.ThrowIfConflicted(
                        cfe.ExistsLocker,
                        param,
                        ref retrys);

                    Thread.Sleep(param.RetryInterval);

                    continue;
                }

                return newlocker;
            }
            while (true);
        }



        protected abstract bool TryEnter(Lockey lockey, Locker locker, LockParameter parameter);
        public virtual bool TryEnter(Lockey lockey, LockParameter param, out Locker locker)
        {
            if (param == null)
            {
                param = this.CreatLockParameter(lockey);
            }

            locker = this.CreateLocker(lockey, param);

            int retrys = 0;

            do
            {
                bool entered = this.TryEnter(lockey, locker, param);

                if (!entered
                    && param.ConflictPloy == ConflictPloy.Wait
                    && retrys < param.RetryTimes)
                {
                    Thread.Sleep(param.RetryInterval);

                    continue;
                }

                locker = entered ? locker : null;

                return entered;
            }
            while (true);
        }



        public abstract void Keep(Lockey lockey, TimeSpan span);
        public abstract void Exit(Lockey lockey);



        protected abstract ValueTask<Locker> EnterAsync(Lockey lockey, Locker locker, LockParameter parameter);
        public virtual async ValueTask<Locker> EnterAsync(Lockey lockey, LockParameter param)
        {
            if (param == null)
            {
                param = this.CreatLockParameter(lockey);
            }

            var locker = this.CreateLocker(lockey, param);

            int retrys = 0;

            do
            {
                Locker newlocker = null;
                LockConflictException cfe = null;

                try
                {
                    newlocker = await this.EnterAsync(lockey, locker, param);

                    UtilMethods.ThrowIfNull(
                        newlocker,
                        $"对类型 {this.GetType()} 的方法 Enter(Lockey lockey, Locker locker, LockParameter param) 的调用返回了意料之外的 null 值。");
                }
                catch (LockConflictException e)
                {
                    cfe = e;
                }

                if (cfe != null)
                {
                    this.ThrowIfConflicted(
                        cfe.ExistsLocker,
                        param,
                        ref retrys);

                    await Task.Delay(param.RetryInterval);

                    continue;
                }

                return newlocker;
            }
            while (true);
        }



        protected abstract ValueTask<bool> TryEnterAsync(Lockey lockey, Locker locker, LockParameter parameter);
        public virtual ValueTask<bool> TryEnterAsync(Lockey lockey, LockParameter param, out Locker locker)
        {
            if (param == null)
            {
                param = this.CreatLockParameter(lockey);
            }

            //TODO 未对 TryEnterAsync 进行 await 之前，locker 也不为空，这是个瑕疵
            locker = this.CreateLocker(lockey, param);

            return TryEnterAsyncLocal(lockey, locker, param);

            async ValueTask<bool> TryEnterAsyncLocal(Lockey lockeyi, Locker lockeri, LockParameter parami)
            {
                int retrys = 0;

                do
                {
                    bool entered = await this.TryEnterAsync(lockeyi, lockeri, parami);

                    if (!entered
                        && parami.ConflictPloy == ConflictPloy.Wait
                        && retrys < parami.RetryTimes)
                    {
                        await Task.Delay(parami.RetryInterval);

                        continue;
                    }

                    return entered;
                }
                while (true);
            }
        }

        

        public abstract ValueTask KeepAsync(Lockey lockey, TimeSpan span);
        public abstract ValueTask ExitAsync(Lockey lockey);
    }
}
