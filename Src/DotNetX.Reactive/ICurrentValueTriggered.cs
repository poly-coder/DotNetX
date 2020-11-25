namespace DotNetX.Reactive
{
    public interface ICurrentValueTriggered<T> : IValueTriggered<T>
    {
        T? Value { get; }
    }

}
