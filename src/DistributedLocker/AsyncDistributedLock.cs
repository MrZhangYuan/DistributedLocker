using DistributedLocker.Extensions;
using DistributedLocker.Internal;
using System;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public abstract class AsyncDistributedLock : DistributedLock, IAsyncDistributedLock
    {
        private readonly ILockOptions _options = null;
        private readonly IDistributedLockCacher _lockCacher = null;
        private readonly bool? _useCache = false;
        private readonly TimeSpan _defaultKeepDuation = default;

        protected AsyncDistributedLock(ILockOptions options, IDistributedLockCacher cacher)
            : base(options, cacher)
        {
            _options = options;

            _lockCacher = cacher;

            var coreextension = this._options.FindExtension<CoreLockOptionsExtension>();

            _useCache = coreextension?.UseCache;

            _defaultKeepDuation = TimeSpan.FromMilliseconds(coreextension.DefaultKeepDuation);

            if (_useCache == true)
            {
                UtilMethods.ThrowIfNull(cacher, nameof(cacher));
            }
        }

        protected abstract ValueTask<Locker> EnterAsync(Lockey lockey,
            Locker locker, 
            LockParameter parameter);
        public virtual async ValueTask<Locker> EnterAsync(Lockey lockey, LockParameter param)
        {
            param = this.CreatOrSetDefaultParameter(lockey, param);

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
                        newlocker = await this._lockCacher.GetOrEnterAsync(
                                        lockey,
                                        async _k =>
                                        {
                                            var temp = await this.EnterAsync(
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
                        newlocker = await this.EnterAsync(
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

                    await Task.Delay(param.RetryInterval.Value);

                    continue;
                }

                return newlocker;
            }
            while (true);
        }



        protected abstract ValueTask<bool> TryEnterAsync(Lockey lockey,
            Locker locker, 
            LockParameter parameter);
        public virtual ValueTask<bool> TryEnterAsync(Lockey lockey,
            LockParameter param,
            out Locker locker)
        {
            param = this.CreatOrSetDefaultParameter(lockey, param);

            //  未对 TryEnterAsync 进行 await 之前，locker 也不为空，这是个瑕疵
            locker = this.CreateLocker(lockey, param);

            return TryEnterAsyncLocal(lockey, locker, param);

            async ValueTask<bool> TryEnterAsyncLocal(Lockey lockeyi,
                Locker lockeri, 
                LockParameter parami)
            {
                int retrys = 0;

                do
                {
                    bool entered = false;

                    if (this._useCache == true)
                    {
                        await this._lockCacher.GetOrEnterAsync(
                            lockey,
                            async _k =>
                            {
                                entered = await this.TryEnterAsync(
                                            lockey,
                                            lockeri,
                                            param);

                                return lockeri;
                            });
                    }
                    else
                    {
                        entered = await this.TryEnterAsync(
                                    lockey, 
                                    lockeri,
                                    param);
                    }

                    if (!entered
                        && parami.ConflictPloy == ConflictPloy.Wait
                        && retrys < parami.RetryTimes)
                    {
                        await Task.Delay(parami.RetryInterval.Value);

                        continue;
                    }

                    return entered;
                }
                while (true);
            }
        }



        public abstract ValueTask KeepAsync(Lockey lockey, 
            Locker locker,
            TimeSpan span);
        public virtual async ValueTask KeepAsync(Lockey lockey, TimeSpan span)
        {
            if (_useCache == true)
            {
                await this._lockCacher.UpdateAsync(
                        lockey,
                        span,
                        (_k, _kr, _s) => this.KeepAsync(_k, _kr, _s));
            }
            else
            {
                await this.KeepAsync(
                    lockey,
                    null,
                    span);
            }
        }
        public virtual async ValueTask KeepAsync(Lockey lockey)
        {
            await this.KeepAsync(lockey, _defaultKeepDuation);
        }



        public abstract ValueTask ExitAsync(Lockey lockey, Locker locker);
        public async virtual ValueTask ExitAsync(Lockey lockey)
        {
            if (_useCache == true)
            {
                await this._lockCacher.ExitAsync(
                        lockey,
                        (_k, _kr) => this.ExitAsync(_k, _kr));
            }
            else
            {
                await this.ExitAsync(lockey, null);
            }
        }
    }
}
