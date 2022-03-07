using Microsoft.Extensions.DependencyInjection;
using System;

namespace DistributedLocker.Extensions
{
    public class CoreLockOptionsExtension : ILockOptionsExtension
    {
        private ConflictPloy _defaultConflictPloy = ConflictPloy.Wait;
        private int _defaultRetryInterval = 50;
        private int _defaultRetryTimes = 3;
        private int _defaultDuation = 200;
        private int _defaultKeepDuation = 100;
        private bool _useCache = false;
        private bool _autoKeep = false;

        /// <summary>
        ///     持久化锁的过期时间
        /// </summary>
        private TimeSpan _persistenceExpiredTime = TimeSpan.FromDays(30);

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

        public void ApplyServices(IServiceCollection services)
        {
            services.AddScoped<IAutoKeeper, AutoKeeper>();
            services.AddSingleton<IDistributedLockCacher, MemoryDistributedLockCacher>();
        }

        public void Validate(ILockOptions options)
        {

        }

        public CoreLockOptionsExtension WidthConflictPloy(ConflictPloy conflictploy)
        {
            this._defaultConflictPloy = conflictploy;
            return this;
        }

        public CoreLockOptionsExtension WidthRetryInterval(int interval)
        {
            this._defaultRetryInterval = interval;
            return this;
        }

        public CoreLockOptionsExtension WidthRetryTimes(int retrytimes)
        {
            this._defaultRetryTimes = retrytimes;
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
    }
}
