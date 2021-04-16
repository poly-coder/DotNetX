using FluentAssertions;
using FsCheck;
using FsCheck.NUnit;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace DotNetX.Tests
{
    [TestFixture]
    public class DisposablesTests
    {
        [Test]
        public void WhenADisposablesIsDisposedWithNoDisposablesItShouldNotFail()
        {
            // Given
            var disposables = new Disposables();

            // When
            disposables.Dispose();

            // Then
            // didn't failed
        }

        [Test]
        public void WhenADisposablesIsDisposedItShouldDisposeChildDisposable()
        {
            // Given
            var calls = new List<string>();
            var disposables = new Disposables()
                .Add(() => calls.Add("action1"));

            // When
            disposables.Dispose();

            // Then
            calls.Should().Equal("action1");
        }
        
        [Test]
        public void WhenADisposablesIsDisposedItShouldDisposeChildrenDisposables()
        {
            // Given
            var calls = new List<string>();
            var disposables = new Disposables()
                .Add(
                    () => calls.Add("action1"),
                    () => calls.Add("action2"),
                    () => calls.Add("action3")
                );

            // When
            disposables.Dispose();

            // Then
            calls.Should().Equal("action1", "action2", "action3");
        }

        [Test]
        public void WhenADisposablesIsDisposedMultipleTimesItShouldDisposeChildrenDisposablesOnce()
        {
            // Given
            var calls = new List<string>();
            var disposables = new Disposables()
                .Add(
                    () => calls.Add("action1"),
                    () => calls.Add("action2"),
                    () => calls.Add("action3")
                );

            // When
            disposables.Dispose();
            disposables.Dispose();
            disposables.Dispose();

            // Then
            calls.Should().Equal("action1", "action2", "action3");
        }

        [Test]
        public void WhenADisposablesWithDisposableArrayIsDisposedItShouldDisposeChildrenDisposables()
        {
            // Given
            var calls = new List<string>();
            var disposables = new Disposables()
                .Add(
                    new Disposable(() => calls.Add("action1")),
                    new Disposable(() => calls.Add("action2")),
                    new Disposable(() => calls.Add("action3")));

            // When
            disposables.Dispose();

            // Then
            calls.Should().Equal("action1", "action2", "action3");
        }

        [Test]
        public void AddingNullDisposableShouldFail()
        {
            // Given
            Action action = () => new Disposables().Add(default(IDisposable)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddingNullDisposableEnumerableShouldFail()
        {
            // Given
            Action action = () => new Disposables().Add(default(IEnumerable<IDisposable>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddingDisposableEnumerableWithNullShouldFail()
        {
            // Given
            Action action = () => new Disposables()
                .Add(new List<IDisposable> 
                {  
                    new Disposable(() => { }),
                    default(IDisposable)!,
                    new Disposable(() => { }),
                });

            // Then
            action.Should().Throw<ArgumentNullException>();
        }
    }
}
