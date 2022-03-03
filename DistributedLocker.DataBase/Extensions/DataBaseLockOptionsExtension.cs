using DistributedLocker.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedLocker.DataBase.Extensions
{
    public class DataBaseLockOptionsExtension : ILockOptionsExtension
    {
        private string _connectionString = string.Empty;

        public string ConnectionString
            => this._connectionString;

        public DataBaseLockOptionsExtension WithConnectionString(string connstr)
        {
            this._connectionString = connstr;
            return this;
        }

        public virtual void ApplyServices(IServiceCollection services)
        {
            services.AddScoped<IAsyncDistributedLock, DatabaseDistributedLock>();
        }

        public virtual void Validate(ILockOptions options)
        {

        }
    }
}
