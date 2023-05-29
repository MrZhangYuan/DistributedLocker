using DistributedLocker.DataBase;

namespace DistributedLocker.SqlServer
{
    public class SqlServerDatabaseDistributedLock : DatabaseDistributedLock
	{
		public SqlServerDatabaseDistributedLock(IDatabaseDistributedLockAdapter adapter,
			ILockOptions options,
			IDistributedLockCacher cacher)
			: base(adapter, options, cacher)
		{

		}
	}
}
