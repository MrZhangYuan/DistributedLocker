using Microsoft.Extensions.DependencyInjection;

namespace DistributedLocker.Extensions
{
    public interface ILockOptionsExtension
    {
        void ApplyServices(IServiceCollection services);
        void Validate(ILockOptions options);
    }
}
