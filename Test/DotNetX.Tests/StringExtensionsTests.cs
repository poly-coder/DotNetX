using FluentAssertions;
using FsCheck;
using FsCheck.NUnit;
using NUnit.Framework;
using System;
using System.Linq;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace DotNetX.Tests
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Property]
        public bool ShorteningToLengthZero(NonNull<string> text, NonNull<string> elipsis) =>
            text.Get.Shorten(0, elipsis.Get) == "";

        [Property]
        public bool ShorteningToAnyLength(NonNull<string> text, NonNegativeInt maxLength, NonNull<string> elipsis) =>
            text.Get.Shorten(maxLength.Get, elipsis.Get).Length <= maxLength.Get;

        [Property]
        public bool ShorteningWithElipsis(NonNull<string> text, NonNegativeInt maxLength, NonNull<string> elipsis)
        {
            return text.Get.Shorten(maxLength.Get, elipsis.Get) switch
            {
                var shorten when text.Get.Length <= maxLength.Get => shorten == text.Get,
                var shorten when maxLength.Get <= elipsis.Get.Length => elipsis.Get.StartsWith(shorten),
                var shorten => shorten == text.Get.Substring(0, maxLength.Get - elipsis.Get.Length) + elipsis.Get,
            };
        }

        [Property]
        public bool NullIsNullOrEmpty() => ((string)null).IsNullOrEmpty();

        [Property]
        public bool EmptyIsNullOrEmpty() => "".IsNullOrEmpty();

        [Property]
        public bool NonEmptyIsNotNullOrEmpty(NonEmptyString text) => !text.Get.IsNullOrEmpty();

        [Property]
        public bool WhiteSpaceIsNotNullOrEmpty(PositiveInt size) =>
            !new String(' ', size.Get).IsNullOrEmpty();

        [Property]
        public bool NullIsNullOrWhiteSpace() => ((string)null).IsNullOrWhiteSpace();

        [Property]
        public bool EmptyIsNullOrWhiteSpace() => "".IsNullOrWhiteSpace();

        [Property]
        public bool WhiteSpaceIsNullOrWhiteSpace(PositiveInt size) =>
            new String(' ', size.Get).IsNullOrWhiteSpace();

        [Property]
        public bool NonEmptyIsNotNullOrWhiteSpace(NonEmptyString text) =>
            text.Get.All(c => char.IsWhiteSpace(c))
                ? text.Get.IsNullOrWhiteSpace()
                : !text.Get.IsNullOrWhiteSpace();
    
        [Property]
        public bool RemoveSuffixWhenFound(NonNull<string> start, NonNull<string> suffix)
        {
            return (start.Get + suffix.Get).RemoveSuffix(suffix.Get) == start.Get;
        }
    }
}