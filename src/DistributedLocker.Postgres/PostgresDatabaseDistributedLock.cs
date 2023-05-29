using DistributedLocker.DataBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedLocker.Postgres
{
    public class PostgresDatabaseDistributedLock : DatabaseDistributedLock
    {
        public PostgresDatabaseDistributedLock(IDatabaseDistributedLockAdapter adapter,
            ILockOptions options,
            IDistributedLockCacher cacher)
            : base(adapter, options, cacher)
        {

        }
    }
}
