using System;
using System.Globalization;
using System.Text;

namespace DotNetX
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);


        public static string RemoveSuffix(this string value, string suffix)
        {
            if (value == null) return null;
            if (value.Length <= suffix.Length || !value.EndsWith(suffix)) return value;
            return value.Substring(0, value.Length - suffix.Length);
        }


        public static string Before(this string text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
            => BeforeAux(text, text.IndexOf(separator, comparisonType));

        public static string Before(this string text, char separator)
            => BeforeAux(text, text.IndexOf(separator));

        public static string BeforeLast(this string text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
            => BeforeAux(text, text.LastIndexOf(separator, comparisonType));

        public static string BeforeLast(this string text, char separator)
            => BeforeAux(text, text.LastIndexOf(separator));

        private static string BeforeAux(string text, int index) => index switch {
            -1 => null,
            _ => text.Substring(0, index)
        };


        public static string After(this string text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
            => AfterAux(text, text.IndexOf(separator, comparisonType), separator.Length);

        public static string After(this string text, char separator)
            => AfterAux(text, text.IndexOf(separator), 1);

        public static string AfterLast(this string text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
            => AfterAux(text, text.LastIndexOf(separator, comparisonType), separator.Length);

        public static string AfterLast(this string text, char separator)
            => AfterAux(text, text.LastIndexOf(separator), 1);

        private static string AfterAux(string text, int index, int separatorLength) => index switch {
            -1 => null,
            _ => text.Substring(index + separatorLength)
        };



        public static byte[] ToEncodingBytes(this string value, Encoding encoding)
        {
            return encoding.GetBytes(value);
        }
        public static string FromEncodingBytes(this byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(bytes);
        }


        public static byte[] ToUtf8(this string value) => value.ToEncodingBytes(Encoding.UTF8);
        public static string FromUtf8(this byte[] bytes) => bytes.FromEncodingBytes(Encoding.UTF8);

        public static string ToBase64(this byte[] bytes) => Convert.ToBase64String(bytes);
        public static byte[] FromBase64(this string value) => Convert.FromBase64String(value);

        public static string ToHexString(this byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
        public static byte[] FromHexStrinng(this string value)
        {
            if (value.Length % 2 != 0)
            {
                throw new ArgumentException("hex string must contain an even number of hex digits");
            }
            var bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(value.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return bytes;
        }

        
        public static string Shorten(this string value, int maxLength, string elipsis = "")
        {
            if (value.Length <= maxLength)
            {
                return value;
            }

            if (elipsis.Length >= maxLength)
            {
                return elipsis.Substring(0, maxLength);
            }

            return value.Substring(0, maxLength - elipsis.Length) + elipsis;
        }
    
    
        public static string ComputeHash(this string value, Func<byte[], byte[]> byteHasher) =>
            byteHasher(value.ToUtf8()).ToHexString();

        public static string ComputeShortenHash(this string value, int maxLength, Func<byte[], byte[]> byteHasher) =>
            value.ComputeHash(byteHasher).Shorten(maxLength, "");

        public static string ComputeMD5(this string value) => value.ComputeHash(BytesExtensions.ComputeMD5);
        public static string ComputeShortenMD5(this string value, int maxLength = 10) =>
            value.ComputeShortenHash(maxLength, BytesExtensions.ComputeMD5);

        public static string ComputeSHA1(this string value) => value.ComputeHash(BytesExtensions.ComputeSHA1);
        public static string ComputeShortenSHA1(this string value, int maxLength = 10) =>
            value.ComputeShortenHash(maxLength, BytesExtensions.ComputeSHA1);

        public static string ComputeSHA256(this string value) => value.ComputeHash(BytesExtensions.ComputeSHA256);
        public static string ComputeShortenSHA256(this string value, int maxLength = 10) =>
            value.ComputeShortenHash(maxLength, BytesExtensions.ComputeSHA256);

        public static string ComputeSHA384(this string value) => value.ComputeHash(BytesExtensions.ComputeSHA384);
        public static string ComputeShortenSHA384(this string value, int maxLength = 10) =>
            value.ComputeShortenHash(maxLength, BytesExtensions.ComputeSHA384);

        public static string ComputeSHA512(this string value) => value.ComputeHash(BytesExtensions.ComputeSHA512);
        public static string ComputeShortenSHA512(this string value, int maxLength = 10) =>
            value.ComputeShortenHash(maxLength, BytesExtensions.ComputeSHA512);

        public static string GetRandomHexString(this int length) =>
            (length / 2 + 1).GetRandomBytes().ToHexString().Shorten(length, "");
    }
}
