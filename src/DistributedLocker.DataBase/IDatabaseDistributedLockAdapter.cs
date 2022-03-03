using System;
using System.Data.Common;

namespace DistributedLocker.DataBase
{
    public interface IDatabaseDistributedLockAdapter
    {
        DbConnection CreateDbConnection();
        string CreateCreate();
        string CreateSelect(Lockey lockey);
        string CreateInsert(Locker locker);
        string CreateUpdate(Lockey lockey);
        string CreateDelete(Lockey lockey);
        bool CheckIfConflictException(Exception exception);
    }
}
