using DistributedLocker.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedLocker.Memory.Extensions
{
    public class MemoryLockOptionsExtension : ILockOptionsExtension
    {
        public void ApplyServices(IServiceCollection services)
        {
            services.AddScoped<IAsyncDistributedLock, MemoryDistributedLock>();
        }

        public void Validate(ILockOptions options)
        {

        }
    }
}
