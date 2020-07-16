using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.KeyValueStorage.Azure.Blobs
{
    public class AzureBlobKeyStreamStorage :
        IKeyStreamStorage<string, string, IDictionary<string, string>>
    {
        private readonly Lazy<Task<BlobContainerClient>> lazyContainer;
        private readonly AzureBlobKeyStreamStorageOptions options;

        public Task<BlobContainerClient> Container => lazyContainer.Value;

        public AzureBlobKeyStreamStorage(AzureBlobKeyStreamStorageOptions options)
        {
            this.options = options;
            lazyContainer = LazyFn.Create(this.CreateContainer, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public async IAsyncEnumerable<(string key, IDictionary<string, string> meta)> ListAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var container = await Container;

            var response = container.GetBlobsAsync(traits: BlobTraits.Metadata, cancellationToken: cancellationToken);

            await foreach (var page in response.AsPages())
            {
                foreach (var blobItem in page.Values)
                {
                    yield return (blobItem.Name, blobItem.Metadata);
                }
            }
        }

        public async IAsyncEnumerable<string> ListKeysAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var container = await Container;

            var response = container.GetBlobsAsync(traits: BlobTraits.None, cancellationToken: cancellationToken);

            await foreach (var page in response.AsPages())
            {
                foreach (var blobItem in page.Values)
                {
                    yield return blobItem.Name;
                }
            }
        }

        public async IAsyncEnumerable<(string key, IDictionary<string, string> meta)> ListAsync(
            string keyPrefix, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var container = await Container;

            var response = container.GetBlobsAsync(
                prefix: keyPrefix,
                traits: BlobTraits.Metadata,
                cancellationToken: cancellationToken);

            await foreach (var page in response.AsPages())
            {
                foreach (var blobItem in page.Values)
                {
                    yield return (blobItem.Name, blobItem.Metadata);
                }
            }
        }

        public async IAsyncEnumerable<string> ListKeysAsync(
            string keyPrefix,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var container = await Container;

            var response = container.GetBlobsAsync(
                prefix: keyPrefix,
                traits: BlobTraits.None,
                cancellationToken: cancellationToken);

            await foreach (var page in response.AsPages())
            {
                foreach (var blobItem in page.Values)
                {
                    yield return blobItem.Name;
                }
            }
        }


        public async Task CleanAsync(CancellationToken cancellationToken)
        {
            var container = await Container;

            var response = container.GetBlobsAsync(
                traits: BlobTraits.None,
                cancellationToken: cancellationToken);

            await foreach (var page in response.AsPages())
            {
                foreach (var blobItem in page.Values)
                {
                    await container.DeleteBlobIfExistsAsync(blobItem.Name, cancellationToken: cancellationToken);
                }
            }
        }

        public async Task CleanAsync(string keyPrefix, CancellationToken cancellationToken)
        {
            var container = await Container;

            var response = container.GetBlobsAsync(
                prefix: keyPrefix,
                traits: BlobTraits.None,
                cancellationToken: cancellationToken);

            await foreach (var page in response.AsPages())
            {
                foreach (var blobItem in page.Values)
                {
                    await container.DeleteBlobIfExistsAsync(blobItem.Name, cancellationToken: cancellationToken);
                }
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken)
        {
            var container = await Container;

            var blobClient = container.GetBlobClient(key);

            var response = await blobClient.ExistsAsync(cancellationToken: cancellationToken);

            return response.Value;
        }

        public async Task<Optional<IDictionary<string, string>>> FetchMetadataAsync(string key, CancellationToken cancellationToken)
        {
            var container = await Container;

            var blobClient = container.GetBlobClient(key);

            try
            {
                var response = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

                return Optional.Some(response.Value.Metadata);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return Optional.None<IDictionary<string, string>>();
            }
        }

        public async Task<T> OpenReadAsync<T>(string key, Func<Stream, Task<T>> onStream, CancellationToken cancellationToken)
        {
            var container = await Container;

            var blobClient = container.GetBlobClient(key);

            try
            {
                var response = await blobClient.DownloadAsync(cancellationToken);

                using (var info = response.Value)
                {
                    return await onStream(info.Content);
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new KeyNotFoundException($"Key '{key}' not found");
            }
        }

        public Task OpenReadAsync(string key, Func<Stream, Task> onStream, CancellationToken cancellationToken)
        {
            return OpenReadAsync(key, async stream =>
            {
                await onStream(stream);
                return true;
            }, cancellationToken);
        }

        public async Task UploadAsync(string key, IDictionary<string, string> metadata, Stream stream, CancellationToken cancellationToken)
        {
            var container = await Container;

            var blobClient = container.GetBlobClient(key);

            await blobClient.UploadAsync(stream, metadata: metadata, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken)
        {
            var container = await Container;

            await container.DeleteBlobIfExistsAsync(key, cancellationToken: cancellationToken);
        }


        private async Task<BlobContainerClient> CreateContainer()
        {
            var client = new BlobContainerClient(options.ConnectionString, options.ContainerName);

            await client.CreateIfNotExistsAsync(publicAccessType: options.PublicAccessType);

            return client;
        }
    }

    public class AzureBlobKeyStreamStorageOptions
    {
        public string? ConnectionString { get; set; }
        public string? ContainerName { get; set; }
        public PublicAccessType PublicAccessType { get; set; } = PublicAccessType.None;
    }


}
