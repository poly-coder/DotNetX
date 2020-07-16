using System;
using System.Threading.Tasks;

namespace DotNetX.KeyValueStorage
{
    public static class KeyStreamStorageExtensions
    {
        public static IKeyStreamStorage<TKeyOut, TKeyPrefixOut, TMetaOut>
            ConvertAsync<TKeyOut, TKeyPrefixOut, TMetaOut, TKeyIn, TKeyPrefixIn, TMetaIn>(
            this IKeyStreamStorage<TKeyIn, TKeyPrefixIn, TMetaIn> innerStorage,
            Func<TKeyOut, Task<TKeyIn>> toKeyIn,
            Func<TKeyIn, Task<TKeyOut>> toKeyOut,
            Func<TKeyPrefixOut, Task<TKeyPrefixIn>> toKeyPrefixIn,
            Func<TKeyPrefixIn, Task<TKeyPrefixOut>> toKeyPrefixOut,
            Func<TMetaOut, Task<TMetaIn>> toMetaIn,
            Func<TMetaIn, Task<TMetaOut>> toMetaOut)
        {
            return new ConverterKeyStreamStorage<TKeyOut, TKeyPrefixOut, TMetaOut, TKeyIn, TKeyPrefixIn, TMetaIn>(
                innerStorage, 
                toKeyIn,
                toKeyOut,
                toKeyPrefixIn,
                toKeyPrefixOut,
                toMetaIn,
                toMetaOut);
        }

        public static IKeyStreamStorage<TKeyOut, TKeyPrefixOut, TMetaOut>
            Convert<TKeyOut, TKeyPrefixOut, TMetaOut, TKeyIn, TKeyPrefixIn, TMetaIn>(
            this IKeyStreamStorage<TKeyIn, TKeyPrefixIn, TMetaIn> innerStorage,
            Func<TKeyOut, TKeyIn> toKeyIn,
            Func<TKeyIn, TKeyOut> toKeyOut,
            Func<TKeyPrefixOut, TKeyPrefixIn> toKeyPrefixIn,
            Func<TKeyPrefixIn, TKeyPrefixOut> toKeyPrefixOut,
            Func<TMetaOut, TMetaIn> toMetaIn,
            Func<TMetaIn, TMetaOut> toMetaOut)
        {
            return new ConverterKeyStreamStorage<TKeyOut, TKeyPrefixOut, TMetaOut, TKeyIn, TKeyPrefixIn, TMetaIn>(
                innerStorage, 
                toKeyIn.AsTaskResult(),
                toKeyOut.AsTaskResult(),
                toKeyPrefixIn.AsTaskResult(),
                toKeyPrefixOut.AsTaskResult(),
                toMetaIn.AsTaskResult(),
                toMetaOut.AsTaskResult());
        }
    }
}
