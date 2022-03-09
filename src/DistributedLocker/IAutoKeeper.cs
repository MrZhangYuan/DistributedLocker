using System;

namespace DistributedLocker
{
    public interface IAutoKeeper
    {
        void AddLockScope(IAsyncLockScope scope);
        void AddLockScope(IAsyncLockScope scope, TimeSpan? span);
        void RemoveScope(IAsyncLockScope scope);
    }

}
