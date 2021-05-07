using DotNetX.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetX.Tests
{
    [TestFixture]
    public partial class InterceptorTests
    {
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

        public interface IDummyTarget
        {
            void VoidMethod(string arg1);

            int SyncMethod(string arg1);

            Task TaskVoidMethod(string arg1);

            Task<int> TaskMethod(string arg1);

            ValueTask ValueTaskVoidMethod(string arg1);

            ValueTask<int> ValueTaskMethod(string arg1);

            IEnumerable EnumerableMethod(string arg1);

            IEnumerable<int> EnumerableOfTMethod(string arg1);

            IAsyncEnumerable<int> AsyncEnumerableMethod(string arg1);

            IObservable<int> ObservableMethod(string arg1);
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

    //static class ObservableExtensions
    //{
    //    public static Task<List<T>> ToListAsync<T>(
    //        this IObservable<T> source,
    //        CancellationToken cancellationToken = default)
    //    {
    //        var taskSource = new TaskCompletionSource<List<T>>();

    //        var list = new List<T>();

    //        var subscription = source.Subscribe(
    //            new DelegateObserver<T>(
    //                completed: () => taskSource.TrySetResult(list),
    //                error: exception => taskSource.TrySetException(exception),
    //                next: value =>
    //                {
    //                    lock (list)
    //                    {
    //                        list.Add(value);
    //                    }
    //                }));

    //        cancellationToken.Register(() =>
    //        {
    //            taskSource.TrySetCanceled(cancellationToken);
    //            subscription.Dispose();
    //        });

    //        return taskSource.Task;
    //    }
    //}

    //class DelegateObserver<T> : IObserver<T>
    //{
    //    private readonly Action completed;
    //    private readonly Action<Exception> error;
    //    private readonly Action<T> next;

    //    public DelegateObserver(
    //        Action completed,
    //        Action<Exception> error,
    //        Action<T> next)
    //    {
    //        this.completed = completed;
    //        this.error = error;
    //        this.next = next;
    //    }

    //    public void OnCompleted()
    //    {
    //        completed();
    //    }

    //    public void OnError(Exception exception)
    //    {
    //        error(exception);
    //    }

    //    public void OnNext(T value)
    //    {
    //        next(value);
    //    }
    //}

}
