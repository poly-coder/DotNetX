using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.KeyValueStorage.Azure.Blobs
{
    public class AzureBlobsFileStorageOptions
    {
        public string ConnectionString { get; set; }
        public string Container { get; set; }
        public bool Create { get; set; } = true;
        public PublicAccessType PublicAccessType { get; set; } = PublicAccessType.None;
    }

    public class AzureBlobsFileStorage : IFileStorage
    {
        public AzureBlobsFileStorage(IOptions<AzureBlobsFileStorageOptions> options)
        {
            var opts = options.Value;

            this.LazyContainer = new Lazy<Task<BlobContainerClient>>(async () =>
            {
                var service = new BlobServiceClient(opts.ConnectionString);

                var container = service.GetBlobContainerClient(opts.Container);

                if (opts.Create)
                {
                    await container.CreateIfNotExistsAsync(
                        publicAccessType: opts.PublicAccessType);
                }

                return container;
            });
        }

        private Lazy<Task<BlobContainerClient>> LazyContainer { get; }

        public async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken)
        {
            var container = await LazyContainer.Value;

            var response = await container.GetBlobClient(filePath).ExistsAsync(cancellationToken);

            return response.Value;
        }

        public async Task<FileMetadata> GetFileMetadataAsync(string filePath, CancellationToken cancellationToken)
        {
            var container = await LazyContainer.Value;

            try
            {
                var response = await container.GetBlobClient(filePath).GetPropertiesAsync(null, cancellationToken);

                var props = response.Value;

                return GetMetadata(filePath, response.Value);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async IAsyncEnumerable<FileMetadata> ListFilesAsync(string subPath, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var container = await LazyContainer.Value;

            var paginator = container.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, subPath, cancellationToken);

            await foreach (var page in paginator.AsPages())
            {
                foreach (var item in page.Values)
                {
                    yield return GetMetadata(item.Name, item.Properties, item.Metadata);
                }
            }
        }

        public async IAsyncEnumerable<FileMetadata> ListFilesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var container = await LazyContainer.Value;

            var paginator = container.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, null, cancellationToken);

            await foreach (var page in paginator.AsPages())
            {
                foreach (var item in page.Values)
                {
                    yield return GetMetadata(item.Name, item.Properties, item.Metadata);
                }
            }
        }

        public async Task<T> ReadFileAsync<T>(string filePath, Func<Stream, FileMetadata, Task<T>> process, Func<Task<T>> onNotFound, CancellationToken cancellationToken)
        {
            var container = await LazyContainer.Value;

            try
            {
                var blob = container.GetBlobClient(filePath);

                var props = await blob.GetPropertiesAsync(null, cancellationToken);

                var metadata = GetMetadata(filePath, props.Value);

                var response = await blob.DownloadAsync(cancellationToken);

                using var stream = response.Value.Content;

                return await process(stream, metadata);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return await onNotFound();
            }
        }

        public Task ReadFileAsync<T>(string filePath, Func<Stream, FileMetadata, Task> process, Func<Task> onNotFound, CancellationToken cancellationToken)
        {
            return ReadFileAsync<bool>(filePath,
                async (stream, metadata) =>
                {
                    await process(stream, metadata);
                    return true;
                },
                async () =>
                {
                    await onNotFound();
                    return false;
                },
                cancellationToken);
        }

        public async Task<FileMetadata> StoreFileAsync(FileMetadata metadata, Stream stream, CancellationToken cancellationToken)
        {
            var container = await LazyContainer.Value;

            var blob = container.GetBlobClient(metadata.FilePath);

            await blob.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentDisposition = metadata.ContentDisposition,
                ContentEncoding = metadata.ContentEncoding,
                ContentType = metadata.ContentType,
                ContentHash = metadata.ContentHash,
                ContentLanguage = metadata.ContentLanguage,
            },
            metadata.Metadata,
            cancellationToken: cancellationToken);

            var props = await blob.GetPropertiesAsync(null, cancellationToken);

            var metadataOut = GetMetadata(metadata.FilePath, props.Value);

            return metadataOut;
        }

        public async Task<FileMetadata> PushFileAsync(FileMetadata metadata, Func<Stream, Task> pushStream, CancellationToken cancellationToken)
        {
            // TODO: Optimize
            // https://github.com/AArnott/Nerdbank.Streams
            // System.IO.Pipelines

            using var memoryStream = new MemoryStream();

            await pushStream(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return await StoreFileAsync(metadata, memoryStream, cancellationToken);
        }

        public async Task CleanFilesAsync(string subPath, CancellationToken cancellationToken)
        {
            var container = await LazyContainer.Value;

            var paginator = container.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, subPath, cancellationToken);

            var items = paginator.AsPages().SelectMany(page => page.Values.ToAsyncEnumerable());

            // TODO: Parallelism ???
            await items.ForEachAwaitAsync(async item =>
            {
                await container.DeleteBlobIfExistsAsync(item.Name, cancellationToken: cancellationToken);
            }, cancellationToken);
        }

        public async Task CleanFilesAsync(CancellationToken cancellationToken)
        {
            var container = await LazyContainer.Value;

            var paginator = container.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, null, cancellationToken);

            var items = paginator.AsPages().SelectMany(page => page.Values.ToAsyncEnumerable());

            await items.ForEachAwaitAsync(async item =>
            {
                await container.DeleteBlobIfExistsAsync(item.Name, cancellationToken: cancellationToken);
            }, cancellationToken);
        }

        private static FileMetadata GetMetadata(string filePath, BlobProperties props)
        {
            return new FileMetadata
            {
                Metadata = props.Metadata,
                FilePath = filePath,
                ContentType = props.ContentType,
                ContentDisposition = props.ContentDisposition,
                ContentEncoding = props.ContentEncoding,
                ContentHash = props.ContentHash,
                ContentLanguage = props.ContentLanguage,
                ContentLength = props.ContentLength,
                ETag = props.ETag.ToString(),
            };
        }

        private static FileMetadata GetMetadata(string filePath, BlobItemProperties props, IDictionary<string, string> metadata)
        {
            return new FileMetadata
            {
                Metadata = metadata,
                FilePath = filePath,
                ContentType = props.ContentType,
                ContentDisposition = props.ContentDisposition,
                ContentEncoding = props.ContentEncoding,
                ContentHash = props.ContentHash,
                ContentLanguage = props.ContentLanguage,
                ContentLength = props.ContentLength,
                ETag = props.ETag.ToString(),
            };
        }

    }
}
