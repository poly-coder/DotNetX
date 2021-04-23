using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetX
{
    public class Disposables : IDisposable
    {
        private bool disposedValue;
        private readonly List<IDisposable> disposables = new List<IDisposable>();

        public Disposables Add(IDisposable disposable)
        {
            if (disposable is null)
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            this.disposables.Add(disposable);

            return this;
        }

        public Disposables Add(IEnumerable<IDisposable> disposables)
        {
            if (disposables is null)
            {
                throw new ArgumentNullException(nameof(disposables));
            }

            this.disposables.AddRange(disposables.Where(d =>
            {
                if (d is null)
                {
                    throw new ArgumentNullException(nameof(disposables), 
                        "You cannot pass a null disposable");
                }
                return !this.disposables.Contains(d);
            }));

            return this;
        }

        public Disposables Add(params IDisposable[] disposables) =>
            Add(disposables as IEnumerable<IDisposable>);

        public Disposables Add(Action dispose) =>
            Add(new Disposable(dispose));

        public Disposables Add(IEnumerable<Action> disposeActions) =>
            Add(disposeActions.Select(d => new Disposable(d)));

        public Disposables Add(params Action[] disposeActions) =>
            Add(disposeActions as IEnumerable<Action>);

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
