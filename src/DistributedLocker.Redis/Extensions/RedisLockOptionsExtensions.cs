using DistributedLocker.Internal;

namespace DistributedLocker.Redis.Extensions
{
    public static class RedisLockOptionsExtensions
    {
        public static LockOptionsBuilder UseRedisLock(this LockOptionsBuilder builder, string connstr, int dbnum)
        {
            UtilMethods.ThrowIfNull(builder, nameof(builder));

            builder.WithOption<RedisLockOptionsExtension>(
                    _p => _p.WithConnectionString(connstr)
                            .WithDbNum(dbnum)
                );

            return builder;
        }
    }
}
