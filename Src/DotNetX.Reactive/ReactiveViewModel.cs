using System;

namespace DotNetX.Reactive
{
    public abstract class ReactiveViewModel : IReactiveElement
    {
        private readonly Disposables disposables = new Disposables();

        private bool disposedValue;

        public Command StateChanged { get; }

        public ReactiveViewModel()
        {
            StateChanged = this.Command();

            // Logging
            //this.Effect(StateChanged, () => Console.WriteLine(this));
        }

        public TDisposable DeferDispose<TDisposable>(TDisposable disposable) where TDisposable : IDisposable
        {
            disposables.Add(disposable);

            return disposable;
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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override string ToString() => this.PrintPropertyValues();
    }

}
