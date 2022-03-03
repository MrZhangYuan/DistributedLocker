using DistributedLocker.DataBase;
using DistributedLocker.DataBase.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedLocker.Oracle.Extensions
{
    public class OracleDataBaseLockOptionsExtension : DataBaseLockOptionsExtension
    {
        public override void ApplyServices(IServiceCollection services)
        {
            base.ApplyServices(services);

            services.AddScoped<IAsyncDistributedLock, OracleDatabaseDistributedLock>();
            services.AddScoped<IDatabaseDistributedLockAdapter, OracleDatabaseDistributedLockAdapter>();
        }
    }
}
