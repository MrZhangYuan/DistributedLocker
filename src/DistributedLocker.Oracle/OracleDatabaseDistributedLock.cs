using Dapper;
using DistributedLocker.DataBase;
using System.Threading.Tasks;

namespace DistributedLocker.Oracle
{
    public class OracleDatabaseDistributedLock : DatabaseDistributedLock
    {
        public OracleDatabaseDistributedLock(IDatabaseDistributedLockAdapter adapter, 
            ILockOptions options,
            IDistributedLockCacher cacher)
            : base(adapter, options, cacher)
        {

        }
    }
}
