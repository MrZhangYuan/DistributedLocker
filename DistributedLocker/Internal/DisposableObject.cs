using System;
using System.Threading.Tasks;

namespace DistributedLocker.Internal
{
    internal class DisposableObject : IDisposable, IAsyncDisposable
    {
        private EventHandler _disposing;

        ~DisposableObject()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsDisposed
        {
            get;
            private set;
        }

        public event EventHandler Disposing
        {
            add
            {
                this.ThrowIfDisposed();
                this._disposing += value;
            }
            remove
            {
                this._disposing -= value;
            }
        }

        protected void ThrowIfDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        protected void Dispose(bool disposing)
        {
            if (this.IsDisposed)
            {
                return;
            }

            try
            {
                if (disposing)
                {
                    this._disposing?.Invoke((object)this, EventArgs.Empty);
                    this._disposing = (EventHandler)null;
                    this.DisposeManagedResources();
                }
                this.DisposeNativeResources();
            }
            finally
            {
                this.IsDisposed = true;
            }
        }

        protected virtual void DisposeManagedResources()
        {
        }

        protected virtual void DisposeNativeResources()
        {
        }

        protected virtual ValueTask DisposeAsyncCore()
        {
            return UtilMethods.DefaultValueTask();
        }

        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsyncCore();

            Dispose(false);

            GC.SuppressFinalize(this);
        }
    }
}
