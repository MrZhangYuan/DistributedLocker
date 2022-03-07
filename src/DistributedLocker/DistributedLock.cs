using DistributedLocker.Extensions;
using DistributedLocker.Internal;
using System;
using System.Threading;

namespace DistributedLocker
{
    public abstract class DistributedLock : DisposableObject, IDistributedLock
    {
        private readonly ILockOptions _options = null;
        private readonly CoreLockOptionsExtension _coreOptionsExtension = null;
        private readonly IDistributedLockCacher _lockCacher = null;
        private readonly bool? _useCache = false;

        protected DistributedLock(ILockOptions options, IDistributedLockCacher cacher)
        {
            UtilMethods.ThrowIfNull(options, nameof(options));

            _options = options;

            _lockCacher = cacher;

            _useCache = this._options.FindExtension<CoreLockOptionsExtension>()
                        ?.UseCache;

            if (_useCache == true
                && this.CanUseCache())
            {
                UtilMethods.ThrowIfNull(cacher, nameof(cacher));
            }
        }


        protected virtual bool CanUseCache() => true;


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


        protected virtual Locker CreateLocker(Lockey lockey,
            LockParameter param)
        {
            UtilMethods.ThrowIfNull(param, nameof(param));

            return new Locker
            {
                BusinessCode = lockey.InternalCode,
                BusinessType = lockey.InternalType,

                //  锁并不以本地 BeginTime 和 EndTime 为准，而是以服务器时间为准
                //  所以，这两个时间戳并不会存入数据库或其他介质中
                //  这里取值只是作为本地内存缓存之用，因为 Duation 是不变的
                BeginTime = UtilMethods.GetTimeStamp(),
                EndTime = UtilMethods.GetTimeStamp() + param.Duation,

                Duation = param.Duation,
                IP = param.IP,
                Token = lockey.Token,
                DelayTimes = 0,
                LockMsg = param.LockMsg,
                ConflictMsg = param.ConflictMsg,
                HostName = Environment.MachineName,
                OperCode = param.OperCode,
                OperName = param.OperName,
                OperType = param.OperType,
                IsPersistence = param.IsPersistence
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


        protected abstract Locker Enter(Lockey lockey,
            Locker locker,
            LockParameter param);
        public virtual Locker Enter(Lockey lockey,
            LockParameter param)
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
                    bool entered = false;

                    if (this._useCache == true)
                    {
                        newlocker = this._lockCacher.GetOrEnter(
                                    lockey,
                                    _k =>
                                    {
                                        var temp = this.Enter(
                                                    lockey,
                                                    locker,
                                                    param);

                                        entered = true;

                                        return temp;
                                    });

                        if (!entered)
                        {
                            cfe = new LockConflictException(newlocker);
                        }
                    }
                    else
                    {
                        newlocker = this.Enter(
                                    lockey,
                                    locker,
                                    param);

                        entered = true;
                    }

                    if (entered)
                    {
                        UtilMethods.ThrowIfNull(
                            newlocker,
                            $"对类型 {this.GetType()} 的方法 Enter(Lockey lockey, Locker locker, LockParameter param) 的调用返回了意料之外的 null 值。");
                    }
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



        protected abstract bool TryEnter(Lockey lockey,
            Locker locker,
            LockParameter parameter);
        public virtual bool TryEnter(Lockey lockey,
            LockParameter param,
            out Locker locker)
        {
            if (param == null)
            {
                param = this.CreatLockParameter(lockey);
            }

            var tplocker = this.CreateLocker(lockey, param);

            int retrys = 0;

            do
            {
                bool entered = false;

                if (this._useCache == true)
                {
                    this._lockCacher.GetOrEnter(
                        lockey,
                        _k =>
                        {
                            entered = this.TryEnter(
                                        lockey,
                                        tplocker,
                                        param);

                            return tplocker;
                        });
                }
                else
                {
                    entered = this.TryEnter(
                                lockey,
                                tplocker,
                                param);
                }

                if (!entered
                    && param.ConflictPloy == ConflictPloy.Wait
                    && retrys < param.RetryTimes)
                {
                    Thread.Sleep(param.RetryInterval);

                    continue;
                }

                locker = entered ? tplocker : null;

                return entered;
            }
            while (true);
        }


        protected abstract void Keep(Lockey lockey,
            Locker locker,
            TimeSpan span);
        public virtual void Keep(Lockey lockey, TimeSpan span)
        {
            if (_useCache == true)
            {
                this._lockCacher.Update(
                    lockey,
                    span,
                    (_k, _kr, _s) => this.Keep(_k, _kr, _s));
            }
            else
            {
                this.Keep(lockey,
                    null,
                    span);
            }
        }


        protected abstract void Exit(Lockey lockey, Locker locker);
        public virtual void Exit(Lockey lockey)
        {
            if (_useCache == true)
            {
                this._lockCacher.Exit(
                    lockey,
                    (_k, _kr) => this.Exit(_k, _kr));
            }
            else
            {
                this.Exit(lockey, null);
            }
        }
    }
}
