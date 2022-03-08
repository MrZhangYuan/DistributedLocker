using Microsoft.Extensions.DependencyInjection;

namespace DistributedLocker.Extensions
{
    public class MemoryCacherOptionsExtension : ILockOptionsExtension
    {
        public void ApplyServices(IServiceCollection services)
        {
            services.AddSingleton<IDistributedLockCacher, MemoryDistributedLockCacher>();
        }

        public void Validate(ILockOptions options)
        {
        }
    }
}
