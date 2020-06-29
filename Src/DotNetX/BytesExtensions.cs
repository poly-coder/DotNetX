using System.Security.Cryptography;

namespace DotNetX
{
    public static class BytesExtensions
    {
        public static byte[] ComputeMD5(this byte[] buffer)
        {
#pragma warning disable CA5351
            using var hasher = MD5.Create();
#pragma warning restore CA5351
            return hasher.ComputeHash(buffer);
        }

        public static byte[] ComputeSHA1(this byte[] buffer)
        {
#pragma warning disable CA5350
            using var hasher = SHA1.Create();
#pragma warning restore CA5350
            return hasher.ComputeHash(buffer);
        }

        public static byte[] ComputeSHA256(this byte[] buffer)
        {
            using var hasher = SHA256.Create();
            return hasher.ComputeHash(buffer);
        }

        public static byte[] ComputeSHA384(this byte[] buffer)
        {
            using var hasher = SHA384.Create();
            return hasher.ComputeHash(buffer);
        }

        public static byte[] ComputeSHA512(this byte[] buffer)
        {
            using var hasher = SHA512.Create();
            return hasher.ComputeHash(buffer);
        }

        public static void FillRandom(this byte[] buffer)
        {
            using var rng = RNGCryptoServiceProvider.Create();
            rng.GetBytes(buffer);
        }

        public static void FillRandomNonZero(this byte[] buffer)
        {
            using var rng = RNGCryptoServiceProvider.Create();
            rng.GetNonZeroBytes(buffer);
        }

        public static byte[] GetRandomBytes(this int byteCount)
        {
            var bytes = new byte[byteCount];
            bytes.FillRandom();
            return bytes;
        }

        public static byte[] GetRandomBytesNonZero(this int byteCount)
        {
            var bytes = new byte[byteCount];
            bytes.FillRandomNonZero();
            return bytes;
        }
    }
}
