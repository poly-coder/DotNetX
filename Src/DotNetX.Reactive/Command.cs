using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DotNetX.Reactive
{
    public class Command<T> : IValueTriggered<T>, IDisposable
    {
        private Subject<T> commandStream =
            new Subject<T>();
        private bool disposedValue;

        public IObservable<T> Stream =>
            commandStream.AsObservable();

        public void CallWith(T command)
        {
            commandStream.OnNext(command);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    commandStream.OnCompleted();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return $"Command<{typeof(T).Name}>()";
        }
    }

}
