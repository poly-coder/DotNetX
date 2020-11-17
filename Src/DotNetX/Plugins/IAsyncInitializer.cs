using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Plugins
{
    public interface IAsyncInitializer<T>
    {
        Task InitializeAsync(T context, CancellationToken cancellationToken);
    }
}
