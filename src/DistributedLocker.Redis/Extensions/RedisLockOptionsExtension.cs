using DistributedLocker.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedLocker.Redis.Extensions
{
    public class RedisLockOptionsExtension : ILockOptionsExtension
    {
        private string _connectionString = string.Empty;
        private int _dbNum = 0;

        public int DbNum
        {
            get => _dbNum;
        }
        public string ConnectionString
        {
            get => _connectionString;
        }

        public RedisLockOptionsExtension WithConnectionString(string connstr)
        {
            this._connectionString = connstr;
            return this;
        }

        public RedisLockOptionsExtension WithDbNum(int dbnum)
        {
            this._dbNum = dbnum;
            return this;
        }

        public void ApplyServices(IServiceCollection services)
        {
            services.AddScoped<IAsyncDistributedLock, RedisDistributedLock>();
        }

        public void Validate(ILockOptions options)
        {

        }
    }
}
