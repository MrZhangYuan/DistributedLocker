using DistributedLocker.Extensions;
using DistributedLocker.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DistributedLocker
{
    public class DistributedLockContext : DisposableObject
    {
        private readonly IAsyncDistributedLock _distributedLock;

        private readonly ILockOptions _options = null;

        private readonly IServiceProvider _internalProvider;

        private readonly IAutoKeeper _autoKeeper;

        public DistributedLockContext(ILockOptions options)
        {
            UtilMethods.ThrowIfNull(options, nameof(options));

            _options = options;

            _internalProvider = ProviderFactory
                                .GetProvider(options);

            _distributedLock = _internalProvider
                                .GetRequiredService<IAsyncDistributedLock>();

            _autoKeeper = _internalProvider
                            .GetRequiredService<IAutoKeeper>();

            UtilMethods.ThrowIfNull(_distributedLock, nameof(_distributedLock));
        }

        private class DistributedLockScope : DisposableObject, IAsyncLockScope
        {
            private readonly DistributedLockContext _context;
            private readonly Lockey _lockey;
            private readonly Locker _locker;
            private readonly LockParameter _parameter;

            public LockParameter Parameter => this._parameter;
            public Lockey Lockey => _lockey;
            public Locker Locker => _locker;

            public DistributedLockScope(DistributedLockContext context,
                Lockey lockey,
                Locker locker,
                LockParameter parameter)
            {
                UtilMethods.ThrowIfNull(context, nameof(context));
                UtilMethods.ThrowIfNull(lockey, nameof(lockey));
                UtilMethods.ThrowIfNull(locker, nameof(locker));

                _context = context;
                _lockey = lockey;
                _locker = locker;
                _parameter = parameter;
            }

            public void Exit()
            {
                this._context.End(this._lockey);
            }

            public async ValueTask ExitAsync()
            {
                await this._context.EndAsync(this._lockey);
            }

            public void Keep(TimeSpan span)
            {
                this._context.Keep(this._lockey, span);
            }

            public void Keep()
            {
                this._context.Keep(this._lockey);
            }

            public async ValueTask KeepAsync(TimeSpan span)
            {
                await this._context.KeepAsync(this._lockey, span);
            }

            public async ValueTask KeepAsync()
            {
                await this._context.KeepAsync(this._lockey);
            }

            public void AutoKeep()
            {
                this._context.AutoKeep(this);
            }
            public void AutoKeep(TimeSpan span)
            {
                this._context.AutoKeep(this, span);
            }

            protected override async ValueTask DisposeAsyncCore()
            {
                this._context._autoKeeper.RemoveScope(this);
                await this._context.EndAsync(this._lockey);
            }

            protected override void DisposeManagedResources()
            {
                base.DisposeManagedResources();

                this._context._autoKeeper.RemoveScope(this);
                this._context.End(this._lockey);
            }
        }


        private IAsyncLockScope CreateScope(Lockey lockey, Locker locker, LockParameter param)
        {
            var scope = new DistributedLockScope(
                        this,
                        lockey,
                        locker,
                        param);

            if (_options.FindExtension<CoreLockOptionsExtension>()?.AutoKeep == true)
            {
                this._autoKeeper.AddLockScope(scope);
            }

            return scope;
        }

        public async ValueTask<IAsyncLockScope> BeginAsync(Lockey lockey, LockParameter parameter)
        {
            var locker = await this._distributedLock.EnterAsync(lockey, parameter);

            return CreateScope(
                    lockey,
                    locker,
                    parameter);
        }

        private async ValueTask KeepAsync(Lockey lockey, TimeSpan span)
        {
            await this._distributedLock.KeepAsync(lockey, span);
        }

        private async ValueTask KeepAsync(Lockey lockey)
        {
            await this._distributedLock.KeepAsync(lockey);
        }

        public async ValueTask EndAsync(Lockey lockey)
        {
            await this._distributedLock.ExitAsync(lockey);
        }



        public ILockScope Begin(Lockey lockey, LockParameter parameter)
        {
            var locker = this._distributedLock.Enter(lockey, parameter);

            return CreateScope(
                    lockey,
                    locker,
                    parameter);
        }

        public bool TryBegin(Lockey lockey,
            LockParameter parameter,
            out ILockScope scope)
        {
            if (this._distributedLock.TryEnter(lockey, parameter, out Locker locker))
            {
                scope = CreateScope(
                        lockey,
                        locker,
                        parameter);

                return true;
            }

            scope = null;
            return false;
        }

        private void Keep(Lockey lockey, TimeSpan span)
        {
            this._distributedLock.Keep(lockey, span);
        }

        private void Keep(Lockey lockey)
        {
            this._distributedLock.Keep(lockey);
        }

        public void End(Lockey lockey)
        {
            this._distributedLock.Exit(lockey);
        }

        private void AutoKeep(IAsyncLockScope scope)
        {
            this._autoKeeper.AddLockScope(scope);
        }

        private void AutoKeep(IAsyncLockScope scope, TimeSpan span)
        {
            this._autoKeeper.AddLockScope(scope, span);
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();

            if (_internalProvider is IDisposable provider)
            {
                provider.Dispose();
            }
        }

        protected override async ValueTask DisposeAsyncCore()
        {
            await base.DisposeAsyncCore();

            if (_internalProvider is IAsyncDisposable provider)
            {
                await provider.DisposeAsync();
            }
        }
    }

}
