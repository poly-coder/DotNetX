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
        public void InterceptSyncMethodDefaultShouldHaveAllActionsToNull()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptSyncMethodDefaultShouldNotFailWhenIntercepting()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.VoidMethod(It.IsAny<string>()));

            var intercepted =
                InterceptorOptions.Default
                    .Add(interceptor)
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);
        }

        [Test]
        public void InterceptSyncMethodWithInterceptorsShouldCallBeforeAndAfterInterceptorsOnVoidResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.VoidMethod(It.IsAny<string>()));

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.VoidMethod), "value1");

            AfterCalledWith(interceptorsMock, null, targetMock.Object, nameof(IDummyTarget.VoidMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptSyncMethodWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnThrow()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.VoidMethod(It.IsAny<string>()))
                .Callback<string>(value => { throw new FormatException($"Value = {value}"); });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod.Default.With(interceptorsMock.Object))
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

            ErrorCalled<FormatException>(interceptorsMock, targetMock.Object, nameof(IDummyTarget.VoidMethod), "value1");
        }

        [Test]
        public void InterceptSyncMethodWithInterceptorsShouldCallBeforeAndAfterInterceptorsOnIntResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.SyncMethod(It.IsAny<string>()))
                .Returns<string>(value => value.Length);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be("value1".Length);

            targetMock.Verify(
                target => target.SyncMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.SyncMethod), "value1");

            AfterCalledWith(interceptorsMock, 6, targetMock.Object, nameof(IDummyTarget.SyncMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptSyncMethodWithInterceptTurnedOffShouldNotCallBeforeAndAfter()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptor => interceptor.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(false);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.VoidMethod(It.IsAny<string>()));

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);

            BeforeNotCalled(interceptorsMock);

            AfterNotCalled(interceptorsMock);

            ErrorNotCalled(interceptorsMock);
        }

        #region [ Configuration ]

        [Test]
        public void InterceptSyncMethodWithNullInterceptorsShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.With(default!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodShouldIntercept0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.ShouldIntercept(default(Func<bool>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodShouldIntercept0WithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            interceptor = interceptor.ShouldIntercept(() => true);

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().NotBeNull();
        }

        [Test]
        public void InterceptSyncMethodShouldIntercept3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.ShouldIntercept(default(Func<object, MethodInfo, object?[]?, bool>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodShouldIntercept3WithActionShouldAssignShouldInterceptAction()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            interceptor = interceptor.ShouldIntercept((_, _, _) => true);

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().NotBeNull();
        }

        [Test]
        public void InterceptSyncMethodBefore0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.Before(default(Action)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodBefore0WithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            interceptor = interceptor.Before(() => { });

            // Then
            interceptor.BeforeAction.Should().NotBeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptSyncMethodBefore0WithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod.Default.Before(() => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptSyncMethodBefore3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.Before(default(Action<object, MethodInfo, object?[]?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodAfter0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.After(default(Action)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodAfter0WithActionShouldAssignAfterAction()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            interceptor = interceptor.After(() => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().NotBeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptSyncMethodAfter0WithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod.Default.After(() => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptSyncMethodAfter1WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.After(default(Action<object?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodAfter1WithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod.Default.After((_) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptSyncMethodAfter4WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.After(default(Action<object, MethodInfo, object?[]?, object?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodError0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.Error(default(Action)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodError0WithActionShouldAssignErrorAction()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            interceptor = interceptor.Error(() => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().NotBeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptSyncMethodError0WithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod.Default.Error(() => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            Action action = () => intercepted.VoidMethod("value1");

            // Then
            action.Should().Throw<Exception>();

            calls.Should().Be(1);
        }

        [Test]
        public void InterceptSyncMethodError1WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.Error(default(Action<Exception>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptSyncMethodError1WithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptSyncMethod.Default.Error((Exception _) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            Action action = () => intercepted.VoidMethod("value1");

            // Then
            action.Should().Throw<Exception>();

            calls.Should().Be(1);
        }

        [Test]
        public void InterceptSyncMethodError4WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptSyncMethod.Default;

            // When
            Action action = () => interceptor.Error(default(Action<object, MethodInfo, object?[]?, Exception>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        #endregion [ Configuration ]

        #region [ Helpers ]

        private static void BeforeNotCalled(Mock<IInterceptSyncMethod> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private static void BeforeCalled(Mock<IInterceptSyncMethod> interceptorsMock, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 }),
                Times.Once);
        }

        private static void AfterNotCalled(Mock<IInterceptSyncMethod> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.After(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);
        }

        private static void AfterCalledWith(Mock<IInterceptSyncMethod> interceptorsMock, object? result, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.After(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 },
                    result),
                Times.Once);
        }

        private static void ErrorNotCalled(Mock<IInterceptSyncMethod> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        private static void ErrorCalled<TException>(Mock<IInterceptSyncMethod> interceptorsMock, object target, string methodName, string arg1)
            where TException : Exception
        {
            interceptorsMock.Verify(
                t => t.Error(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 },
                    It.IsAny<TException>()),
                Times.Once);
        }

        #endregion [ Helpers ]
    }
}
