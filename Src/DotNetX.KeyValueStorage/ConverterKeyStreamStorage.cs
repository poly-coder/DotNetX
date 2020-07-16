using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.KeyValueStorage
{
    public class ConverterKeyStreamStorage<TKeyOut, TKeyPrefixOut, TMetaOut, TKeyIn, TKeyPrefixIn, TMetaIn> :
        IKeyStreamStorage<TKeyOut, TKeyPrefixOut, TMetaOut>
    {
        private readonly IKeyStreamStorage<TKeyIn, TKeyPrefixIn, TMetaIn> innerStorage;
        private readonly Func<TKeyOut, Task<TKeyIn>> toKeyIn;
        private readonly Func<TKeyIn, Task<TKeyOut>> toKeyOut;
        private readonly Func<TKeyPrefixOut, Task<TKeyPrefixIn>> toKeyPrefixIn;
        private readonly Func<TKeyPrefixIn, Task<TKeyPrefixOut>> toKeyPrefixOut;
        private readonly Func<TMetaOut, Task<TMetaIn>> toMetaIn;
        private readonly Func<TMetaIn, Task<TMetaOut>> toMetaOut;

        public ConverterKeyStreamStorage(
            IKeyStreamStorage<TKeyIn, TKeyPrefixIn, TMetaIn> innerStorage,
            Func<TKeyOut, Task<TKeyIn>> toKeyIn,
            Func<TKeyIn, Task<TKeyOut>> toKeyOut,
            Func<TKeyPrefixOut, Task<TKeyPrefixIn>> toKeyPrefixIn,
            Func<TKeyPrefixIn, Task<TKeyPrefixOut>> toKeyPrefixOut,
            Func<TMetaOut, Task<TMetaIn>> toMetaIn,
            Func<TMetaIn, Task<TMetaOut>> toMetaOut)
        {
            this.innerStorage = innerStorage ?? throw new ArgumentNullException(nameof(innerStorage));
            this.toKeyIn = toKeyIn ?? throw new ArgumentNullException(nameof(toKeyIn));
            this.toKeyOut = toKeyOut ?? throw new ArgumentNullException(nameof(toKeyOut));
            this.toKeyPrefixIn = toKeyPrefixIn ?? throw new ArgumentNullException(nameof(toKeyPrefixIn));
            this.toKeyPrefixOut = toKeyPrefixOut ?? throw new ArgumentNullException(nameof(toKeyPrefixOut));
            this.toMetaIn = toMetaIn ?? throw new ArgumentNullException(nameof(toMetaIn));
            this.toMetaOut = toMetaOut ?? throw new ArgumentNullException(nameof(toMetaOut));
        }

        public async Task CleanAsync(CancellationToken cancellationToken)
        {
            await innerStorage.CleanAsync(cancellationToken);
        }

        public async Task CleanAsync(TKeyPrefixOut keyPrefix, CancellationToken cancellationToken)
        {
            await innerStorage.CleanAsync(await toKeyPrefixIn(keyPrefix), cancellationToken);
        }

        public async Task DeleteAsync(TKeyOut key, CancellationToken cancellationToken)
        {
            await innerStorage.DeleteAsync(await toKeyIn(key), cancellationToken);
        }

        public async Task<bool> ExistsAsync(TKeyOut key, CancellationToken cancellationToken)
        {
            return await innerStorage.ExistsAsync(await toKeyIn(key), cancellationToken);
        }

        public async Task<Optional<TMetaOut>> FetchMetadataAsync(TKeyOut key, CancellationToken cancellationToken)
        {
            var metaIn = await innerStorage.FetchMetadataAsync(await toKeyIn(key), cancellationToken);

            return await metaIn.MapAsync(toMetaOut);

        }

        public IAsyncEnumerable<(TKeyOut key, TMetaOut meta)> ListAsync(CancellationToken cancellationToken)
        {
            return innerStorage
                .ListAsync(cancellationToken)
                .SelectAwait(async pairIn => (await toKeyOut(pairIn.key), await toMetaOut(pairIn.meta)));
        }

        public async IAsyncEnumerable<(TKeyOut key, TMetaOut meta)> ListAsync(TKeyPrefixOut keyPrefix, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in ListAsyncAux(await toKeyPrefixIn(keyPrefix), cancellationToken))
            {
                yield return item;
            }
        }

        private IAsyncEnumerable<(TKeyOut key, TMetaOut meta)> ListAsyncAux(TKeyPrefixIn keyPrefix, CancellationToken cancellationToken)
        {
            return innerStorage
                .ListAsync(keyPrefix, cancellationToken)
                .SelectAwait(async pairIn => (await toKeyOut(pairIn.key), await toMetaOut(pairIn.meta)));
        }

        public IAsyncEnumerable<TKeyOut> ListKeysAsync(CancellationToken cancellationToken)
        {
            return innerStorage
                .ListKeysAsync(cancellationToken)
                .SelectAwait(async keyIn => await toKeyOut(keyIn));
        }

        public async IAsyncEnumerable<TKeyOut> ListKeysAsync(TKeyPrefixOut keyPrefix, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in ListKeysAsyncAux(await toKeyPrefixIn(keyPrefix), cancellationToken))
            {
                yield return item;
            }
        }

        private IAsyncEnumerable<TKeyOut> ListKeysAsyncAux(TKeyPrefixIn keyPrefix, CancellationToken cancellationToken)
        {
            return innerStorage
                .ListKeysAsync(keyPrefix, cancellationToken)
                .SelectAwait(async keyIn => await toKeyOut(keyIn));
        }

        public async Task<T> OpenReadAsync<T>(TKeyOut key, Func<Stream, Task<T>> onStream, CancellationToken cancellationToken)
        {
            return await innerStorage.OpenReadAsync(await toKeyIn(key), onStream, cancellationToken);
        }

        public async Task OpenReadAsync(TKeyOut key, Func<Stream, Task> onStream, CancellationToken cancellationToken)
        {
            await innerStorage.OpenReadAsync(await toKeyIn(key), onStream, cancellationToken);
        }

        public async Task UploadAsync(TKeyOut key, TMetaOut metadata, Stream stream, CancellationToken cancellationToken)
        {
            await innerStorage.UploadAsync(await toKeyIn(key), await toMetaIn(metadata), stream, cancellationToken);
        }
    }
}
