using System;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public interface IAsyncDistributedLock : IDistributedLock, IAsyncDisposable
    {
        ValueTask<Locker> EnterAsync(Lockey lockey, LockParameter parameter);
        ValueTask<bool> TryEnterAsync(Lockey lockey, LockParameter parameter, out Locker locker);
        ValueTask KeepAsync(Lockey lockey, TimeSpan span);
        ValueTask KeepAsync(Lockey lockey);
        ValueTask ExitAsync(Lockey lockey);
    }
}
