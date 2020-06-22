using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;

namespace DotNetX.Tests
{
    [TestFixture]
    public class OptionalTests
    {
        [Test]
        public static void OptionalMustBeAStruct()
        {
            typeof(Optional<>).IsValueType.Should().BeTrue();
        }

        [Test]
        public static void OptionalNoneShouldNotBeSome()
        {
            Optional.None<string>().IsSome.Should().BeFalse();
        }

        [Test]
        public static void OptionalNoneShouldBeNone()
        {
            Optional.None<string>().IsNone.Should().BeTrue();
        }

        [Test]
        public static void OptionalSomeShouldBeSome()
        {
            Optional.Some("").IsSome.Should().BeTrue();
        }

        [Test]
        public static void OptionalSomeShouldNotBeNone()
        {
            Optional.Some("").IsNone.Should().BeFalse();
        }

        [Test]
        public static void OptionalMatchWithActionNoneShouldCallOnNone()
        {
            string result = "Not called";
            void OnSome(string value) => result = $"Called OnSome({value})";
            void OnNone() => result = $"Called OnNone()";
            Optional.None<string>().MatchWith(OnSome, OnNone);
            result.Should().Be("Called OnNone()");
        }

        [Test]
        public static void OptionalMatchWithActionSomeShouldCallOnSome()
        {
            string result = "Not called";
            void OnSome(string value) => result = $"Called OnSome({value})";
            void OnNone() => result = $"Called OnNone()";
            Optional.Some("value").MatchWith(OnSome, OnNone);
            result.Should().Be("Called OnSome(value)");
        }

        [Test]
        public static void OptionalMatchWithNoneShouldCallOnNone()
        {
            Optional.None<int>().MatchWith(v => v + 1, () => 42).Should().Be(42);
        }

        [Test]
        public static void OptionalMatchWithSomeShouldCallOnSome()
        {
            Optional.Some(10).MatchWith(v => v + 1, () => 42).Should().Be(11);
        }

        [Test]
        public static void OptionalNoneBindOnNoneShouldReturnNone()
        {
            Optional.None<int>().Bind(_ => Optional.None<string>()).Should().Be(Optional.None<string>());
        }

        [Test]
        public static void OptionalNoneBindOnSomeShouldReturnNone()
        {
            Optional.None<int>().Bind(v => Optional.Some(v.ToString())).Should().Be(Optional.None<string>());
        }

        [Test]
        public static void OptionalSomeBindOnNoneShouldReturnNone()
        {
            Optional.Some(42).Bind(_ => Optional.None<string>()).Should().Be(Optional.None<string>());
        }

        [Test]
        public static void OptionalSomeBindOnSomeShouldReturnSome()
        {
            Optional.Some(42).Bind(v => Optional.Some(v.ToString())).Should().Be(Optional.Some("42"));
        }

        [Test]
        public static void OptionalNoneMapShouldReturnNone()
        {
            Optional.None<int>().Map(v => v.ToString()).Should().Be(Optional.None<string>());
        }

        [Test]
        public static void OptionalSomeMapShouldReturnSome()
        {
            Optional.Some(42).Map(v => v.ToString()).Should().Be(Optional.Some("42"));
        }

        [Test]
        public static void OptionalNoneTryGetValueShouldBeFalse()
        {
            Optional.None<int>().TryGetValue(out var value).Should().BeFalse();
        }

        [Test]
        public static void OptionalSomeTryGetValueShouldBeTrue()
        {
            Optional.Some(42).TryGetValue(out var value).Should().BeTrue();
            value.Should().Be(42);
        }

        [Test]
        public static void OptionalNoneGetValueShouldThrow()
        {
            true.Invoking(_ => Optional.None<int>().GetValue()).Should().Throw<InvalidOperationException>();
        }

        [Test]
        public static void OptionalSomeGetValueShouldBeTheValue()
        {
            Optional.Some(42).GetValue().Should().Be(42);
        }

        [Test]
        public static void OptionalNoneGetValueWithDefaultShouldReturnDefault()
        {
            Optional.None<int>().GetValue(100).Should().Be(100);
        }

        [Test]
        public static void OptionalSomeGetValueWithDefaultShouldReturnTheValue()
        {
            Optional.Some(42).GetValue(100).Should().Be(42);
        }

        [Test]
        public static void OptionalNoneGetValueWithDefaultFuncShouldReturnDefault()
        {
            Optional.None<int>().GetValue(() => 100).Should().Be(100);
        }

        [Test]
        public static void OptionalSomeGetValueWithDefaultFuncShouldReturnTheValue()
        {
            Optional.Some(42).GetValue(() => 100).Should().Be(42);
        }

        [Test]
        public static void OptionalNoneAsEnumerableShouldReturnEmpty()
        {
            Optional.None<int>().AsEnumerable().Should().Equal(Enumerable.Empty<int>());
        }

        [Test]
        public static void OptionalSomeAsEnumerableShouldReturnSingleton()
        {
            Optional.Some(42).AsEnumerable().Should().Equal(42.Singleton());
        }
    }
}
