namespace DotNetX.Plugins
{
    public interface IInitializer<T>
    {
        void Initialize(T context);
    }
}
