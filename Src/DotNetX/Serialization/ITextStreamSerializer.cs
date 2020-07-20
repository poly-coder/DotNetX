using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Serialization
{
    public interface ITextStreamSerializer
    {
        Task SerializeAsync<T>(TextWriter writer, T value, CancellationToken cancellationToken);
        
        Task<T> DeserializeAsync<T>(TextReader reader, CancellationToken cancellationToken);
    }
}
