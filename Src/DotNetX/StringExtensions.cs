using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetX
{

    public static class StringExtensions
    {
        #region [ IsNullOrEmpty / IsNullOrWhiteSpace ]

        public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);

        public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

        public static bool IsNotNullOrEmpty(this string? value) => !string.IsNullOrEmpty(value);

        public static bool IsNotNullOrWhiteSpace(this string? value) => !string.IsNullOrWhiteSpace(value);

        #endregion [ IsNullOrEmpty / IsNullOrWhiteSpace ]


        #region [ RemoveSuffix / RemovePrefix / RemovePattern ]

        public static string? RemoveSuffix(this string? value, string suffix)
        {
            if (value == null) return null;

            if (suffix is null)
            {
                throw new ArgumentNullException(nameof(suffix));
            }

            if (value.Length <= suffix.Length || !value.EndsWith(suffix, StringComparison.Ordinal)) return value;
            return value.Substring(0, value.Length - suffix.Length);
        }

        public static string? RemovePrefix(this string? value, string prefix)
        {
            if (value == null) return null;

            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (value.Length <= prefix.Length || !value.StartsWith(prefix, StringComparison.Ordinal)) return value;
            return value.Substring(prefix.Length);
        }

        public static string? RemovePattern(this string? value, Regex regex, int startAt = 0, int length = -1, string? groupName = null)
        {
            if (value == null) return null;

            if (regex is null)
            {
                throw new ArgumentNullException(nameof(regex));
            }

            var match = regex.Match(value, startAt, length < 0 ? value.Length : length);
            if (match.Success)
            {
                var capture = groupName == null ? match : match.Groups[groupName];
                return value.Remove(capture.Index, capture.Length);
            }
            return value;
        }

        public static string? RemovePattern(this string? value, string pattern, int startAt = 0, int length = -1, string? groupName = null, RegexOptions options = RegexOptions.None)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            var regex = new Regex(pattern, options);
            return value.RemovePattern(regex, startAt, length, groupName);
        }

        #endregion [ RemoveSuffix / RemovePrefix ]


        #region [ Before[Last][OrAll] / After[Last][OrAll] ]

        public static string? Before(this string? text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (text is null)
            {
                return null;
            }

            if (separator is null)
            {
                throw new ArgumentNullException(nameof(separator));
            }

            return BeforeAux(text, text.IndexOf(separator, comparisonType));
        }

        public static string? Before(this string? text, char separator)
        {
            if (text is null)
            {
                return null;
            }

            return BeforeAux(text, text.IndexOf(separator, StringComparison.Ordinal));
        }

        public static string? BeforeLast(this string? text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (text is null)
            {
                return null;
            }

            if (separator is null)
            {
                throw new ArgumentNullException(nameof(separator));
            }

            return BeforeAux(text, text.LastIndexOf(separator, comparisonType));
        }

        public static string? BeforeLast(this string? text, char separator)
        {
            if (text is null)
            {
                return null;
            }

            return BeforeAux(text, text.LastIndexOf(separator));
        }

        public static string BeforeOrAll(this string text, string separator, StringComparison comparisonType = StringComparison.Ordinal) =>
            text.Before(separator, comparisonType) ?? text;

        public static string BeforeOrAll(this string text, char separator)
            => Before(text, separator) ?? text;

        public static string BeforeLastOrAll(this string text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
            => BeforeLast(text, separator, comparisonType) ?? text;

        public static string BeforeLastOrAll(this string text, char separator)
            => BeforeLast(text, separator) ?? text;

        private static string? BeforeAux(string text, int index) => index switch
        {
            _ when index < 0 => null,
            _ when index >= text.Length => text,
            _ => text.Substring(0, index)
        };


        public static string? After(this string? text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (text is null)
            {
                return null;
            }

            if (separator is null)
            {
                throw new ArgumentNullException(nameof(separator));
            }

            return AfterAux(text, text.IndexOf(separator, comparisonType), separator.Length);
        }

        public static string? After(this string? text, char separator)
        {
            if (text is null)
            {
                return null;
            }

            return AfterAux(text, text.IndexOf(separator, StringComparison.Ordinal), 1);
        }

        public static string? AfterLast(this string? text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (text is null)
            {
                return null;
            }

            if (separator is null)
            {
                throw new ArgumentNullException(nameof(separator));
            }

            return AfterAux(text, text.LastIndexOf(separator, comparisonType), separator.Length);
        }

        public static string? AfterLast(this string? text, char separator)
        {
            if (text is null)
            {
                return null;
            }

            return AfterAux(text, text.LastIndexOf(separator), 1);
        }

        public static string AfterOrAll(this string text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
            => After(text, separator, comparisonType) ?? text;

        public static string AfterOrAll(this string text, char separator)
            => After(text, separator) ?? text;

        public static string AfterLastOrAll(this string text, string separator, StringComparison comparisonType = StringComparison.Ordinal)
            => AfterLast(text, separator, comparisonType) ?? text;

        public static string AfterLastOrAll(this string text, char separator)
            => AfterLast(text, separator) ?? text;

        private static string? AfterAux(string text, int index, int separatorLength) => index switch
        {
            _ when index < 0 => null,
            _ => text.Substring(index + separatorLength)
        };

        #endregion [ Before[Last][OrAll] / After[Last][OrAll] ]


        #region [ ToEncodingBytes / FromEncodingBytes ]

        public static byte[] ToEncodingBytes(this string value, Encoding encoding)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return encoding.GetBytes(value);
        }
        public static string FromEncodingBytes(this byte[] bytes, Encoding encoding)
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return encoding.GetString(bytes);
        }


        public static byte[] ToUtf8(this string value) => value.ToEncodingBytes(Encoding.UTF8);
        public static string FromUtf8(this byte[] bytes) => bytes.FromEncodingBytes(Encoding.UTF8);

        #endregion [ ToEncodingBytes / FromEncodingBytes ]


        #region [ ToBase64 / FromBase64 ]

        public static string ToBase64(this byte[] bytes) => Convert.ToBase64String(bytes);
        public static byte[] FromBase64(this string value) => Convert.FromBase64String(value);

        #endregion [ ToBase64 / FromBase64 ]


        #region [ ToHexString / FromHexString ]

        public static string ToHexString(this byte[] bytes, bool lowercase = false)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var sb = new StringBuilder(bytes.Length * 2);
            var format = lowercase ? "x2" : "X2";
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString(format, CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }
        public static byte[] FromHexString(this string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length % 2 != 0)
            {
                throw new ArgumentException(Resource.Error_HexStringLengthMustBeEven);
            }
            var bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(value.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return bytes;
        }

        #endregion [ ToHexString / FromHexStrinng ]


        #region [ Shorten ]

        public static string Shorten(this string value, int maxLength, string elipsis = "")
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length <= maxLength)
            {
                return value;
            }

            if (elipsis is null)
            {
                throw new ArgumentNullException(nameof(elipsis));
            }

            if (elipsis.Length >= maxLength)
            {
                return elipsis.Substring(0, maxLength);
            }

            return value.Substring(0, maxLength - elipsis.Length) + elipsis;
        }

        #endregion [ Shorten ]


        #region [ Compute[Shorten][Hash|MD5|SHAn] ]

        public static string ComputeHash(this string value, Func<byte[], byte[]> byteHasher)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (byteHasher is null)
            {
                throw new ArgumentNullException(nameof(byteHasher));
            }

            return byteHasher(value.ToUtf8()).ToHexString();
        }

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

        #endregion [ Compute[Shorten][Hash|MD5|SHAn] ]


        #region [ GetRandomHexString ]

        public static string GetRandomHexString(this int length, bool lowercase = false) =>
            (length / 2 + 1).GetRandomBytes().ToHexString(lowercase: lowercase).Shorten(length, "");

        #endregion [ GetRandomHexString ]


        #region [ ChangeCase ]

        // TODO: This is a simplified version. We have to create a solution to allow for all case change operations
        public static string ToCamelCase(this string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (text.Length >= 1)
            {
                if (char.IsUpper(text, 0))
                {
                    text = char.ToLowerInvariant(text[0]) + text.Substring(1);
                }
            }
            return text;
        }

        #endregion [ ChangeCase ]


        #region [ Format ]

        public static string Format(this string format, params object[] args) =>
#pragma warning disable CA1305
            string.Format(format, args);
#pragma warning restore CA1305

        public static string Format(this string format, object arg0, object arg1, object arg2) =>
#pragma warning disable CA1305
            string.Format(format, arg0, arg1, arg2);
#pragma warning restore CA1305

        public static string Format(this string format, object arg0, object arg1) =>
#pragma warning disable CA1305
            string.Format(format, arg0, arg1);
#pragma warning restore CA1305

        public static string Format(this string format, object arg0) =>
#pragma warning disable CA1305
            string.Format(format, arg0);
#pragma warning restore CA1305

        public static string Format(this string format, IFormatProvider provider, params object[] args) =>
            string.Format(provider, format, args);

        public static string Format(this string format, IFormatProvider provider, object arg0, object arg1, object arg2) =>
            string.Format(provider, format, arg0, arg1, arg2);

        public static string Format(this string format, IFormatProvider provider, object arg0, object arg1) =>
            string.Format(provider, format, arg0, arg1);

        public static string Format(this string format, IFormatProvider provider, object arg0) =>
            string.Format(provider, format, arg0);

        #endregion [ Format ]
    }
}
