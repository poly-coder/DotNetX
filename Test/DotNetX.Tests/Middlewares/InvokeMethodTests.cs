using DotNetX.Middlewares;
using FluentAssertions;
using FsCheck;
using FsCheck.NUnit;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace DotNetX.Tests.Middlewares
{
    using InvokeMethodSyncMiddlewareFunc = VoidSyncMiddlewareFunc<InvokeMethodContext>;
    using InvokeMethodSyncMiddleware = VoidSyncMiddleware<InvokeMethodContext>;
    using InvokeMethodAsyncMiddlewareFunc = TaskMiddlewareFunc<InvokeMethodContext>;
    using InvokeMethodAsyncMiddleware = TaskMiddleware<InvokeMethodContext>;

    [TestFixture]
    public class InvokeMethodTests
    {
        [Test]
        public void WhenInvokeMethodContextIsCreatedItShouldNotHaveResult()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            // Then 
            context.Method.Should().BeSameAs(method);
            context.Parameters.Should().BeSameAs(parameters);
            context.Instance.Should().BeSameAs(this);
            context.HasResult.Should().BeFalse();
            context.Result.Should().BeNull();
            context.HasException.Should().BeFalse();
            context.Exception.Should().BeNull();
        }

        [Test]
        public void WhenInvokeMethodContextHasResultSetItShouldHaveResult()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            var result = new object();

            // When
            context.SetResult(result);

            // Then 
            context.HasResult.Should().BeTrue();
            context.Result.Should().BeSameAs(result);
            context.HasException.Should().BeFalse();
            context.Exception.Should().BeNull();
        }
        
        [Test]
        public void WhenInvokeMethodContextHasExceptionSetItShouldHaveException()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            var exception = new Exception();

            // When
            context.SetException(exception);

            // Then 
            context.HasResult.Should().BeFalse();
            context.Result.Should().BeNull();
            context.HasException.Should().BeTrue();
            context.Exception.Should().BeSameAs(exception);
        }

        [Test]
        public void WhenInvokeMethodContextHasExceptionSetAndThenResultItShouldHaveResultAndNoException()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            var exception = new Exception();
            var result = new object();

            // When
            context.SetException(exception);
            context.SetResult(result);

            // Then 
            context.HasResult.Should().BeTrue();
            context.Result.Should().BeSameAs(result);
            context.HasException.Should().BeFalse();
            context.Exception.Should().BeNull();
        }
        
        [Test]
        public void WhenInvokeMethodContextHasResultSetAndThenExceptionItShouldHaveExceptionAndNoResult()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            var exception = new Exception();
            var result = new object();

            // When
            context.SetResult(result);
            context.SetException(exception);

            // Then 
            context.HasResult.Should().BeFalse();
            context.Result.Should().BeNull();
            context.HasException.Should().BeTrue();
            context.Exception.Should().BeSameAs(exception);
        }

        [Test]
        public void WhenInvokeMethodContextHasResultReturnShouldNotFail()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            var result = new object();

            // When
            context.SetResult(result);

            // Then 
            context.Return();
        }

        [Test]
        public void WhenInvokeMethodContextHasExceptionReturnShouldFail()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            var exception = new FormatException();

            // When
            context.SetException(exception);

            // Then
            Action action = () => context.Return();
            action.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<FormatException>();
        }

        [Test]
        public void WhenInvokeMethodContextIsNotSetReturnShouldFail()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            // Then
            Action action = () => context.Return();
            action.Should()
                .Throw<InvokeMethodWithoutResultException>();
        }

        [Test]
        public void WhenInvokeMethodContextHasResultTypedReturnShouldNotFail()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            var result = 42;

            // When
            context.SetResult(result);

            // Then 
            context.Return<int>().Should().Be(42);
        }

        [Test]
        public void WhenInvokeMethodContextHasExceptionTypedReturnShouldFail()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            var exception = new FormatException();

            // When
            context.SetException(exception);

            // Then
            Action action = () => context.Return<int>();
            action.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<FormatException>();
        }

        [Test]
        public void WhenInvokeMethodContextIsNotSetTypedReturnShouldFail()
        {
            // Given
            var method = typeof(InvokeMethodTests).GetMethod(nameof(WhenInvokeMethodContextIsCreatedItShouldNotHaveResult))!;

            var parameters = new object[0];

            var context = new InvokeMethodContext(
                method,
                parameters,
                this);

            // Then
            Action action = () => context.Return<int>();
            action.Should()
                .Throw<InvokeMethodWithoutResultException>();
        }

        public interface ISampleService
        {
            void CallVoid(string p1);
            Task CallTask(string p1, CancellationToken cancellationToken);
            int GetSync(string p1);
            Task<int> GetAsync(string p1, CancellationToken cancellationToken);
        }

        record SampleServiceWrapperSyncMiddleware(InvokeMethodSyncMiddleware Middleware);
        record SampleServiceWrapperAsyncMiddleware(InvokeMethodAsyncMiddleware Middleware);

        class SampleServiceWrapper : ISampleService
        {
            public SampleServiceWrapper(
                ISampleService innerService,
                SampleServiceWrapperSyncMiddleware syncMiddleware,
                SampleServiceWrapperAsyncMiddleware asyncMiddleware)
            {
                this.innerService = innerService;
                this.syncMiddleware = syncMiddleware.Middleware;
                this.asyncMiddleware = asyncMiddleware.Middleware;

                this.callSync = this.syncMiddleware.Combine(InvokeMethodMiddleware.CallSyncFunc);
                this.callAsync = this.asyncMiddleware.Combine(InvokeMethodMiddleware.CallAsyncFunc);
            }

            public Task CallTask(string p1, CancellationToken cancellationToken) =>
                callAsync.CallInvokeAsync(
                    innerService, 
                    CallTaskMethod, 
                    cancellationToken, 
                    p1, cancellationToken);

            public void CallVoid(string p1) => 
                callSync.CallInvoke(
                    innerService, 
                    CallVoidMethod, 
                    p1);

            public Task<int> GetAsync(string p1, CancellationToken cancellationToken) =>
                callAsync.CallInvokeAsync<int>(
                    innerService,
                    GetAsyncMethod,
                    cancellationToken,
                    p1, cancellationToken);

            public int GetSync(string p1) => 
                callSync.CallInvoke<int>(
                    innerService, 
                    GetSyncMethod, 
                    p1);

            #region [ Internal ]

            private readonly ISampleService innerService;
            private readonly InvokeMethodSyncMiddleware syncMiddleware;
            private readonly InvokeMethodAsyncMiddleware asyncMiddleware;

            private readonly InvokeMethodSyncMiddlewareFunc callSync;
            private readonly InvokeMethodAsyncMiddlewareFunc callAsync;

            private static readonly MethodInfo CallTaskMethod =
                typeof(ISampleService).GetMethod(
                    name: nameof(ISampleService.CallTask),
                    bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                    binder: default(Binder),
                    types: new Type[] { typeof(string), typeof(CancellationToken) },
                    modifiers: default)!;

            private static readonly MethodInfo CallVoidMethod =
                typeof(ISampleService).GetMethod(
                    name: nameof(ISampleService.CallVoid),
                    bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                    binder: default(Binder),
                    types: new Type[] { typeof(string) },
                    modifiers: default)!;

            private static readonly MethodInfo GetAsyncMethod =
                typeof(ISampleService).GetMethod(
                    name: nameof(ISampleService.GetAsync),
                    bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                    binder: default(Binder),
                    types: new Type[] { typeof(string), typeof(CancellationToken) },
                    modifiers: default)!;

            private static readonly MethodInfo GetSyncMethod =
                typeof(ISampleService).GetMethod(
                    name: nameof(ISampleService.GetSync),
                    bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                    binder: default(Binder),
                    types: new Type[] { typeof(string) },
                    modifiers: default)!;

            #endregion [ Internal ]
        }

        private (Mock<ISampleService>, Mock<InvokeMethodSyncMiddleware>, Mock<InvokeMethodAsyncMiddleware>) CreateMocks()
        {
            var mock = new Mock<ISampleService>(MockBehavior.Strict);

            var syncMiddleware = new Mock<InvokeMethodSyncMiddleware>();
            syncMiddleware
                .Setup(m => m(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodSyncMiddlewareFunc>()))
                .Callback((InvokeMethodContext ctx, InvokeMethodSyncMiddlewareFunc next) => next(ctx));

            var asyncMiddleware = new Mock<InvokeMethodAsyncMiddleware>();
            asyncMiddleware
                .Setup(m => m(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodAsyncMiddlewareFunc>(),
                    It.IsAny<CancellationToken>()))
                .Returns((InvokeMethodContext ctx, InvokeMethodAsyncMiddlewareFunc next, CancellationToken ct) => next(ctx, ct));

            return (mock, syncMiddleware, asyncMiddleware);
        }

        [Test]
        public void WhenSampleServiceWrapperIsCreatedItShouldNotFail()
        {
            // Given
            var (inner, syncMiddleware, asyncMiddleware) = CreateMocks();

            // Then
            var wrapper = new SampleServiceWrapper(
                inner.Object,
                new SampleServiceWrapperSyncMiddleware(syncMiddleware.Object),
                new SampleServiceWrapperAsyncMiddleware(asyncMiddleware.Object));
        }

        [Test]
        public void WhenCallVoidIsUsedItShouldCallInner()
        {
            // Given
            var (inner, syncMiddleware, asyncMiddleware) = CreateMocks();

            inner.Setup(e => e.CallVoid(It.IsAny<string>()));

            ISampleService wrapper = new SampleServiceWrapper(
                inner.Object,
                new SampleServiceWrapperSyncMiddleware(syncMiddleware.Object),
                new SampleServiceWrapperAsyncMiddleware(asyncMiddleware.Object));

            // When
            wrapper.CallVoid("input");

            // Then
            inner.Verify(e => e.CallVoid("input"), Times.Once);

            syncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodSyncMiddlewareFunc>()),
                    Times.Once);

            asyncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodAsyncMiddlewareFunc>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Test]
        public void WhenCallVoidIsUsedWithExceptionItShouldCallInner()
        {
            // Given
            var (inner, syncMiddleware, asyncMiddleware) = CreateMocks();

            inner.Setup(e => e.CallVoid(It.IsAny<string>())).Throws<FormatException>();

            ISampleService wrapper = new SampleServiceWrapper(
                inner.Object,
                new SampleServiceWrapperSyncMiddleware(syncMiddleware.Object),
                new SampleServiceWrapperAsyncMiddleware(asyncMiddleware.Object));

            // When
            Action action = () => wrapper.CallVoid("input");

            // Then
            action.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<FormatException>();

            syncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodSyncMiddlewareFunc>()),
                    Times.Once);

            asyncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodAsyncMiddlewareFunc>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Test]
        public void WhenGetSyncIsUsedItShouldCallInner()
        {
            // Given
            var (inner, syncMiddleware, asyncMiddleware) = CreateMocks();

            inner.Setup(e => e.GetSync(It.IsAny<string>())).Returns(42);

            ISampleService wrapper = new SampleServiceWrapper(
                inner.Object,
                new SampleServiceWrapperSyncMiddleware(syncMiddleware.Object),
                new SampleServiceWrapperAsyncMiddleware(asyncMiddleware.Object));

            // When
            var result = wrapper.GetSync("input");

            // Then
            inner.Verify(e => e.GetSync("input"), Times.Once);

            syncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodSyncMiddlewareFunc>()),
                    Times.Once);

            asyncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodAsyncMiddlewareFunc>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Test]
        public void WhenGetSyncIsUsedWithExceptionItShouldCallInner()
        {
            // Given
            var (inner, syncMiddleware, asyncMiddleware) = CreateMocks();

            inner.Setup(e => e.GetSync(It.IsAny<string>())).Throws<FormatException>();

            ISampleService wrapper = new SampleServiceWrapper(
                inner.Object,
                new SampleServiceWrapperSyncMiddleware(syncMiddleware.Object),
                new SampleServiceWrapperAsyncMiddleware(asyncMiddleware.Object));

            // When
            Action action = () => wrapper.GetSync("input");

            // Then
            action.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<FormatException>();

            syncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodSyncMiddlewareFunc>()),
                    Times.Once);

            asyncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodAsyncMiddlewareFunc>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Test]
        public async Task WhenCallTaskIsUsedItShouldCallInner()
        {
            // Given
            var (inner, syncMiddleware, asyncMiddleware) = CreateMocks();

            inner
                .Setup(e => e.CallTask(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            ISampleService wrapper = new SampleServiceWrapper(
                inner.Object,
                new SampleServiceWrapperSyncMiddleware(syncMiddleware.Object),
                new SampleServiceWrapperAsyncMiddleware(asyncMiddleware.Object));

            using var cancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = cancellationTokenSource.Token;

            // When
            await wrapper.CallTask("input", cancellationToken);

            // Then
            inner.Verify(e => e.CallTask("input", cancellationToken), Times.Once);

            syncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodSyncMiddlewareFunc>()),
                    Times.Never);

            asyncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodAsyncMiddlewareFunc>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        public async Task WhenCallTaskIsUsedWithExceptionItShouldCallInner()
        {
            // Given
            var (inner, syncMiddleware, asyncMiddleware) = CreateMocks();

            inner
                .Setup(e => e.CallTask(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async (string input, CancellationToken ct) =>
                {
                    await Task.CompletedTask;
                    throw new FormatException();
                });

            ISampleService wrapper = new SampleServiceWrapper(
                inner.Object,
                new SampleServiceWrapperSyncMiddleware(syncMiddleware.Object),
                new SampleServiceWrapperAsyncMiddleware(asyncMiddleware.Object));

            using var cancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = cancellationTokenSource.Token;

            // When
            Func<Task> action = () => wrapper.CallTask("input", cancellationToken);

            // Then
            (await action.Should()
                .ThrowAsync<TargetInvocationException>())
                .WithInnerException<FormatException>();

            syncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodSyncMiddlewareFunc>()),
                    Times.Never);

            asyncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodAsyncMiddlewareFunc>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        public async Task WhenGetAsyncIsUsedItShouldCallInner()
        {
            // Given
            var (inner, syncMiddleware, asyncMiddleware) = CreateMocks();

            inner
                .Setup(e => e.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(42));

            ISampleService wrapper = new SampleServiceWrapper(
                inner.Object,
                new SampleServiceWrapperSyncMiddleware(syncMiddleware.Object),
                new SampleServiceWrapperAsyncMiddleware(asyncMiddleware.Object));

            using var cancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = cancellationTokenSource.Token;

            // When
            var result = await wrapper.GetAsync("input", cancellationToken);

            // Then
            result.Should().Be(42);

            inner.Verify(e => e.GetAsync("input", cancellationToken), Times.Once);

            syncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodSyncMiddlewareFunc>()),
                    Times.Never);

            asyncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodAsyncMiddlewareFunc>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        public async Task WhenGetAsyncIsUsedWithExceptionItShouldCallInner()
        {
            // Given
            var (inner, syncMiddleware, asyncMiddleware) = CreateMocks();

            inner
                .Setup(e => e.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async (string input, CancellationToken ct) =>
                {
                    await Task.CompletedTask;
                    throw new FormatException();
                });

            ISampleService wrapper = new SampleServiceWrapper(
                inner.Object,
                new SampleServiceWrapperSyncMiddleware(syncMiddleware.Object),
                new SampleServiceWrapperAsyncMiddleware(asyncMiddleware.Object));

            using var cancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = cancellationTokenSource.Token;

            // When
            Func<Task<int>> action = () => wrapper.GetAsync("input", cancellationToken);

            // Then
            (await action.Should()
                .ThrowAsync<TargetInvocationException>())
                .WithInnerException<FormatException>();

            syncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodSyncMiddlewareFunc>()),
                    Times.Never);

            asyncMiddleware.Verify(e => e(
                    It.IsAny<InvokeMethodContext>(),
                    It.IsAny<InvokeMethodAsyncMiddlewareFunc>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
        }
    }
}
