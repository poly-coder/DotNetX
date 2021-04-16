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
    public class DisposableTests
    {
        [Test]
        public void WhenADisposableIsCreatedWithNoDisposeActionItShouldFail()
        {
            // Given
            Action createDisposable = () => new Disposable(null!);

            // Then
            createDisposable.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void WhenADisposableIsCreatedItShouldNotCallDisposeAction()
        {
            // Given
            var calls = new List<string>();
            Action disposeAction = () => calls.Add("Managed");

            // When
            var disposable = new Disposable(disposeAction);

            // Then
            calls.Should().Equal();
        }

        [Test]
        public void WhenADisposableIsCreatedWithUnmanagedActionItShouldNotCallDisposeActions()
        {
            // Given
            var calls = new List<string>();
            Action disposeAction = () => calls.Add("Managed");
            Action unmanagedDisposeAction = () => calls.Add("Unmanaged");

            // When
            var disposable = new Disposable(disposeAction, unmanagedDisposeAction);

            // Then
            calls.Should().Equal();
        }

        [Test]
        public void WhenADisposableIsDisposedItShouldCallDisposeAction()
        {
            // Given
            var calls = new List<string>();
            Action disposeAction = () => calls.Add("Managed");

            // When
            using (var disposable = new Disposable(disposeAction))
            {

            }

            // Then
            calls.Should().Equal("Managed");
        }

        [Test]
        public void WhenADisposableIsDisposedWithUnmanagedActionItShouldCallDisposeActions()
        {
            // Given
            var calls = new List<string>();
            Action disposeAction = () => calls.Add("Managed");
            Action unmanagedDisposeAction = () => calls.Add("Unmanaged");

            // When
            using (var disposable = new Disposable(disposeAction, unmanagedDisposeAction))
            {

            }

            // Then
            calls.Should().Equal("Managed", "Unmanaged");
        }

        [Test]
        public void WhenADisposableIsDisposedMultipleTimesItShouldCallDisposeActionsOnce()
        {
            // Given
            var calls = new List<string>();
            Action disposeAction = () => calls.Add("Managed");
            Action unmanagedDisposeAction = () => calls.Add("Unmanaged");

            // When
            using (var disposable = new Disposable(disposeAction, unmanagedDisposeAction))
            {
                disposable.Dispose();
                disposable.Dispose();
                disposable.Dispose();
            }

            // Then
            calls.Should().Equal("Managed", "Unmanaged");
        }
    }
}
