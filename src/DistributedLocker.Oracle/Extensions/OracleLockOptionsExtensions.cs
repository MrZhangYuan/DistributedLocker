using DistributedLocker.Internal;

namespace DistributedLocker.Oracle.Extensions
{
    public static class OracleLockOptionsExtensions
    {
        public static LockOptionsBuilder UseOracleLock(this LockOptionsBuilder builder,
            string connstr)
        {
            UtilMethods.ThrowIfNull(builder, nameof(builder));

            builder.WithOption<OracleDataBaseLockOptionsExtension>(
                    _p => (OracleDataBaseLockOptionsExtension)_p.WithConnectionString(connstr)
                );

            return builder;
        }
    }
}
