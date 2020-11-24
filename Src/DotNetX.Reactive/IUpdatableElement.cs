using System;
using System.Reactive;

namespace DotNetX.Reactive
{
    public interface IUpdatableElement
    {
        IObservable<Unit> ValueChanged { get; }
    }

}
