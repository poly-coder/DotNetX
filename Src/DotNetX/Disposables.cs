using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetX
{
    public class Disposables : IDisposable
    {
        private bool disposedValue;
        private readonly List<IDisposable> disposables = new List<IDisposable>();

        public void Add(params IDisposable[] disposables)
        {
            if (disposables is null)
            {
                throw new ArgumentNullException(nameof(disposables));
            }

            foreach (var item in disposables)
            {
                if (item is null)
                {
                    throw new ArgumentNullException(nameof(disposables), "You cannot pass a null disposable");
                }
            }

            this.disposables.AddRange(disposables.Where(d => !disposables.Contains(d)));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var item in disposables)
                    {
                        item.Dispose();
                    }

                    disposables.Clear();

                    OnDispose();
                }

                disposedValue = true;
            }
        }

        protected virtual void OnDispose()
        {
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
