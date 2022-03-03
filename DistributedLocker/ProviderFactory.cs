using Microsoft.Extensions.DependencyInjection;
using System;

namespace DistributedLocker
{
    internal static class ProviderFactory
    {
        public static IServiceProvider GetProvider(ILockOptions options)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<ILockOptions>(options);
            foreach (var extension in options.Extensions)
            {
                extension.ApplyServices(services);
            }
            var provider = services.BuildServiceProvider();
            return provider;
        }
    }

}
