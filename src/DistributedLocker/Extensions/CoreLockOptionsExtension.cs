using DistributedLocker.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DistributedLocker.Extensions
{
    public class CoreLockOptionsExtension : ILockOptionsExtension
    {
        private ConflictPloy _defaultConflictPloy = ConflictPloy.Wait;

        //  最好 _defaultRetryInterval * _defaultRetryTimes > _defaultDuation 即可
        private int _defaultRetryInterval = 45;
        private int _defaultRetryTimes = 5;
        private int _defaultDuation = 200;
        private int _defaultKeepDuation = 100;
        private bool _useCache = false;
        private bool _autoKeep = false;

        /// <summary>
        ///     持久化锁的过期时间
        /// </summary>
        private TimeSpan _persistenceDuation = TimeSpan.FromDays(10);

        /// <summary>
        ///     默认持久化
        /// </summary>
        private bool _defaultPersistence = false;

        public ConflictPloy DefaultConflictPloy
        {
            get => _defaultConflictPloy;
        }

        public int DefaultRetryInterval
        {
            get => _defaultRetryInterval;
        }

        public int DefaultRetryTimes
        {
            get => _defaultRetryTimes;
        }

        public int DefaultDuation
        {
            get => _defaultDuation;
        }

        public int DefaultKeepDuation
        {
            get => _defaultKeepDuation;
        }

        public bool UseCache
        {
            get => _useCache;
        }

        public bool AutoKeep
        {
            get => _autoKeep;
        }

        public TimeSpan PersistenceDuation
        {
            get => _persistenceDuation;
        }

        public void ApplyServices(IServiceCollection services)
        {
            services.AddScoped<IAutoKeeper, AutoKeeper>();
        }

        public void Validate(ILockOptions options)
        {

        }

        public CoreLockOptionsExtension WidthConflictPloy(ConflictPloy conflictploy)
        {
            this._defaultConflictPloy = conflictploy;
            return this;
        }

        public CoreLockOptionsExtension WidthRetry(int retrytimes, int interval)
        {
            this._defaultRetryTimes = retrytimes;
            this._defaultRetryInterval = interval;
            return this;
        }

        public CoreLockOptionsExtension WidthDuation(int duation)
        {
            this._defaultDuation = duation;
            return this;
        }

        public CoreLockOptionsExtension WidthKeepDuation(int duation)
        {
            this._defaultKeepDuation = duation;
            return this;
        }

        public CoreLockOptionsExtension WidthCache(bool usecache)
        {
            this._useCache = usecache;
            return this;
        }

        public CoreLockOptionsExtension WidthAutoKeep(bool autokeep)
        {
            this._autoKeep = autokeep;
            return this;
        }


        public CoreLockOptionsExtension WidthPersistence(bool persistence, TimeSpan duation)
        {
            this._defaultPersistence = persistence;
            this._persistenceDuation = duation;
            return this;
        }

        internal LockParameter CreateDefaultParameter(Lockey lockey)
        {
            return new LockParameter
            {
                ConflictPloy = _defaultConflictPloy,
                RetryInterval = _defaultRetryInterval,
                RetryTimes = _defaultRetryTimes,
                Duation = _defaultDuation,
                KeepDuation = _defaultKeepDuation,
                AutoKeep = _autoKeep,
                IsPersistence = _defaultPersistence
            };
        }

        internal LockParameter OverrideDefaultParameter(LockParameter param)
        {
            UtilMethods.ThrowIfNull(param, nameof(param));

            if (!param.ConflictPloy.HasValue)
            {
                param.ConflictPloy = _defaultConflictPloy;
            }

            if (!param.RetryInterval.HasValue)
            {
                param.RetryInterval = _defaultRetryInterval;
            }

            if (!param.RetryTimes.HasValue)
            {
                param.RetryTimes = _defaultRetryTimes;
            }

            if (!param.Duation.HasValue)
            {
                param.Duation = _defaultDuation;
            }

            if (!param.KeepDuation.HasValue)
            {
                param.KeepDuation = _defaultKeepDuation;
            }

            if (!param.AutoKeep.HasValue)
            {
                param.AutoKeep = _autoKeep;
            }

            if (!param.IsPersistence.HasValue)
            {
                param.IsPersistence = _defaultPersistence;
            }

            return param;
        }
    }
}
