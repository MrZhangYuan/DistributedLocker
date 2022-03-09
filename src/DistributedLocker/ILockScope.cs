using System;

namespace DistributedLocker
{
    public interface ILockScope : IDisposable, IAsyncDisposable
    {
        Lockey Lockey { get; }
        Locker Locker { get; }
        LockParameter Parameter { get; }
        void Keep(TimeSpan span);
        void Keep();
        void Exit();
        void AutoKeep();
        void AutoKeep(TimeSpan span);
    }

}
