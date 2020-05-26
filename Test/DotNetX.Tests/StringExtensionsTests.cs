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

        #region [ Shorten ]

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

        #endregion [ Shorten ]

        #region [ IsNullOrEmpty / IsNullOrWhiteSpace ]

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

        #endregion [ IsNullOrEmpty / IsNullOrWhiteSpace ]

        #region [ RemoveSuffix / RemovePrefix ]

        [Property]
        public bool RemoveSuffixWhenNonEmptyStart(NonEmptyString start, NonNull<string> suffix)
        {
            (start.Get + suffix.Get).RemoveSuffix(suffix.Get).Should().Be(start.Get);
            return true;
        }
    
        [Property]
        public bool RemoveSuffixWhenEmptyStart(NonNull<string> suffix)
        {
            suffix.Get.RemoveSuffix(suffix.Get).Should().Be(suffix.Get);
            return true;
        }
    
        [Property]
        public bool RemoveSuffixWhenNonSuffix(NonEmptyString other, NonEmptyString suffix)
        {
            if (!other.Get.EndsWith(suffix.Get))
            {
                other.Get.RemoveSuffix(suffix.Get).Should().Be(other.Get);
            }
            return true;
        }

        [Property]
        public bool RemovePrefixWhenNonEmptyStart(NonNull<string> prefix, NonEmptyString end)
        {
            (prefix.Get + end.Get).RemovePrefix(prefix.Get).Should().Be(end.Get);
            return true;
        }
    
        [Property]
        public bool RemovePrefixWhenEmptyStart(NonNull<string> prefix)
        {
            prefix.Get.RemovePrefix(prefix.Get).Should().Be(prefix.Get);
            return true;
        }
    
        [Property]
        public bool RemovePrefixWhenNonPrefix(NonEmptyString other, NonEmptyString prefix)
        {
            if (!other.Get.StartsWith(prefix.Get))
            {
                other.Get.RemovePrefix(prefix.Get).Should().Be(other.Get);
            }
            return true;
        }

        #endregion [ RemoveSuffix / RemovePrefix ]

        #region [ Before / After ]

        [Test]
        public void BeforeWithNonExistingSeparator()
        {
            "This does not have separator".Before(",").Should().BeNull();
        }

        [Test]
        public void BeforeWithExistingSeparator()
        {
            "This does not have separator".Before(" ").Should().Be("This");
        }

        [Test]
        public void BeforeOrAllWithNonExistingSeparator()
        {
            "This does not have separator".BeforeOrAll(",").Should().Be("This does not have separator");
        }

        [Test]
        public void BeforeOrAllWithExistingSeparator()
        {
            "This does not have separator".BeforeOrAll(" ").Should().Be("This");
        }

        [Test]
        public void BeforeLastWithNonExistingSeparator()
        {
            "This does not have separator".BeforeLast(",").Should().BeNull();
        }

        [Test]
        public void BeforeLastWithExistingSeparator()
        {
            "This does not have separator".BeforeLast(" ").Should().Be("This does not have");
        }

        [Test]
        public void BeforeLastOrAllWithNonExistingSeparator()
        {
            "This does not have separator".BeforeLastOrAll(",").Should().Be("This does not have separator");
        }

        [Test]
        public void BeforeLastOrAllWithExistingSeparator()
        {
            "This does not have separator".BeforeLastOrAll(" ").Should().Be("This does not have");
        }
        
        #endregion [ Before / After ]
    }
}