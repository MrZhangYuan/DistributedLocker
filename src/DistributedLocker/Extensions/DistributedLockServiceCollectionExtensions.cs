using Microsoft.Extensions.DependencyInjection;
using System;

namespace DistributedLocker.Extensions
{
    public static class DistributedLockServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedLock(this IServiceCollection services, Action<LockOptionsBuilder> builderact)
        {
            LockOptionsBuilder builder = new LockOptionsBuilder();

            if (builderact != null)
            {
                builderact(builder);
            }

            services.AddSingleton<ILockOptions>(builder.Options);
            services.AddScoped<DistributedLockContext>();

            return services;
        }
    }

}
