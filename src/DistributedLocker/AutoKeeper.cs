using DistributedLocker.Internal;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public class AutoKeeper : DisposableObject, IAutoKeeper
    {
        private readonly ILockOptions _options = null;

        private readonly ConcurrentQueue<IAsyncLockScope> _scopers = new ConcurrentQueue<IAsyncLockScope>();

        public AutoKeeper(ILockOptions options)
        {
            _options = options;
        }

        public void AddLockScope(IAsyncLockScope scope)
        {
            _scopers.Enqueue(scope);
            this.EnsureStarted();
        }


        private volatile int _started = 0;

        private void EnsureStarted()
        {
            if (Interlocked.CompareExchange(ref _started, 1, 0) == 0)
            {
                
            }
        }


        protected override void DisposeManagedResources()
        {

        }

        protected override ValueTask DisposeAsyncCore()
        {
            this.DisposeManagedResources();

            return UtilMethods.DefaultValueTask();
        }
    }

}
