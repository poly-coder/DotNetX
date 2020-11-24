using System;

namespace DotNetX.Reactive
{
    public interface IReactiveElement : IDisposable
    {
        Command StateChanged { get; }

        TDisposable DeferDispose<TDisposable>(TDisposable disposable)
            where TDisposable : IDisposable;
    }

}
