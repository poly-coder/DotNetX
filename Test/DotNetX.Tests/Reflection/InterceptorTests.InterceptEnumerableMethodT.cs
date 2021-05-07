using DotNetX.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetX.Tests
{
    partial class InterceptorTests
    {

        [Test]
        public void InterceptEnumerableMethodTDefaultShouldHaveAllActionsToNull()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTDefaultShouldNotFailWhenIntercepting()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 });

            var intercepted =
                InterceptorOptions.Default
                    .Add(interceptor)
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.EnumerableMethod("value1");

            // Then
            result.Should().Equal(new[] { 1, 2, 3, 4, 5 });

            targetMock.Verify(
                target => target.EnumerableMethod("value1"),
                Times.Once);
        }

        #region [ IEnumerable ]

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeNextAndCompleteInterceptorsOnIEnumerableResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.EnumerableMethod("value1");

            // Then
            result.Should().Equal(new[] { 1, 2, 3, 4, 5 });

            targetMock.Verify(
                target => target.EnumerableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            NextCalledTimes(interceptorsMock, 5, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 1, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 2, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 3, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 4, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 5, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            CompleteCalled(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndCompleteInterceptorsOnIEnumerableResultWithNull()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Returns(default(IEnumerable)!);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.EnumerableMethod("value1");

            // Then
            result.Should().BeNull();

            targetMock.Verify(
                target => target.EnumerableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            NextNotCalled(interceptorsMock);

            CompleteCalled(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnIEnumerableResultWithStartError()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Throws<FormatException>();

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Action action = () => intercepted.EnumerableMethod("value1");

            // Then
            action.Should().Throw<FormatException>();

            targetMock.Verify(
                target => target.EnumerableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            NextNotCalled(interceptorsMock);

            CompleteNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnIEnumerableResultWithMiddleError()
        {
            // Given
            IEnumerable MockEnumerable()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                throw new FormatException();
            }

            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Returns((string _) => MockEnumerable());

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.EnumerableMethod("value1");
            Action action = () => result.Should().Equal(new[] { 1, 2, 3, 4, 5 });

            // Then
            action.Should().Throw<FormatException>();

            targetMock.Verify(
                target => target.EnumerableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            NextCalledTimes(interceptorsMock, 3, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 1, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 2, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 3, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            CompleteNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsTurnedOffShouldNotCallInterceptorsOnIEnumerable()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(false);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.EnumerableMethod("value1");

            // Then
            result.Should().Equal(new[] { 1, 2, 3, 4, 5 });

            targetMock.Verify(
                target => target.EnumerableMethod("value1"),
                Times.Once);

            BeforeNotCalled(interceptorsMock);

            NextNotCalled(interceptorsMock);

            CompleteNotCalled(interceptorsMock);

            ErrorNotCalled(interceptorsMock);
        }

        #endregion [ IEnumerable ]

        #region [ IEnumerable<T> ]

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeNextAndCompleteInterceptorsOnIEnumerableTResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableOfTMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.EnumerableOfTMethod("value1");

            // Then
            result.Should().Equal(new[] { 1, 2, 3, 4, 5 });

            targetMock.Verify(
                target => target.EnumerableOfTMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");

            NextCalledTimes(interceptorsMock, 5, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 1, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 2, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 3, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 4, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 5, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");

            CompleteCalled(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndCompleteInterceptorsOnIEnumerableTResultWithNull()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableOfTMethod(It.IsAny<string>()))
                .Returns(default(IEnumerable<int>)!);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.EnumerableOfTMethod("value1");

            // Then
            result.Should().BeNull();

            targetMock.Verify(
                target => target.EnumerableOfTMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");

            NextNotCalled(interceptorsMock);

            CompleteCalled(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnIEnumerableTResultWithStartError()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableOfTMethod(It.IsAny<string>()))
                .Throws<FormatException>();

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Action action = () => intercepted.EnumerableOfTMethod("value1");

            // Then
            action.Should().Throw<FormatException>();

            targetMock.Verify(
                target => target.EnumerableOfTMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");

            NextNotCalled(interceptorsMock);

            CompleteNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnIEnumerableTResultWithMiddleError()
        {
            // Given
            IEnumerable<int> MockEnumerable()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                throw new FormatException();
            }

            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableOfTMethod(It.IsAny<string>()))
                .Returns((string _) => MockEnumerable());

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.EnumerableOfTMethod("value1");
            Action action = () => result.Should().Equal(new[] { 1, 2, 3, 4, 5 });

            // Then
            action.Should().Throw<FormatException>();

            targetMock.Verify(
                target => target.EnumerableOfTMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");

            NextCalledTimes(interceptorsMock, 3, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 1, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 2, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 3, targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");

            CompleteNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.EnumerableOfTMethod), "value1");
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsTurnedOffShouldNotCallInterceptorsOnIEnumerableT()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(false);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableOfTMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.EnumerableOfTMethod("value1");

            // Then
            result.Should().Equal(new[] { 1, 2, 3, 4, 5 });

            targetMock.Verify(
                target => target.EnumerableOfTMethod("value1"),
                Times.Once);

            BeforeNotCalled(interceptorsMock);

            NextNotCalled(interceptorsMock);

            CompleteNotCalled(interceptorsMock);

            ErrorNotCalled(interceptorsMock);
        }

        #endregion [ IEnumerable<T> ]

        #region [ IAsyncEnumerable<T> ]

        [Test]
        public async Task InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeNextAndCompleteInterceptorsOnIAsyncEnumerableAsync()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.AsyncEnumerableMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 }.ToAsyncEnumerable());

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.AsyncEnumerableMethod("value1");

            // Then
            (await result.ToArrayAsync()).Should().Equal(new[] { 1, 2, 3, 4, 5 });

            targetMock.Verify(
                target => target.AsyncEnumerableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");

            NextCalledTimes(interceptorsMock, 5, "STATE", targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 1, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 2, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 3, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 4, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 5, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");

            CompleteCalled(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndCompleteInterceptorsOnIAsyncEnumerableWithNull()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.AsyncEnumerableMethod(It.IsAny<string>()))
                .Returns(default(IAsyncEnumerable<int>)!);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.AsyncEnumerableMethod("value1");

            // Then
            result.Should().BeNull();

            targetMock.Verify(
                target => target.AsyncEnumerableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");

            NextNotCalled(interceptorsMock);

            CompleteCalled(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnIAsyncEnumerableWithStartError()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.AsyncEnumerableMethod(It.IsAny<string>()))
                .Throws<FormatException>();

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Action action = () => intercepted.AsyncEnumerableMethod("value1");

            // Then
            action.Should().Throw<FormatException>();

            targetMock.Verify(
                target => target.AsyncEnumerableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");

            NextNotCalled(interceptorsMock);

            CompleteNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");
        }

        [Test]
        public async Task InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnIAsyncEnumerableWithMiddleErrorAsync()
        {
            // Given
            async IAsyncEnumerable<int> MockEnumerable()
            {
                await Task.CompletedTask;
                yield return 1;
                yield return 2;
                yield return 3;
                throw new FormatException();
            }

            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.AsyncEnumerableMethod(It.IsAny<string>()))
                .Returns((string _) => MockEnumerable());

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.AsyncEnumerableMethod("value1");
            Func<Task> action = async () =>
            {
                var arr = await result.ToArrayAsync();
                arr.Should().Equal(new[] { 1, 2, 3, 4, 5 });
            };

            // Then
            await action.Should().ThrowAsync<FormatException>();

            targetMock.Verify(
                target => target.AsyncEnumerableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");

            NextCalledTimes(interceptorsMock, 3, "STATE", targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 1, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 2, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 3, targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");

            CompleteNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.AsyncEnumerableMethod), "value1");
        }

        [Test]
        public async Task InterceptEnumerableMethodTWithInterceptorsTurnedOffShouldNotCallInterceptorsOnIAsyncEnumerableAsync()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(false);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.AsyncEnumerableMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 }.ToAsyncEnumerable());

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.AsyncEnumerableMethod("value1");

            // Then
            (await result.ToArrayAsync()).Should().Equal(new[] { 1, 2, 3, 4, 5 });

            targetMock.Verify(
                target => target.AsyncEnumerableMethod("value1"),
                Times.Once);

            BeforeNotCalled(interceptorsMock);

            NextNotCalled(interceptorsMock);

            CompleteNotCalled(interceptorsMock);

            ErrorNotCalled(interceptorsMock);
        }

        #endregion [ IAsyncEnumerable<T> ]

        #region [ IObserable<T> ]

        [Test]
        public async Task InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeNextAndCompleteInterceptorsOnIObservableAsync()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ObservableMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 }.ToObservable());

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.ObservableMethod("value1");

            // Then
            var array = await result.ToArray();
            array.Should().Equal(new[] { 1, 2, 3, 4, 5 });

            targetMock.Verify(
                target => target.ObservableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");

            NextCalledTimes(interceptorsMock, 5, "STATE", targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 1, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 2, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 3, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 4, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 5, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");

            CompleteCalled(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndCompleteInterceptorsOnIObservableWithNull()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ObservableMethod(It.IsAny<string>()))
                .Returns(default(IObservable<int>)!);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.ObservableMethod("value1");

            // Then
            result.Should().BeNull();

            targetMock.Verify(
                target => target.ObservableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");

            NextNotCalled(interceptorsMock);

            CompleteCalled(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnIObservableWithStartError()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ObservableMethod(It.IsAny<string>()))
                .Throws<FormatException>();

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Action action = () => intercepted.ObservableMethod("value1");

            // Then
            action.Should().Throw<FormatException>();

            targetMock.Verify(
                target => target.ObservableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");

            NextNotCalled(interceptorsMock);

            CompleteNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");
        }

        [Test]
        public async Task InterceptEnumerableMethodTWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnIObservableWithMiddleErrorAsync()
        {
            // Given
            IObservable<int> MockEnumerable()
            {
                return Observable.Create<int>(observer =>
                {
                    observer.OnNext(1);
                    observer.OnNext(2);
                    observer.OnNext(3);
                    observer.OnError(new FormatException());
                    return () => { };
                });
            }

            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ObservableMethod(It.IsAny<string>()))
                .Returns((string _) => MockEnumerable());

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.ObservableMethod("value1");
            Func<Task> action = async () =>
            {
                var arr = await result.ToArray();
                arr.Should().Equal(new[] { 1, 2, 3, 4, 5 });
            };

            // Then
            await action.Should().ThrowAsync<FormatException>();

            targetMock.Verify(
                target => target.ObservableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");

            NextCalledTimes(interceptorsMock, 3, "STATE", targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 1, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 2, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");
            NextCalledWith(interceptorsMock, "STATE", 3, targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");

            CompleteNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.ObservableMethod), "value1");
        }

        [Test]
        public async Task InterceptEnumerableMethodTWithInterceptorsTurnedOffShouldNotCallInterceptorsOnIObservableAsync()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(false);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ObservableMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 }.ToObservable());

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.ObservableMethod("value1");

            // Then
            (await result.ToArray()).Should().Equal(new[] { 1, 2, 3, 4, 5 });

            targetMock.Verify(
                target => target.ObservableMethod("value1"),
                Times.Once);

            BeforeNotCalled(interceptorsMock);

            NextNotCalled(interceptorsMock);

            CompleteNotCalled(interceptorsMock);

            ErrorNotCalled(interceptorsMock);
        }

        #endregion [ IObservable<T> ]

        #region [ Configuration ]

        [Test]
        public void InterceptEnumerableMethodTWithNullInterceptorsShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.With(default!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTShouldIntercept0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.ShouldIntercept(default(Func<bool>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTShouldIntercept0WithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.ShouldIntercept(() => true);

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().NotBeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTShouldIntercept3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.ShouldIntercept(default(Func<object, MethodInfo, object?[]?, bool>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTShouldIntercept3WithActionShouldAssignShouldInterceptAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.ShouldIntercept((_, _, _) => true);

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().NotBeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTBefore0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.Before(default(Func<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTBefore0WithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.Before(() => "");

            // Then
            interceptor.BeforeAction.Should().NotBeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTBefore3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.Before(default(Func<object, MethodInfo, object?[]?, string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTBefore3WithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.Before((_, _, _) => "");

            // Then
            interceptor.BeforeAction.Should().NotBeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTNext0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.Next(default(Action<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTNext0WithActionShouldAssignNextAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.Next((_) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().NotBeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTNext1WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.Next(default(Action<string, object?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTNext1WithActionShouldAssignNextAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.Next((_, _) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().NotBeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTNext3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.Next(default(Action<string, object, MethodInfo, object?[]?, object?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTNext3WithActionShouldAssignNextAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.Next((_, _, _, _, _) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().NotBeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTComplete0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.Complete(default(Action<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTComplete0WithActionShouldAssignCompleteAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.Complete((_) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().NotBeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTComplete3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.Complete(default(Action<string, object, MethodInfo, object?[]?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTComplete3WithActionShouldAssignCompleteAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.Complete((_, _, _, _) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().NotBeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTError0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Action<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTError0WithActionShouldAssignErrorAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.Error((_) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().NotBeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTError1WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Action<string, Exception>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTError1WithActionShouldAssignErrorAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.Error((_, _) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().NotBeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodTError3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Action<string, object, MethodInfo, object?[]?, Exception>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptEnumerableMethodTError3WithActionShouldAssignErrorAction()
        {
            // Given
            var interceptor = InterceptEnumerableMethod<string>.Default;

            // When
            interceptor = interceptor.Error((_, _, _, _, _) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().NotBeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        #endregion [ Configuration ]

        #region [ Helpers ]

        private static void BeforeNotCalled(Mock<IInterceptEnumerableMethod<string>> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private static void BeforeCalled(Mock<IInterceptEnumerableMethod<string>> interceptorsMock, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 }),
                Times.Once);
        }

        private static void NextNotCalled(Mock<IInterceptEnumerableMethod<string>> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Next(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);
        }

        private static void NextCalledTimes(Mock<IInterceptEnumerableMethod<string>> interceptorsMock, int times, string state, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Next(
                    state, 
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 },
                    It.IsAny<object>()),
                Times.Exactly(times));
        }

        private static void NextCalledWith(Mock<IInterceptEnumerableMethod<string>> interceptorsMock, string state, object value, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Next(
                    state,
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 },
                    value),
                Times.Once);
        }

        private static void CompleteNotCalled(Mock<IInterceptEnumerableMethod<string>> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Complete(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private static void CompleteCalled(Mock<IInterceptEnumerableMethod<string>> interceptorsMock, string state, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Complete(
                    state,
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 }),
                Times.Once);
        }

        private static void ErrorNotCalled(Mock<IInterceptEnumerableMethod<string>> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Error(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        private static void ErrorCalled<TException>(Mock<IInterceptEnumerableMethod<string>> interceptorsMock, string state, object target, string methodName, string arg1)
            where TException : Exception
        {
            interceptorsMock.Verify(
                t => t.Error(
                    state,
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 },
                    It.IsAny<TException>()),
                Times.Once);
        }

        #endregion [ Helpers ]
    }
}
