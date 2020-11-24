using System;
using System.Reactive;
using System.Reactive.Subjects;

namespace DotNetX.Reactive
{
    public class Computed<T> : ICurrentValueTriggered<T>, IUpdatableElement, IDisposable
    {
        private Disposables disposables = new Disposables();
        private Subject<Unit> valueChanged = new Subject<Unit>();
        private bool disposedValue;

        public Computed(IObservable<T> valueStream)
        {
            valueChanged = new Subject<Unit>();
            Stream = valueStream;

            disposables.Add(Stream.Subscribe(value =>
            {
                Value = value;
                valueChanged.OnNext(Unit.Default);
            }));

            disposables.Add(new Disposable(() => valueChanged.OnCompleted()));
        }

        public T Value { get; private set; }
        public IObservable<T> Stream { get; }

        public IObservable<Unit> ValueChanged => valueChanged.AsObservable();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposables.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return $"Computed<{typeof(T).Name}>({Value})";
        }
    }

}
