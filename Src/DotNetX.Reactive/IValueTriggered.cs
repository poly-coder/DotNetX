using System;

namespace DotNetX.Reactive
{
    public interface IValueTriggered<T>
    {
        IObservable<T> Stream { get; }
    }

}
