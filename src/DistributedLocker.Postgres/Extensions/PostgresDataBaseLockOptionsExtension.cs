using DistributedLocker.DataBase;
using DistributedLocker.DataBase.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedLocker.Postgres.Extensions
{
    public class PostgresDataBaseLockOptionsExtension : DataBaseLockOptionsExtension
    {
        public override void ApplyServices(IServiceCollection services)
        {
            base.ApplyServices(services);

            services.AddScoped<IAsyncDistributedLock, PostgresDatabaseDistributedLock>();
            services.AddScoped<IDatabaseDistributedLockAdapter, PostgresDatabaseDistributedLockAdapter>();
        }
    }
}
