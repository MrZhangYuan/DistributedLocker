using DistributedLocker.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedLocker.SqlServer.Extensions
{
    public static class SqlServerLockOptionsExtensions
    {
        public static LockOptionsBuilder UseSqlServerLock(this LockOptionsBuilder builder,
            string connstr)
        {
            UtilMethods.ThrowIfNull(builder, nameof(builder));

            builder.WithOption<SqlServerDataBaseLockOptionsExtension>(
                    _p => (SqlServerDataBaseLockOptionsExtension)_p.WithConnectionString(connstr)
                );

            return builder;
        }
    }
}
