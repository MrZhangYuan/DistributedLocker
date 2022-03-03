using DistributedLocker.Internal;

namespace DistributedLocker.Memory.Extensions
{
    public static class MemoryLockOptionsExtensions
    {
        public static LockOptionsBuilder UseMemoryLock(this LockOptionsBuilder builder)
        {
            UtilMethods.ThrowIfNull(builder, nameof(builder));

            builder.WithOption<MemoryLockOptionsExtension>(
                    _p => _p
                );

            return builder;
        }
    }
}
