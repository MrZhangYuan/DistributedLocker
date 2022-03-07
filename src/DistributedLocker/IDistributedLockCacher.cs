using System;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public interface IDistributedLockCacher
    {
        Locker GetOrEnter(Lockey key, Func<Lockey, Locker> enter);
        void Update(Lockey key, TimeSpan span, Action<Lockey, Locker, TimeSpan> updater);
        void Exit(Lockey lockey, Action<Lockey, Locker> exiter);


        ValueTask<Locker> GetOrEnterAsync(Lockey key, Func<Lockey, ValueTask<Locker>> valueFactory);
        ValueTask UpdateAsync(Lockey key, TimeSpan span, Func<Lockey, Locker, TimeSpan, ValueTask> updater);
        ValueTask ExitAsync(Lockey lockey, Func<Lockey, Locker, ValueTask> exiter);
    }
}
