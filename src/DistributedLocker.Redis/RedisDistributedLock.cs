using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedLocker.Redis
{
    public class RedisDistributedLock : AsyncDistributedLock
    {
        public RedisDistributedLock(ILockOptions options, IDistributedLockCacher cacher)
            : base(options, cacher)
        {

        }

        public override ValueTask ExitAsync(Lockey lockey, Locker locker)
        {
            throw new NotImplementedException();
        }

        public override ValueTask KeepAsync(Lockey lockey, Locker locker, TimeSpan span)
        {
            throw new NotImplementedException();
        }

        protected override Locker Enter(Lockey lockey, Locker locker, LockParameter param)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<Locker> EnterAsync(Lockey lockey, Locker locker, LockParameter parameter)
        {
            throw new NotImplementedException();
        }

        protected override void Exit(Lockey lockey, Locker locker)
        {
            throw new NotImplementedException();
        }

        protected override void Keep(Lockey lockey, Locker locker, TimeSpan span)
        {
            throw new NotImplementedException();
        }

        protected override bool TryEnter(Lockey lockey, Locker locker, LockParameter parameter)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<bool> TryEnterAsync(Lockey lockey, Locker locker, LockParameter parameter)
        {
            throw new NotImplementedException();
        }
    }
}
