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


        internal enum ScopeState
        {
            Created,
            Entered
        }


        private class DistributedLockScope : DisposableObject, IAsyncLockScope
        {
            private readonly DistributedLockContext _context;
            private readonly Lockey _lockey;
            private readonly Locker _locker;
            private readonly LockParameter _parameter;
            internal ScopeState _scopeState = ScopeState.Created;

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



            private void CheckState()
            {
                if (_scopeState != ScopeState.Entered)
                {
                    throw new ScopeInvalidStateException();
                }
            }



            public void Keep(TimeSpan span)
            {
                this.CheckState();
                this._context.Keep(this._lockey, span);
            }
            public void Keep()
            {
                this.CheckState();
                this._context.Keep(this._lockey);
            }



            public async ValueTask KeepAsync(TimeSpan span)
            {
                this.CheckState();
                await this._context.KeepAsync(this._lockey, span);
            }
            public async ValueTask KeepAsync()
            {
                this.CheckState();
                await this._context.KeepAsync(this._lockey);
            }



            public void AutoKeep()
            {
                this.CheckState();
                this._context._autoKeeper.AddLockScope(this);
            }
            public void AutoKeep(TimeSpan span)
            {
                this.CheckState();
                this._context._autoKeeper.AddLockScope(this, span);
            }



            public void Exit()
            {
                this.CheckState();
                this._context._autoKeeper.RemoveScope(this);
                this._context.End(this._lockey);
            }
            public async ValueTask ExitAsync()
            {
                this.CheckState();
                this._context._autoKeeper.RemoveScope(this);
                await this._context.EndAsync(this._lockey);
            }



            protected override async ValueTask DisposeAsyncCore()
            {
                this._context._autoKeeper.RemoveScope(this);

                if (this._scopeState == ScopeState.Entered)
                {
                    await this._context.EndAsync(this._lockey);
                }
            }
            protected override void DisposeManagedResources()
            {
                base.DisposeManagedResources();

                this._context._autoKeeper.RemoveScope(this);

                if (_scopeState == ScopeState.Entered)
                {
                    this._context.End(this._lockey);
                }
            }
        }


        private IAsyncLockScope CreateScope(Lockey lockey,
            Locker locker,
            LockParameter param,
            ScopeState state = ScopeState.Entered)
        {
            var scope = new DistributedLockScope(
                        this,
                        lockey,
                        locker,
                        param);

            return ChangeScopeState(scope, state);
        }

        private IAsyncLockScope ChangeScopeState(IAsyncLockScope scope, ScopeState state)
        {
            if (state == ScopeState.Entered)
            {
                ((DistributedLockScope)scope)._scopeState = ScopeState.Entered;

                if (scope.Parameter.AutoKeep.HasValue)
                {
                    if (scope.Parameter.AutoKeep == true)
                    {
                        this._autoKeeper.AddLockScope(scope);
                    }
                }
                else if (_options.FindExtension<CoreLockOptionsExtension>()?.AutoKeep == true)
                {
                    this._autoKeeper.AddLockScope(scope);
                }
            }

            return scope;
        }



        public async ValueTask<IAsyncLockScope> BeginAsync(Lockey lockey)
            => await this.BeginAsync(lockey, null);
        public async ValueTask<IAsyncLockScope> BeginAsync(Lockey lockey, LockParameter param)
        {
            param = this._distributedLock.CreatOrSetDefaultParameter(lockey, param);

            var locker = await this._distributedLock.EnterAsync(lockey, param);

            return CreateScope(
                    lockey,
                    locker,
                    param);
        }



        public virtual ValueTask<bool> TryBeginAsync(Lockey lockey, out IAsyncLockScope scope)
        {
            return this.TryBeginAsync(
                    lockey,
                    null,
                    out scope);
        }
        public virtual ValueTask<bool> TryBeginAsync(Lockey lockey,
            LockParameter param,
            out IAsyncLockScope scope)
        {
            param = this._distributedLock.CreatOrSetDefaultParameter(lockey, param);

            ValueTask<bool> task = this._distributedLock.TryEnterAsync(
                                    lockey,
                                    param,
                                    out Locker locker);

            scope = CreateScope(
                    lockey,
                    locker,
                    param,
                    ScopeState.Created);

            return TryBeginAsyncLocal(scope);

            async ValueTask<bool> TryBeginAsyncLocal(IAsyncLockScope scopei)
            {
                var entered = await task;

                if (entered)
                {
                    this.ChangeScopeState(scopei, ScopeState.Entered);
                }

                await UtilMethods.DefaultValueTask();

                return entered;
            }
        }



        private async ValueTask KeepAsync(Lockey lockey, TimeSpan span)
            => await this._distributedLock.KeepAsync(lockey, span);
        private async ValueTask KeepAsync(Lockey lockey)
            => await this._distributedLock.KeepAsync(lockey);



        public async ValueTask EndAsync(Lockey lockey)
            => await this._distributedLock.ExitAsync(lockey);



        public ILockScope Begin(Lockey lockey)
            => this.Begin(lockey, null);
        public ILockScope Begin(Lockey lockey, LockParameter param)
        {
            param = this._distributedLock.CreatOrSetDefaultParameter(lockey, param);

            var locker = this._distributedLock.Enter(lockey, param);

            return CreateScope(
                    lockey,
                    locker,
                    param);
        }



        public bool TryBegin(Lockey lockey, out ILockScope scope)
            => TryBegin(
                lockey,
                null,
                out scope);
        public bool TryBegin(Lockey lockey,
            LockParameter param,
            out ILockScope scope)
        {
            param = this._distributedLock.CreatOrSetDefaultParameter(lockey, param);

            if (this._distributedLock.TryEnter(lockey, param, out Locker locker))
            {
                scope = CreateScope(
                        lockey,
                        locker,
                        param);

                return true;
            }

            scope = null;
            return false;
        }



        private void Keep(Lockey lockey, TimeSpan span)
            => this._distributedLock.Keep(lockey, span);
        private void Keep(Lockey lockey)
            => this._distributedLock.Keep(lockey);



        public void End(Lockey lockey)
            => this._distributedLock.Exit(lockey);



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
