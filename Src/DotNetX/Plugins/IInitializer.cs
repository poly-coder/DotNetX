namespace DotNetX.Plugins
{
    public interface IInitializer
    {
        void Initialize();
    }

    public interface IInitializer<T>
    {
        void Initialize(T context);
    }
}
