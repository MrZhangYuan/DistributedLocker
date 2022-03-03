using System;

namespace DistributedLocker
{
    public interface ILockScope : IDisposable, IAsyncDisposable
    {
        LockParameter Parameter { get; }
        void Keep(TimeSpan span);
        void Exit();
    }

}
