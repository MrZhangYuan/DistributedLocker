using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedLocker.Redis
{
    public class RedisDistributedLock : CachedDistributedLock
    {
        private readonly ILockOptions _options = null;

        public RedisDistributedLock(ILockOptions options)
            : base(options)
        {
            _options = options;
        }

        
    }
}
