using DistributedLocker.Extensions;
using DistributedLocker.Internal;
using System;

namespace DistributedLocker
{
    public class LockOptionsBuilder
    {
        private readonly ILockOptions _options;

        public virtual ILockOptions Options => _options;

        public LockOptionsBuilder()
            : this(new LockOptions())
        {

        }
        public LockOptionsBuilder(ILockOptions options)
        {
            UtilMethods.ThrowIfNull(options, nameof(options));

            _options = options;
        }


        public void AddOrUpdateExtension<TExtension>(TExtension extension)
            where TExtension : class, ILockOptionsExtension
        {
            _options.WidthExtension<TExtension>(extension);
        }

        public LockOptionsBuilder WidthConflictPloy(ConflictPloy conflictploy)
        {
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthConflictPloy(conflictploy));
        }

        public LockOptionsBuilder WidthRetryInterval(int interval)
        {
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthRetryInterval(interval));
        }

        public LockOptionsBuilder WidthRetryTimes(int retrytimes)
        {
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthRetryTimes(retrytimes));
        }

        public LockOptionsBuilder WidthDuation(int duation)
        {
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthDuation(duation));
        }

        public LockOptionsBuilder WidthKeepDuation(int duation)
        {
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthKeepDuation(duation));
        }

        public LockOptionsBuilder WidthCache(bool usecache)
        {
            this.WithOption<MemoryCacherOptionsExtension>(_p => _p);
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthCache(usecache));
        }

        public LockOptionsBuilder WidthAutoKeep(bool autokeep)
        {
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthAutoKeep(autokeep));
        }

        public LockOptionsBuilder WidthPersistenceDuation(TimeSpan duation)
        {
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthPersistenceDuation(duation));
        }

        public LockOptionsBuilder WidthDefaultPersistence(bool persistence)
        {
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthDefaultPersistence(persistence));
        }

        public virtual LockOptionsBuilder WithOption<TExtension>(Func<TExtension, TExtension> setAction)
            where TExtension : class, ILockOptionsExtension, new()
        {
            this.AddOrUpdateExtension(
                setAction(this.Options.FindExtension<TExtension>() ?? new TExtension()));

            return this;
        }
    }
}
