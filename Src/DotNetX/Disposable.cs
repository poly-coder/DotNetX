using System;

namespace DotNetX
{
    public class Disposable : IDisposable
    {
        private bool disposedValue;
        private readonly Action dispose;
        private readonly Action? disposeUnmanaged;

        public Disposable(Action dispose, Action? disposeUnmanaged = null)
        {
            this.dispose = dispose;
            this.disposeUnmanaged = disposeUnmanaged;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dispose?.Invoke();
                }

                disposeUnmanaged?.Invoke();

                disposedValue = true;
            }
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        ~Disposable()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            if (disposeUnmanaged != null)
            {
                Dispose(disposing: false);
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
