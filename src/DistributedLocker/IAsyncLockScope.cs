using System;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public interface IAsyncLockScope : ILockScope
    {
        ValueTask KeepAsync(TimeSpan span);
        ValueTask ExitAsync();
    }

}
