using DistributedLocker.Internal;

namespace DistributedLocker.Postgres.Extensions
{
    public static class PostgresLockOptionsExtensions
    {
        public static LockOptionsBuilder UsePostgresLock(this LockOptionsBuilder builder,
            string connstr)
        {
            UtilMethods.ThrowIfNull(builder, nameof(builder));

            builder.WithOption<PostgresDataBaseLockOptionsExtension>(
                    _p => (PostgresDataBaseLockOptionsExtension)_p.WithConnectionString(connstr)
                );

            return builder;
        }
    }
}
