using FluentAssertions;
using FsCheck;
using FsCheck.NUnit;
using DotNetX.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;
using System.Collections;
using System.Threading.Tasks;
using System.ComponentModel.Design;
using Moq;

namespace DotNetX.Tests
{
    [TestFixture]
    public class InterceptorTests
    {
        #region [ Interceptor ]

        [Test]
        public void InterceptorWithoutInterceptorsShouldCallTargetMethod()
        {
            // Given
            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.VoidMethod(It.IsAny<string>()));

            var intercepted =
                InterceptorOptions.Default
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);
        }

        [Test]
        public void InterceptorWithAddRangeInterceptorsShouldWork()
        {
            // Given
            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(new FalseInterceptMethod())
                    .AddRange(new IInterceptMethod[]
                    {
                        new FalseInterceptMethod(),
                        InterceptSyncMethod.Default,
                        new ThrowInterceptMethod(),
                    })
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);
        }

        [Test]
        public void InterceptorWithPrependInterceptorsShouldWork()
        {
            // Given
            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(new ThrowInterceptMethod())
                    .Prepend(InterceptSyncMethod.Default)
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);
        }

        [Test]
        public void InterceptorWithNullOptionsShouldFail()
        {
            // Given
            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);

            // When
            Action action = () => default(InterceptorOptions)!.CreateInterceptor(targetMock.Object);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptorWithNullTargetShouldFail()
        {
            // When
            Action action = () => InterceptorOptions.Default.CreateInterceptor(default(IDummyTarget)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        #endregion [ Interceptor ]

        #region [ InterceptSyncMethod ]

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

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object, 
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.VoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.VoidMethod)),
                    new object[] { "value1" },
                    It.Is<FormatException>(ex => ex.Message == "Value = value1")),
                Times.Once);
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

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.SyncMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.SyncMethod)),
                    new object[] { "value1" },
                    "value1".Length),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
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

            interceptorsMock.Verify(
                target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]?>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]?>(),
                    It.IsAny<object?>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]?>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

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

        private static void CompleteNotCalled(Mock<IInterceptSyncMethod> interceptorsMock)
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

        #endregion [ InterceptSyncMethod ]

        #region [ InterceptSyncMethod<T> ]

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

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.VoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.VoidMethod)),
                    new object[] { "value1" },
                    null),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
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

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.VoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.VoidMethod)),
                    new object[] { "value1" },
                    It.Is<FormatException>(ex => ex.Message == "Value = value1")),
                Times.Once);
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

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.SyncMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.SyncMethod)),
                    new object[] { "value1" },
                    "value1".Length),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

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

        #endregion [ InterceptSyncMethod<T> ]

        #region [ InterceptAsyncMethod ]

        [Test]
        public void InterceptAsyncMethodDefaultShouldHaveAllActionsToNull()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public async Task InterceptAsyncMethodDefaultShouldNotFailWhenInterceptingTaskMethod()
        {
            // Given
            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskVoidMethod(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default)
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.TaskVoidMethod("value1"),
                Times.Once);
        }
        
        [Test]
        public async Task InterceptAsyncMethodDefaultShouldNotFailWhenInterceptingValueTaskMethod()
        {
            // Given
            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ValueTaskVoidMethod(It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default)
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.ValueTaskVoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.ValueTaskVoidMethod("value1"),
                Times.Once);
        }

        [Test]
        public async Task InterceptAsyncMethodWithSyncInterceptorsShouldCallBeforeAndAfterInterceptorsOnTaskVoidResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskVoidMethod(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.TaskVoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" },
                    null),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Test]
        public async Task InterceptAsyncMethodWithAsyncInterceptorsShouldCallBeforeAndAfterInterceptorsOnTaskVoidResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptAsyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskVoidMethod(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.TaskVoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" },
                    null),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Test]
        public async Task InterceptAsyncMethodWithSyncInterceptorsShouldCallBeforeAndAfterInterceptorsOnValueTaskVoidResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ValueTaskVoidMethod(It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.ValueTaskVoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.ValueTaskVoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskVoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskVoidMethod)),
                    new object[] { "value1" },
                    null),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Test]
        public async Task InterceptAsyncMethodWithSyncInterceptorsShouldCallBeforeAndErrorInterceptorsOnThrowTaskVoid()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async value => 
                {
                    await Task.CompletedTask;
                    throw new FormatException($"Value = {value}"); 
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            targetMock.Verify(
                target => target.TaskVoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" },
                    It.Is<FormatException>(ex => ex.Message == "Value = value1")),
                Times.Once);
        }

        [Test]
        public async Task InterceptAsyncMethodWithSyncInterceptorsShouldCallBeforeAndErrorInterceptorsOnThrowValueTaskVoid()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ValueTaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async value =>
                {
                    await ValueTask.CompletedTask;
                    throw new FormatException($"Value = {value}");
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = async () => await intercepted.ValueTaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            targetMock.Verify(
                target => target.ValueTaskVoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskVoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskVoidMethod)),
                    new object[] { "value1" },
                    It.Is<FormatException>(ex => ex.Message == "Value = value1")),
                Times.Once);
        }

        [Test]
        public async Task InterceptAsyncMethodWithSyncInterceptorsShouldCallBeforeAndAfterInterceptorsOnTaskIntResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskMethod(It.IsAny<string>()))
                .Returns(Task.FromResult(42));

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = await intercepted.TaskMethod("value1");

            // Then
            result.Should().Be(42);

            targetMock.Verify(
                target => target.TaskMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskMethod)),
                    new object[] { "value1" },
                    42),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Test]
        public async Task InterceptAsyncMethodWithSyncInterceptorsShouldCallBeforeAndAfterInterceptorsOnValueTaskIntResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ValueTaskMethod(It.IsAny<string>()))
                .Returns(ValueTask.FromResult(42));

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = await intercepted.ValueTaskMethod("value1");

            // Then
            result.Should().Be(42);

            targetMock.Verify(
                target => target.ValueTaskMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskMethod)),
                    new object[] { "value1" },
                    42),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Test]
        public async Task InterceptAsyncMethodWithSyncInterceptorsShouldCallBeforeAndErrorInterceptorsOnThrowTaskInt()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskMethod(It.IsAny<string>()))
                .Returns<string>(async value =>
                {
                    await Task.CompletedTask;
                    throw new FormatException($"Value = {value}");
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            targetMock.Verify(
                target => target.TaskMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskMethod)),
                    new object[] { "value1" },
                    It.Is<FormatException>(ex => ex.Message == "Value = value1")),
                Times.Once);
        }

        [Test]
        public async Task InterceptAsyncMethodWithSyncInterceptorsShouldCallBeforeAndErrorInterceptorsOnThrowValueTaskInt()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ValueTaskMethod(It.IsAny<string>()))
                .Returns<string>(async value =>
                {
                    await ValueTask.CompletedTask;
                    throw new FormatException($"Value = {value}");
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = async () => await intercepted.ValueTaskMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            targetMock.Verify(
                target => target.ValueTaskMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskMethod)),
                    new object[] { "value1" },
                    It.Is<FormatException>(ex => ex.Message == "Value = value1")),
                Times.Once);
        }

        [Test]
        public void InterceptAsyncMethodWithNullSyncInterceptorsShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.With(default(IInterceptSyncMethod)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Test]
        public void InterceptAsyncMethodWithNullAsyncInterceptorsShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.With(default(IInterceptAsyncMethod)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodShouldIntercept0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.ShouldIntercept(default(Func<bool>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodShouldIntercept0WithActionShouldAssignShouldInterceptAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            interceptor = interceptor.ShouldIntercept(() => true);

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().NotBeNull();
        }

        [Test]
        public void InterceptAsyncMethodShouldIntercept3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.ShouldIntercept(default(Func<object, MethodInfo, object?[]?, bool>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodShouldIntercept3WithActionShouldAssignShouldInterceptAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            interceptor = interceptor.ShouldIntercept((_, _, _) => true);

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().NotBeNull();
        }

        [Test]
        public void InterceptAsyncMethodBefore0AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.Before(default(Func<Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodBefore0SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.Before(default(Action)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodBefore0AsyncWithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            interceptor = interceptor.Before(async () => { await Task.CompletedTask; });

            // Then
            interceptor.BeforeAction.Should().NotBeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptAsyncMethodBefore0SyncWithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            interceptor = interceptor.Before(() => { });

            // Then
            interceptor.BeforeAction.Should().NotBeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public async Task InterceptAsyncMethodBefore0AsyncWithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.Before(async () => 
                    { 
                        calls++;
                        await Task.CompletedTask;
                    }))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }
        
        [Test]
        public async Task InterceptAsyncMethodBefore0SyncWithActionShouldCallInterceptorAsync()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.Before(() => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptAsyncMethodBefore3AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.Before(default(Func<object, MethodInfo, object?[]?, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodBefore3SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.Before(default(Action<object, MethodInfo, object?[]?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Test]
        public void InterceptAsyncMethodAfter0AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.After(default(Func<Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodAfter0SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.After(default(Action)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodAfter0AsyncWithActionShouldAssignAfterAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            interceptor = interceptor.After(async () => { await Task.CompletedTask; });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().NotBeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptAsyncMethodAfter0SyncWithActionShouldAssignAfterAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            interceptor = interceptor.After(() => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().NotBeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public async Task InterceptAsyncMethodAfter0AsyncWithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.After(async () => 
                    { 
                        calls++;
                        await Task.CompletedTask;
                    }))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }
        
        [Test]
        public async Task InterceptAsyncMethodAfter0SyncWithActionShouldCallInterceptorAsync()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.After(() => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptAsyncMethodAfter1AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.After(default(Func<object?, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodAfter1SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.After(default(Action<object?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task InterceptAsyncMethodAfter1AsyncWithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.After(async (result) => 
                    { 
                        calls++;
                        await Task.CompletedTask;
                    }))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }
        
        [Test]
        public async Task InterceptAsyncMethodAfter1SyncWithActionShouldCallInterceptorAsync()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.After((result) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptAsyncMethodAfter3AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.After(default(Func<object, MethodInfo, object?[]?, object?, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodAfter3SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.After(default(Action<object, MethodInfo, object?[]?, object?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodError0AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.Error(default(Func<Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodError0SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.Error(default(Action)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodError0AsyncWithActionShouldAssignErrorAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            interceptor = interceptor.Error(async () => { await Task.CompletedTask; });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().NotBeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptAsyncMethodError0SyncWithActionShouldAssignErrorAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            interceptor = interceptor.Error(() => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().NotBeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public async Task InterceptAsyncMethodError0AsyncWithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);
            targetMock
                .Setup(t => t.TaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async _ =>
                {
                    await Task.CompletedTask;
                    throw new FormatException();
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.Error(async () => 
                    { 
                        calls++;
                        await Task.CompletedTask;
                    }))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            calls.Should().Be(1);
        }
        
        [Test]
        public async Task InterceptAsyncMethodError0SyncWithActionShouldCallInterceptorAsync()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);
            targetMock
                .Setup(t => t.TaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async _ =>
                {
                    await Task.CompletedTask;
                    throw new FormatException();
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.Error(() => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            calls.Should().Be(1);
        }

        [Test]
        public void InterceptAsyncMethodError1AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.Error(default(Func<Exception, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodError1SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.Error(default(Action<Exception>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task InterceptAsyncMethodError1AsyncWithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);
            targetMock
                .Setup(t => t.TaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async _ =>
                {
                    await Task.CompletedTask;
                    throw new FormatException();
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.Error(async (exception) => 
                    { 
                        calls++;
                        await Task.CompletedTask;
                    }))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            calls.Should().Be(1);
        }
        
        [Test]
        public async Task InterceptAsyncMethodError1SyncWithActionShouldCallInterceptorAsync()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);
            targetMock
                .Setup(t => t.TaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async _ =>
                {
                    await Task.CompletedTask;
                    throw new FormatException();
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.Error((exception) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            calls.Should().Be(1);
        }

        [Test]
        public void InterceptAsyncMethodError3AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.Error(default(Func<object, MethodInfo, object?[]?, Exception, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodError3SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod.Default;

            // When
            Action action = () => interceptor.Error(default(Action<object, MethodInfo, object?[]?, Exception>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodWithSyncInterceptorsShouldNotInterceptSyncMethods()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod>(MockBehavior.Loose);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        #endregion [ InterceptAsyncMethod ]

        #region [ InterceptAsyncMethod<T> ]

        [Test]
        public void InterceptAsyncMethodTDefaultShouldHaveAllActionsToNull()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public async Task InterceptAsyncMethodTDefaultShouldNotFailWhenInterceptingTaskMethod()
        {
            // Given
            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskVoidMethod(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default)
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.TaskVoidMethod("value1"),
                Times.Once);
        }

        [Test]
        public async Task InterceptAsyncMethodTDefaultShouldNotFailWhenInterceptingValueTaskMethod()
        {
            // Given
            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ValueTaskVoidMethod(It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default)
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.ValueTaskVoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.ValueTaskVoidMethod("value1"),
                Times.Once);
        }

        [Test]
        public async Task InterceptAsyncMethodTWithSyncInterceptorsShouldCallBeforeAndAfterInterceptorsOnTaskVoidResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskVoidMethod(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.TaskVoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" },
                    null),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    "STATE",
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Test]
        public async Task InterceptAsyncMethodTWithAsyncInterceptorsShouldCallBeforeAndAfterInterceptorsOnTaskVoidResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptAsyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]>()))
                .Returns(Task.FromResult("STATE"));

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskVoidMethod(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.TaskVoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" },
                    null),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    "STATE",
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Test]
        public async Task InterceptAsyncMethodTWithSyncInterceptorsShouldCallBeforeAndAfterInterceptorsOnValueTaskVoidResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ValueTaskVoidMethod(It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.ValueTaskVoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.ValueTaskVoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskVoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskVoidMethod)),
                    new object[] { "value1" },
                    null),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    "STATE",
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Test]
        public async Task InterceptAsyncMethodTWithSyncInterceptorsShouldCallBeforeAndErrorInterceptorsOnThrowTaskVoid()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async value =>
                {
                    await Task.CompletedTask;
                    throw new FormatException($"Value = {value}");
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            targetMock.Verify(
                target => target.TaskVoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    "STATE",
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskVoidMethod)),
                    new object[] { "value1" },
                    It.Is<FormatException>(ex => ex.Message == "Value = value1")),
                Times.Once);
        }

        [Test]
        public async Task InterceptAsyncMethodTWithSyncInterceptorsShouldCallBeforeAndErrorInterceptorsOnThrowValueTaskVoid()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ValueTaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async value =>
                {
                    await ValueTask.CompletedTask;
                    throw new FormatException($"Value = {value}");
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = async () => await intercepted.ValueTaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            targetMock.Verify(
                target => target.ValueTaskVoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskVoidMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskVoidMethod)),
                    new object[] { "value1" },
                    It.Is<FormatException>(ex => ex.Message == "Value = value1")),
                Times.Once);
        }

        [Test]
        public async Task InterceptAsyncMethodTWithSyncInterceptorsShouldCallBeforeAndAfterInterceptorsOnTaskIntResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskMethod(It.IsAny<string>()))
                .Returns(Task.FromResult(42));

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = await intercepted.TaskMethod("value1");

            // Then
            result.Should().Be(42);

            targetMock.Verify(
                target => target.TaskMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskMethod)),
                    new object[] { "value1" },
                    42),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Test]
        public async Task InterceptAsyncMethodTWithSyncInterceptorsShouldCallBeforeAndAfterInterceptorsOnValueTaskIntResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ValueTaskMethod(It.IsAny<string>()))
                .Returns(ValueTask.FromResult(42));

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = await intercepted.ValueTaskMethod("value1");

            // Then
            result.Should().Be(42);

            targetMock.Verify(
                target => target.ValueTaskMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskMethod)),
                    new object[] { "value1" },
                    42),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Test]
        public async Task InterceptAsyncMethodTWithSyncInterceptorsShouldCallBeforeAndErrorInterceptorsOnThrowTaskInt()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.TaskMethod(It.IsAny<string>()))
                .Returns<string>(async value =>
                {
                    await Task.CompletedTask;
                    throw new FormatException($"Value = {value}");
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            targetMock.Verify(
                target => target.TaskMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.TaskMethod)),
                    new object[] { "value1" },
                    It.Is<FormatException>(ex => ex.Message == "Value = value1")),
                Times.Once);
        }

        [Test]
        public async Task InterceptAsyncMethodTWithSyncInterceptorsShouldCallBeforeAndErrorInterceptorsOnThrowValueTaskInt()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);
            interceptorsMock
                .Setup(target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?[]>()))
                .Returns("STATE");

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.ValueTaskMethod(It.IsAny<string>()))
                .Returns<string>(async value =>
                {
                    await ValueTask.CompletedTask;
                    throw new FormatException($"Value = {value}");
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = async () => await intercepted.ValueTaskMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            targetMock.Verify(
                target => target.ValueTaskMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskMethod)),
                    new object[] { "value1" }),
                Times.Once);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    "STATE",
                    targetMock.Object,
                    It.Is<MethodInfo>(m => m.Name == nameof(IDummyTarget.ValueTaskMethod)),
                    new object[] { "value1" },
                    It.Is<FormatException>(ex => ex.Message == "Value = value1")),
                Times.Once);
        }

        [Test]
        public void InterceptAsyncMethodTWithNullSyncInterceptorsShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.With(default(IInterceptSyncMethod<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTWithNullAsyncInterceptorsShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.With(default(IInterceptAsyncMethod<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTShouldIntercept0WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.ShouldIntercept(default(Func<bool>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTShouldIntercept0WithActionShouldAssignShouldInterceptAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            interceptor = interceptor.ShouldIntercept(() => true);

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().NotBeNull();
        }

        [Test]
        public void InterceptAsyncMethodTShouldIntercept3WithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.ShouldIntercept(default(Func<object, MethodInfo, object?[]?, bool>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTShouldIntercept3WithActionShouldAssignShouldInterceptAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            interceptor = interceptor.ShouldIntercept((_, _, _) => true);

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().NotBeNull();
        }

        [Test]
        public void InterceptAsyncMethodTBefore0AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Before(default(Func<Task<string>>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTBefore0SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Before(default(Func<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTBefore0AsyncWithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            interceptor = interceptor.Before(async () => { await Task.CompletedTask; return "STATE"; });

            // Then
            interceptor.BeforeAction.Should().NotBeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptAsyncMethodTBefore0SyncWithActionShouldAssignBeforeAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            interceptor = interceptor.Before(() => "STATE");

            // Then
            interceptor.BeforeAction.Should().NotBeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public async Task InterceptAsyncMethodTBefore0AsyncWithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.Before(async () =>
                    {
                        calls++;
                        await Task.CompletedTask;
                        return "STATE";
                    }))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public async Task InterceptAsyncMethodTBefore0SyncWithActionShouldCallInterceptorAsync()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.Before(() => { calls++; return "STATE"; }))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptAsyncMethodTBefore3AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Before(default(Func<object, MethodInfo, object?[]?, Task<string>>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTBefore3SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Before(default(Func<object, MethodInfo, object?[]?, string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTAfter0AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.After(default(Func<string, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTAfter0SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.After(default(Action<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTAfter0AsyncWithActionShouldAssignAfterAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            interceptor = interceptor.After(async (state) => { await Task.CompletedTask; });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().NotBeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptAsyncMethodTAfter0SyncWithActionShouldAssignAfterAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            interceptor = interceptor.After((state) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().NotBeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public async Task InterceptAsyncMethodTAfter0AsyncWithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default
                        .Before(() => "STATE")
                        .After(async (state) =>
                        {
                            calls++;
                            await Task.CompletedTask;
                        }))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public async Task InterceptAsyncMethodTAfter0SyncWithActionShouldCallInterceptorAsync()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default
                        .Before(() => "STATE")
                        .After((state) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptAsyncMethodTAfter1AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.After(default(Func<string, object?, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTAfter1SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.After(default(Action<string, object?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task InterceptAsyncMethodTAfter1AsyncWithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default
                        .Before(() => "STATE")
                        .After(async (state, result) =>
                        {
                            calls++;
                            await Task.CompletedTask;
                        }))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public async Task InterceptAsyncMethodTAfter1SyncWithActionShouldCallInterceptorAsync()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default
                        .Before(() => "STATE")
                        .After((state, result) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            calls.Should().Be(1);
        }

        [Test]
        public void InterceptAsyncMethodTAfter3AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.After(default(Func<string, object, MethodInfo, object?[]?, object?, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTAfter3SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.After(default(Action<string, object, MethodInfo, object?[]?, object?>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTError0AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Func<string, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTError0SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Action<string>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTError0AsyncWithActionShouldAssignErrorAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            interceptor = interceptor.Error(async (state) => { await Task.CompletedTask; });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().NotBeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptAsyncMethodTError0SyncWithActionShouldAssignErrorAction()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            interceptor = interceptor.Error((state) => { });

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.AfterAction.Should().BeNull();
            interceptor.ErrorAction.Should().NotBeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public async Task InterceptAsyncMethodTError0AsyncWithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);
            targetMock
                .Setup(t => t.TaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async _ =>
                {
                    await Task.CompletedTask;
                    throw new FormatException();
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default
                        .Before(() => "STATE")
                        .Error(async (state) =>
                        {
                            calls++;
                            await Task.CompletedTask;
                        }))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            calls.Should().Be(1);
        }

        [Test]
        public async Task InterceptAsyncMethodTError0SyncWithActionShouldCallInterceptorAsync()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);
            targetMock
                .Setup(t => t.TaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async _ =>
                {
                    await Task.CompletedTask;
                    throw new FormatException();
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default
                        .Before(() => "STATE")
                        .Error((state) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            calls.Should().Be(1);
        }

        [Test]
        public void InterceptAsyncMethodTError1AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Func<string, Exception, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTError1SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Action<string, Exception>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task InterceptAsyncMethodTError1AsyncWithActionShouldCallInterceptor()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);
            targetMock
                .Setup(t => t.TaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async _ =>
                {
                    await Task.CompletedTask;
                    throw new FormatException();
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default
                        .Before(() => "STATE")
                        .Error(async (state, exception) =>
                        {
                            calls++;
                            await Task.CompletedTask;
                        }))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            calls.Should().Be(1);
        }

        [Test]
        public async Task InterceptAsyncMethodTError1SyncWithActionShouldCallInterceptorAsync()
        {
            // Given
            var calls = 0;

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);
            targetMock
                .Setup(t => t.TaskVoidMethod(It.IsAny<string>()))
                .Returns<string>(async _ =>
                {
                    await Task.CompletedTask;
                    throw new FormatException();
                });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default
                        .Before(() => "STATE")
                        .Error((state, exception) => calls++))
                    .CreateInterceptor(targetMock.Object);

            // When
            Func<Task> action = () => intercepted.TaskVoidMethod("value1");

            // Then
            await action.Should().ThrowAsync<FormatException>();

            calls.Should().Be(1);
        }

        [Test]
        public void InterceptAsyncMethodTError3AsyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Func<string, object, MethodInfo, object?[]?, Exception, Task>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTError3SyncWithNullShouldFail()
        {
            // Given
            var interceptor = InterceptAsyncMethod<string>.Default;

            // When
            Action action = () => interceptor.Error(default(Action<string, object, MethodInfo, object?[]?, Exception>)!);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void InterceptAsyncMethodTWithSyncInterceptorsShouldNotInterceptSyncMethods()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptSyncMethod<string>>(MockBehavior.Loose);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptAsyncMethod<string>.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            targetMock.Verify(
                target => target.VoidMethod("value1"),
                Times.Once);

            interceptorsMock.Verify(
                target => target.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.After(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);

            interceptorsMock.Verify(
                target => target.Error(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        #endregion [ InterceptAsyncMethod ]

        #region [ InterceptEnumerableMethod ]


        [Test]
        public void InterceptEnumerableMethodDefaultShouldHaveAllActionsToNull()
        {
            // Given
            var interceptor = InterceptEnumerableMethod.Default;

            // Then
            interceptor.BeforeAction.Should().BeNull();
            interceptor.NextAction.Should().BeNull();
            interceptor.CompleteAction.Should().BeNull();
            interceptor.ErrorAction.Should().BeNull();
            interceptor.ShouldInterceptAction.Should().BeNull();
        }

        [Test]
        public void InterceptEnumerableMethodDefaultShouldNotFailWhenIntercepting()
        {
            // Given
            var interceptor = InterceptEnumerableMethod.Default;

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

        [Test]
        public void InterceptEnumerableMethodWithInterceptorsShouldCallBeforeNextAndCompleteInterceptorsOnIEnumerableResult()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod.Default.With(interceptorsMock.Object))
                    .CreateInterceptor(targetMock.Object);

            // When
            var result = intercepted.EnumerableMethod("value1");

            // Then
            result.Should().Equal(new[] { 1, 2, 3, 4, 5 });

            targetMock.Verify(
                target => target.EnumerableMethod("value1"),
                Times.Once);

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            
            NextCalledTimes(interceptorsMock, 5, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, 1, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, 2, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, 3, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, 4, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, 5, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            CompleteCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptEnumerableMethodWithInterceptorsShouldCallBeforeAndCompleteInterceptorsOnIEnumerableResultWithNull()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Returns(default(IEnumerable)!);

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod.Default.With(interceptorsMock.Object))
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

            CompleteCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            ErrorNotCalled(interceptorsMock);
        }

        [Test]
        public void InterceptEnumerableMethodWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnIEnumerableResultWithStartError()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Throws<FormatException>();

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod.Default.With(interceptorsMock.Object))
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

            ErrorCalled<FormatException>(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
        }

        [Test]
        public void InterceptEnumerableMethodWithInterceptorsShouldCallBeforeAndErrorInterceptorsOnIEnumerableResultWithMiddleError()
        {
            // Given
            IEnumerable MockEnumerable()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                throw new FormatException();
            }

            var interceptorsMock = new Mock<IInterceptEnumerableMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(true);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Returns((string _) => MockEnumerable());

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod.Default.With(interceptorsMock.Object))
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

            NextCalledTimes(interceptorsMock, 3, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, 1, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, 2, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
            NextCalledWith(interceptorsMock, 3, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");

            CompleteNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, targetMock.Object, nameof(IDummyTarget.EnumerableMethod), "value1");
        }

        [Test]
        public void InterceptEnumerableMethodWithInterceptorsTurnedOffShouldNotCallInterceptorsOnIEnumerable()
        {
            // Given
            var interceptorsMock = new Mock<IInterceptEnumerableMethod>(MockBehavior.Loose);
            interceptorsMock
                .Setup(interceptors => interceptors.ShouldIntercept(It.IsAny<object>(), It.IsAny<MethodInfo>(), It.IsAny<object?[]?>()))
                .Returns(false);

            var targetMock = new Mock<IDummyTarget>(MockBehavior.Strict);
            targetMock
                .Setup(target => target.EnumerableMethod(It.IsAny<string>()))
                .Returns(new[] { 1, 2, 3, 4, 5 });

            var intercepted =
                InterceptorOptions.Default
                    .Add(InterceptEnumerableMethod.Default.With(interceptorsMock.Object))
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

        //[Test]
        //public void InterceptEnumerableMethodWithNullInterceptorsShouldFail()
        //{
        //    // Given
        //    var interceptor = InterceptEnumerableMethod.Default;

        //    // When
        //    Action action = () => interceptor.With(default!);

        //    // Then
        //    action.Should().Throw<ArgumentNullException>();
        //}

        //[Test]
        //public void InterceptEnumerableMethodShouldIntercept0WithNullShouldFail()
        //{
        //    // Given
        //    var interceptor = InterceptEnumerableMethod.Default;

        //    // When
        //    Action action = () => interceptor.ShouldIntercept(default(Func<bool>)!);

        //    // Then
        //    action.Should().Throw<ArgumentNullException>();
        //}

        //[Test]
        //public void InterceptEnumerableMethodShouldIntercept0WithActionShouldAssignBeforeAction()
        //{
        //    // Given
        //    var interceptor = InterceptEnumerableMethod.Default;

        //    // When
        //    interceptor = interceptor.ShouldIntercept(() => true);

        //    // Then
        //    interceptor.BeforeAction.Should().BeNull();
        //    interceptor.AfterAction.Should().BeNull();
        //    interceptor.ErrorAction.Should().BeNull();
        //    interceptor.ShouldInterceptAction.Should().NotBeNull();
        //}

        //[Test]
        //public void InterceptEnumerableMethodShouldIntercept3WithNullShouldFail()
        //{
        //    // Given
        //    var interceptor = InterceptEnumerableMethod.Default;

        //    // When
        //    Action action = () => interceptor.ShouldIntercept(default(Func<object, MethodInfo, object?[]?, bool>)!);

        //    // Then
        //    action.Should().Throw<ArgumentNullException>();
        //}

        //[Test]
        //public void InterceptEnumerableMethodShouldIntercept3WithActionShouldAssignShouldInterceptAction()
        //{
        //    // Given
        //    var interceptor = InterceptEnumerableMethod.Default;

        //    // When
        //    interceptor = interceptor.ShouldIntercept((_, _, _) => true);

        //    // Then
        //    interceptor.BeforeAction.Should().BeNull();
        //    interceptor.AfterAction.Should().BeNull();
        //    interceptor.ErrorAction.Should().BeNull();
        //    interceptor.ShouldInterceptAction.Should().NotBeNull();
        //}

        #region [ Helpers ]

        private static void BeforeNotCalled(Mock<IInterceptEnumerableMethod> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private static void BeforeCalled(Mock<IInterceptEnumerableMethod> interceptorsMock, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 }),
                Times.Once);
        }

        private static void NextNotCalled(Mock<IInterceptEnumerableMethod> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Next(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);
        }

        private static void NextCalledTimes(Mock<IInterceptEnumerableMethod> interceptorsMock, int times, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Next(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 },
                    It.IsAny<object>()),
                Times.Exactly(times));
        }

        private static void NextCalledWith(Mock<IInterceptEnumerableMethod> interceptorsMock, object value, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Next(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 },
                    value),
                Times.Once);
        }

        private static void CompleteNotCalled(Mock<IInterceptEnumerableMethod> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Complete(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private static void CompleteCalled(Mock<IInterceptEnumerableMethod> interceptorsMock, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Complete(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 }),
                Times.Once);
        }

        private static void ErrorNotCalled(Mock<IInterceptEnumerableMethod> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        private static void ErrorCalled<TException>(Mock<IInterceptEnumerableMethod> interceptorsMock, object target, string methodName, string arg1)
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

        #endregion [ InterceptEnumerableMethod ]

        public interface IDummyTarget
        {
            void VoidMethod(string arg1);

            int SyncMethod(string arg1);

            Task TaskVoidMethod(string arg1);

            Task<int> TaskMethod(string arg1);

            ValueTask ValueTaskVoidMethod(string arg1);

            ValueTask<int> ValueTaskMethod(string arg1);

            IEnumerable EnumerableMethod(string arg1);

            IEnumerable<int> IEnumerableOfTMethod(string arg1);

            IAsyncEnumerable<int> IAsyncEnumerableMethod(string arg1);
            
            IObservable<int> IObserableMethod(string arg1);
        }

        public record ThrowInterceptMethod() : IInterceptMethod
        {
            public bool TryToIntercept(object target, MethodInfo targetMethod, object?[]? args, out object? result)
            {
                throw new InvalidOperationException();
            }
        }

        public record FalseInterceptMethod() : IInterceptMethod
        {
            public bool TryToIntercept(object target, MethodInfo targetMethod, object?[]? args, out object? result)
            {
                result = null;
                return false;
            }
        }

        public record TrueInterceptMethod(object? Value) : IInterceptMethod
        {
            public bool TryToIntercept(object target, MethodInfo targetMethod, object?[]? args, out object? result)
            {
                result = Value;
                return true;
            }
        }
    }
}
