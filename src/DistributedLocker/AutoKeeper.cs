using DistributedLocker.Extensions;
using DistributedLocker.Internal;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public class AutoKeeper : DisposableObject, IAutoKeeper
    {
        private class AutoKeeperTask
        {
            private readonly ILockOptions _options = null;
            private readonly IAsyncLockScope _lockScope;
            private bool _flag = false;
            private readonly object _sync = new object();
            private TimeSpan? _keepDuation;

            public AutoKeeperTask(IAsyncLockScope lockScope, ILockOptions options, TimeSpan? keep = null)
            {
                UtilMethods.ThrowIfNull(lockScope, nameof(lockScope));

                _options = options;
                _lockScope = lockScope;
                _keepDuation = keep;
            }

            public AutoKeeperTask Start()
            {
                if (!_flag)
                {
                    lock (this._sync)
                    {
                        if (!_flag)
                        {
                            _flag = true;

                            Task.Factory.StartNew(this.StartCore);
                        }
                    }
                }

                return this;
            }

            public void Stop()
            {
                lock (_sync)
                {
                    if (_flag)
                    {
                        _flag = false;
                    }
                }
            }

            private async Task StartCore()
            {
                //  AutoKeep 时，使用指定的 KeepDuation 或默认的 DefaultKeepDuation
                //  AutoKeep 的时机为 Duation 快结束时
                if (!_keepDuation.HasValue)
                {
                    var coreex = _options.FindExtension<CoreLockOptionsExtension>();
                    _keepDuation = _lockScope.Parameter?.KeepDuation != null
                                    ?
                                    TimeSpan.FromMilliseconds(_lockScope.Parameter.KeepDuation.Value)
                                    :
                                    TimeSpan.FromMilliseconds(coreex.DefaultKeepDuation);
                }

                int? duation = _lockScope.Parameter?.Duation;
                if (!duation.HasValue)
                {
                    duation = _options.FindExtension<CoreLockOptionsExtension>()
                                .DefaultDuation;
                }

                duation = (int)(duation.Value * 0.8);

                if (duation > 0)
                {
                    bool firsttime = true;
                    while (_flag)
                    {
                        await Task.Delay(duation.Value);

                        if (firsttime)
                        {
                            firsttime = false;
                            duation = (int)(_keepDuation.Value.TotalMilliseconds * 0.8);
                        }

                        if (_flag)
                        {
                            Console.WriteLine($"{this._lockScope.Lockey.BusinessType} - {this._lockScope.Lockey.BusinessCode} KeepAsync：" + _keepDuation.Value.TotalMilliseconds);

                            try
                            {
                                await _lockScope.KeepAsync(_keepDuation.Value);
                            }
                            catch (LockExpiredException)
                            {
                                //  有可能用户解锁在前
                                Console.WriteLine("LockExpiredException");
                                break;
                            }
                        }
                    }
                }
            }
        }

        private readonly ConcurrentDictionary<Lockey, AutoKeeperTask> _scopers = new ConcurrentDictionary<Lockey, AutoKeeperTask>();
        private readonly ILockOptions _options = null;

        public AutoKeeper(ILockOptions options)
        {
            _options = options;
        }

        public void AddLockScope(IAsyncLockScope scope, TimeSpan? span)
        {
            var newtask = new AutoKeeperTask(scope, _options, span);

            var task = _scopers.GetOrAdd(scope.Lockey, _k => newtask);

            if (object.ReferenceEquals(newtask, task))
            {
                task.Start();
            }
        }

        public void AddLockScope(IAsyncLockScope scope)
        {
            this.AddLockScope(scope, null);
        }

        public void RemoveScope(IAsyncLockScope scope)
        {
            if (_scopers.TryRemove(scope.Lockey, out var task))
            {
                task.Stop();
            }
        }

        private void EnsureStoped()
        {
            var keys = _scopers.Keys.ToList();
            foreach (var lockey in keys)
            {
                if (_scopers.TryRemove(lockey, out var task))
                {
                    task.Stop();
                }
            }

            if (!_scopers.IsEmpty)
            {
                this.EnsureStoped();
            }
        }

        protected override void DisposeManagedResources()
        {
            this.EnsureStoped();
            this._scopers.Clear();
        }

        protected override ValueTask DisposeAsyncCore()
        {
            this.DisposeManagedResources();

            return UtilMethods.DefaultValueTask();
        }
    }

}
