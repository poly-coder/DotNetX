using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DotNetX.Reactive
{
    public class Property<T> : ICurrentValueTriggered<T>, IUpdatableElement, IDisposable
    {
        private Disposables disposables = new Disposables();
        private BehaviorSubject<T> stream;
        private bool disposedValue;
        private Func<T, T, T>? coerceValue;

        public Property(T initialValue, Func<T, T, T>? coerceValue = null, IEqualityComparer<T>? comparer = null)
        {
            this.Value = initialValue;

            this.coerceValue = coerceValue;

            stream = new BehaviorSubject<T>(CoerceValue(initialValue));

            ValueChanged = stream
                .DistinctUntilChanged(comparer.OrDefault())
                .Select(_ => Unit.Default);

            disposables.Add(stream.Subscribe(value => Value = value));

            disposables.Add(new Disposable(() => stream.OnCompleted()));
        }

        public T Value { get; private set; }

        public IObservable<T> Stream => stream.AsObservable();

        public void Set(T value)
        {
            stream.OnNext(CoerceValue(value));
        }

        public IObservable<Unit> ValueChanged { get; }

        protected T CoerceValue(T value)
        {
            if (coerceValue != null)
            {
                return coerceValue(value, Value);
            }
            return value;
        }

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
            return $"Property<{typeof(T).Name}>({Value})";
        }
    }

}
