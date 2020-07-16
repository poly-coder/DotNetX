using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.KeyValueStorage
{
    public class FileMetadata
    {
        public string FilePath { get; set; }

        public IDictionary<string, string> Metadata { get; set; }

        public string ContentLanguage { get; set; }
        public string ContentDisposition { get; set; }
        public string ContentEncoding { get; set; }
        public byte[] ContentHash { get; set; }
        public long? ContentLength { get; set; }
        public string ContentType { get; set; }

        public string ETag { get; set; }
    }

    public interface IFileLister
    {
        IAsyncEnumerable<FileMetadata> ListFilesAsync(string subPath, CancellationToken cancellationToken);

        IAsyncEnumerable<FileMetadata> ListFilesAsync(CancellationToken cancellationToken);
    }

    public interface IFileReader
    {
        Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken);

        Task<FileMetadata> GetFileMetadataAsync(string filePath, CancellationToken cancellationToken);

        Task<T> ReadFileAsync<T>(string filePath,
            Func<Stream, FileMetadata, Task<T>> process,
            Func<Task<T>> onNotFound,
            CancellationToken cancellationToken);

        Task ReadFileAsync<T>(string filePath,
            Func<Stream, FileMetadata, Task> process,
            Func<Task> onNotFound,
            CancellationToken cancellationToken);
    }

    public interface IFileStorer
    {
        Task<FileMetadata> StoreFileAsync(FileMetadata metadata, Stream stream, CancellationToken cancellationToken);

        Task<FileMetadata> PushFileAsync(FileMetadata metadata, Func<Stream, Task> pushStream, CancellationToken cancellationToken);
    }

    public interface IFileCleaner
    {
        Task CleanFilesAsync(string subPath, CancellationToken cancellationToken);

        Task CleanFilesAsync(CancellationToken cancellationToken);
    }

    public interface IFileStorage :
        IFileLister,
        IFileReader,
        IFileStorer,
        IFileCleaner
    {

    }
}
