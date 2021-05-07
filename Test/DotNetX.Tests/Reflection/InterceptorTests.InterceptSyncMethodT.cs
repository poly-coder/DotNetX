using DotNetX.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Reflection;

namespace DotNetX.Tests
{
    partial class InterceptorTests
    {
        [Test]
        public void InterceptSyncMethodTStateDefaultShouldHaveAllActionsToNull()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptSyncMethodTStateDefaultShouldNotFailWhenIntercepting()
        {
            // Given
            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.VoidMethod(It.IsAny<string>()));

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod<string>.Default)
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);
        }

        [Test]
        public void InterceptSyncMethodTStateDefaultShouldThrowWhenInterceptingAThrow()
        {
            // Given
            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.VoidMethod(It.IsAny<string>()))
                .Callback<string>(value => { throw new FormatException($"Value = {value}"); });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod<string>.Default)
                    .CreateInterceptor(targetMock.Object);

            // When
            Action action = () => intercepted.VoidMethod("value1");

            // Then
            action.Should().Throw<FormatException>();

            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);
        }

        [Test]
        public void InterceptSyncMethodTStateWithInterceptorsShouldCallBeforeAndAfterInterceptorsOnVoidResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.VoidMethod(It.IsAny<string>()));

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.VoidMethod), "value1");

            AfterCalledWith(interceptorsMock, "STATE", null, targetMock.Object, nameof(IDummyTarget.VoidMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptSyncMethodTStateWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnThrow()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.VoidMethod(It.IsAny<string>()))
                .Callback<string>(value => { throw new FormatException($"Value = {value}"); });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Action action = () => intercepted.VoidMethod("value1");

            // Then
            action.Should().Throw<FormatException>();

            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.VoidMethod), "value1");

            AfterNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.VoidMethod), "value1");
        }

        [Test]
        public void InterceptSyncMethodTStateWithInterceptorsShouldCallBeforeAndAfterInterceptorsOnIntResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.Before(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns("STATE");
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.SyncMethod(It.IsAny<string>()))
                .Returns<string>(value => value.Length);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be("value1".Length);

            targetMock.Verify(
                target => target.SyncMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.SyncMethod), "value1");

            AfterCalledWith(interceptorsMock, "STATE", 6, targetMock.Object, nameof(IDummyTarget.SyncMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        #region [ Configuration ]

        [Test]
        public void InterceptSyncMethodTStateWithNullInterceptorsShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.With(default!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodTStateShouldIntercept0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.ShouldIntercept(default(Func<bool>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodTStateShouldIntercept0WithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            interceptor = interceptor.ShouldIntercept(() => true);

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().NotBeNull();
        }

        [Test]
        public void InterceptSyncMethodTStateShouldIntercept3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.ShouldIntercept(default(Func<object, MethodInfo, object?[]?, bool>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodTStateShouldIntercept3WithActionShouldAssignShouldInterceptAction()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            interceptor = interceptor.ShouldIntercept((_, _, _) => true);

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().NotBeNull();
        }

        [Test]
        public void InterceptSyncMethodTStateBefore0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Before(default(Func<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodTStateBefore0WithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            interceptor = interceptor.Before(() => "");

            // Then
            interceptor.BeforeAction.Should().NotBeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptSyncMethodTStateBefore0WithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod<string>.Default.Before(() => { calls++; return "STATE"; }))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptSyncMethodTStateBefore3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Before(default(Func<object, MethodInfo, object?[]?, string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodTStateAfter0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.After(default(Action<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodTStateAfter0WithActionShouldAssignAfterAction()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            interceptor = interceptor.After((_) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().NotBeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptSyncMethodTStateAfter0WithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod<string>.Default.Before(() => "").After((_) => { calls++; }))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptSyncMethodTStateAfter1WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.After(default(Action<string, object?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodTStateAfter1WithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod<string>.Default.Before(() => "").After((_, _) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptSyncMethodTStateAfter4WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.After(default(Action<string, object, MethodInfo, object?[]?, object?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodTStateError0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Action<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodTStateError0WithActionShouldAssignErrorAction()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            interceptor = interceptor.Error((_) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().NotBeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptSyncMethodTStateError0WithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod<string>.Default.Before(() => "").Error((_) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            Action action = () => intercepted.VoidMethod("value1");

            // Then
            action.Should().Throw<Exception>();

            calls.Should().Be(1);
        }

        [Test]
        public void InterceptSyncMethodTStateError1WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Action<string, Exception>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodTStateError1WithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod<string>.Default.Before(() => "").Error((string _, Exception _) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            Action action = () => intercepted.VoidMethod("value1");

            // Then
            action.Should().Throw<Exception>();

            calls.Should().Be(1);
        }

        [Test]
        public void InterceptSyncMethodTStateError4WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Action<string, object, MethodInfo, object?[]?, Exception>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        #endregion [ Configuration ]

        #region [ Helpers ]

        private static void BeforeNotCalled(Mock<IInterceptSyncMethod<string>> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private static void BeforeCalled(Mock<IInterceptSyncMethod<string>> interceptorsMock, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 }),
                Times.Once);
        }

        private static void AfterNotCalled(Mock<IInterceptSyncMethod<string>> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.After(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);
        }

        private static void AfterCalledWith(Mock<IInterceptSyncMethod<string>> interceptorsMock, string state, object? result, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.After(
                    state,
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 },
                    result),
                Times.Once);
        }

        private static void ErrorNotCalled(Mock<IInterceptSyncMethod<string>> interceptorsMock)
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

        private static void ErrorCalled<TException>(Mock<IInterceptSyncMethod<string>> interceptorsMock, string state, object target, string methodName, string arg1)
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
