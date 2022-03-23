using DistributedLocker.DataBase;
using DistributedLocker.DataBase.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedLocker.SqlServer.Extensions
{
    public class SqlServerDataBaseLockOptionsExtension : DataBaseLockOptionsExtension
    {
        public override void ApplyServices(IServiceCollection services)
        {
            base.ApplyServices(services);

            services.AddScoped<IAsyncDistributedLock, SqlServerDatabaseDistributedLock>();
            services.AddScoped<IDatabaseDistributedLockAdapter, SqlServerDatabaseDistributedLockAdapter>();
        }
    }
}
