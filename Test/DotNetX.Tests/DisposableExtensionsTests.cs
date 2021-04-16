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
    public class DisposableExtensionsTests
    {

        [Test]
        public void CallingSetAndUndoWithNullDoActionShouldFail()
        {
            // Given
            Action action = () => "sample".SetAndUndo(default(Action)!, default!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CallingSetAndUndoWithNullUndoActionShouldFail()
        {
            // Given
            Action action = () => "sample".SetAndUndo(() => { }, default!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CallingSetAndUndoShouldCallDoActionAndReturnADisposable()
        {
            // Given
            var calls = new List<string>();

            // When
            var disposable = "sample".SetAndUndo(
                () => calls.Add("Do"),
                value => calls.Add($"Undo: {value}"));

            // Then
            disposable.Should().NotBeNull();
            calls.Should().Equal("Do");
        }

        [Test]
        public void CallingSetAndUndoAndThenDisposeShouldCallAllActions()
        {
            // Given
            var calls = new List<string>();

            // When
            var disposable = "sample".SetAndUndo(
                () => calls.Add("Do"),
                value => calls.Add($"Undo: {value}"));

            // And
            disposable.Dispose();

            // Then
            calls.Should().Equal("Do", "Undo: sample");
        }

        [Test]
        public void CallingSetAndUndoWithNullSetActionShouldFail()
        {
            // Given
            Action action = () => "sample".SetAndUndo("example", default!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CallingSetAndUndoShouldCallSetActionAndReturnADisposable()
        {
            // Given
            var calls = new List<string>();

            // When
            var disposable = "sample".SetAndUndo(
                "example", 
                value => calls.Add($"Set: {value}"));

            // Then
            disposable.Should().NotBeNull();
            calls.Should().Equal("Set: example");
        }

        [Test]
        public void CallingSetAndUndoAndThenDisposeShouldCallSetActionForDoAndUndo()
        {
            // Given
            var calls = new List<string>();

            // When
            var disposable = "sample".SetAndUndo(
                "example", 
                value => calls.Add($"Set: {value}"));

            // And
            disposable.Dispose();

            // Then
            calls.Should().Equal("Set: example", "Set: sample");
        }
    }
}
