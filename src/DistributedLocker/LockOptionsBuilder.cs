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

            this.WithOption<CoreLockOptionsExtension>(_p => _p);
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

        public LockOptionsBuilder WidthRetry(int retrytimes, int interval)
        {
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthRetry(retrytimes, interval));
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

        public LockOptionsBuilder WidthPersistence(bool persistence, TimeSpan duation)
        {
            return this.WithOption<CoreLockOptionsExtension>(_p => _p.WidthPersistence(persistence, duation));
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
