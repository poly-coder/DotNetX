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

        #region [ Task ]

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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");

            AfterCalledWith(interceptorsMock, "STATE", null, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");

            ErrorNotCalled(interceptorsMock);
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");

            AfterCalledWith(interceptorsMock, "STATE", null, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");

            ErrorNotCalled(interceptorsMock);
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");

            AfterNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.TaskVoidMethod), "value1");
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskMethod), "value1");

            AfterCalledWith(interceptorsMock, "STATE", 42, targetMock.Object, nameof(IDummyTarget.TaskMethod), "value1");

            ErrorNotCalled(interceptorsMock);
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.TaskMethod), "value1");

            AfterNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.TaskMethod), "value1");
        }

        #endregion [ Task ]

        #region [ ValueTask ]

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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ValueTaskVoidMethod), "value1");

            AfterCalledWith(interceptorsMock, "STATE", null, targetMock.Object, nameof(IDummyTarget.ValueTaskVoidMethod), "value1");

            ErrorNotCalled(interceptorsMock);
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ValueTaskVoidMethod), "value1");

            AfterNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.ValueTaskVoidMethod), "value1");
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ValueTaskMethod), "value1");

            AfterCalledWith(interceptorsMock, "STATE", 42, targetMock.Object, nameof(IDummyTarget.ValueTaskMethod), "value1");

            ErrorNotCalled(interceptorsMock);
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

            BeforeCalled(interceptorsMock, targetMock.Object, nameof(IDummyTarget.ValueTaskMethod), "value1");

            AfterNotCalled(interceptorsMock);

            ErrorCalled<FormatException>(interceptorsMock, "STATE", targetMock.Object, nameof(IDummyTarget.ValueTaskMethod), "value1");
        }

        #endregion [ ValueTask ]

        #region [ Configuration ]

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

            BeforeNotCalled(interceptorsMock);

            AfterNotCalled(interceptorsMock);

            ErrorNotCalled(interceptorsMock);
        }

        #endregion [ Configuration ]

        #region [ Helpers ]

        private static void BeforeNotCalled(Mock<IInterceptAsyncMethod<string>> interceptorsMock)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    It.IsAny<object>(),
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private static void BeforeCalled(Mock<IInterceptAsyncMethod<string>> interceptorsMock, object target, string methodName, string arg1)
        {
            interceptorsMock.Verify(
                t => t.Before(
                    target,
                    It.Is<MethodInfo>(m => m.Name == methodName),
                    new object[] { arg1 }),
                Times.Once);
        }

        private static void AfterNotCalled(Mock<IInterceptAsyncMethod<string>> interceptorsMock)
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

        private static void AfterCalledWith(Mock<IInterceptAsyncMethod<string>> interceptorsMock, string state, object? result, object target, string methodName, string arg1)
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

        private static void ErrorNotCalled(Mock<IInterceptAsyncMethod<string>> interceptorsMock)
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

        private static void ErrorCalled<TException>(Mock<IInterceptAsyncMethod<string>> interceptorsMock, string state, object target, string methodName, string arg1)
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
