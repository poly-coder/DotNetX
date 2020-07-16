using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.KeyValueStorage
{
    public interface IKeyStreamStorage<TKey, TKeyPrefix, TMeta>
    {
        IAsyncEnumerable<(TKey key, TMeta meta)> ListAsync(CancellationToken cancellationToken);
        IAsyncEnumerable<(TKey key, TMeta meta)> ListAsync(TKeyPrefix keyPrefix, CancellationToken cancellationToken);

        IAsyncEnumerable<TKey> ListKeysAsync(CancellationToken cancellationToken);
        IAsyncEnumerable<TKey> ListKeysAsync(TKeyPrefix keyPrefix, CancellationToken cancellationToken);

        Task CleanAsync(CancellationToken cancellationToken);
        Task CleanAsync(TKeyPrefix keyPrefix, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken);
        Task<Optional<TMeta>> FetchMetadataAsync(TKey key, CancellationToken cancellationToken);
        
        Task<T> OpenReadAsync<T>(TKey key, Func<Stream, Task<T>> onStream, CancellationToken cancellationToken);
        Task OpenReadAsync(TKey key, Func<Stream, Task> onStream, CancellationToken cancellationToken);

        Task UploadAsync(TKey key, TMeta metadata, Stream stream, CancellationToken cancellationToken);

        Task DeleteAsync(TKey key, CancellationToken cancellationToken);
    }
}
