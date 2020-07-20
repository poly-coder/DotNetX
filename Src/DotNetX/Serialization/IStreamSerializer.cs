using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Serialization
{
    public interface IStreamSerializer
    {
        Task SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken);
        
        Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken);
    }
}
