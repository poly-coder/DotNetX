using DotNetX.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetX.Tests
{
    partial class InterceptorTests
    {

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

        #region [ Task ]

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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");

            AfterCalledWith(interceptorsMock, null, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");

            ErrorNotCalled(interceptorsMock);
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");

            AfterCalledWith(interceptorsMock, null, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");

            ErrorNotCalled(interceptorsMock);
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");

            AfterNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskMethod), "value1");

            AfterCalledWith(interceptorsMock, 42, targetMock.Object, nameof(IDummyTarget.TaskMethod), "value1");

            ErrorNotCalled(interceptorsMock);
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskMethod), "value1");

            AfterNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskMethod), "value1");
        }

        #endregion [ Task ]

        #region [ ValueTask ]

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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ValueTaskVoidMethod), "value1");

            AfterCalledWith(interceptorsMock, null, targetMock.Object, nameof(IDummyTarget.ValueTaskVoidMethod), "value1");

            ErrorNotCalled(interceptorsMock);
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ValueTaskVoidMethod), "value1");

            AfterNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ValueTaskVoidMethod), "value1");
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ValueTaskMethod), "value1");

            AfterCalledWith(interceptorsMock, 42, targetMock.Object, nameof(IDummyTarget.ValueTaskMethod), "value1");

            ErrorNotCalled(interceptorsMock);
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ValueTaskMethod), "value1");

            AfterNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ValueTaskMethod), "value1");
        }

        #endregion [ ValueTask ]

        #region [ Configuration ]

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

        #endregion [ Configuration ]

        #region [ Helpers ]

        private static void BeforeNotCalled(Mock<IInterceptAsyncMethod> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private static void BeforeCalled(Mock<IInterceptAsyncMethod> interceptorsMock, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 }),
                Times.Once);
        }

        private static void AfterNotCalled(Mock<IInterceptAsyncMethod> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.After(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<object>()),
                Times.Never);
        }

        private static void AfterCalledWith(Mock<IInterceptAsyncMethod> interceptorsMock, object? result, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.After(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 },
                    result),
                Times.Once);
        }

        private static void ErrorNotCalled(Mock<IInterceptAsyncMethod> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Error(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        private static void ErrorCalled<TException>(Mock<IInterceptAsyncMethod> interceptorsMock, object target, string methodName, string arg1)
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
