using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace DotNetX.Logging.Tests
{
    public class LoggingInterceptorTests
    {
        #region [ Builder.WithLogger... ]

        [Fact]
        public void WithDefaultConfigurationBuildShouldFail()
        {
            // Given
            var builder = new LoggingInterceptorBuilder();

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        #region [ ILoggerFactory ]

        [Fact]
        public void WithAILoggerFactoryBuildShouldNotFail()
        {
            // Given
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(loggerFactory.Object);

            // When
            var interceptor = builder.Build();

            // Then
            interceptor.Should().NotBeNull();
        }

        [Fact]
        public void WithAILoggerFactoryAndLoggerCategoryNameBuildShouldNotFail()
        {
            // Given
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(loggerFactory.Object)
                .WithLoggerCategory<IDummyTarget>("DummyTarget");

            // When
            var interceptor = builder.Build();

            // Then
            interceptor.Should().NotBeNull();
        }

        [Fact]
        public void WithAILoggerFactoryCallingInterceptShouldNotFail()
        {
            // Given
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(loggerFactory.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);

            // When
            var intercepted = interceptor.Intercept(target.Object);

            // Then
            intercepted.Should().NotBeNull();
        }

        [Fact]
        public void WithAILoggerFactoryItShouldInterceptVoidMethods()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(e => e.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(loggerFactory.Object)
                .WithOptions(options);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            target.Verify(
                t => t.VoidMethod("value1"),
                Times.Once);

            loggerFactory.Verify(
                t => t.CreateLogger(typeof(IDummyTarget).FullName),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.VoidMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.VoidMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void WithAILoggerFactoryItShouldInterceptVoidMethodsThrowing()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(e => e.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(loggerFactory.Object)
                .WithOptions(options);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target
                .Setup(t => t.VoidMethod(It.IsAny<string>()))
                .Throws<FormatException>();

            var intercepted = interceptor.Intercept(target.Object);

            // When
            Action action = () => intercepted.VoidMethod("value1");

            // Then
            action.Should().Throw<FormatException>();

            target.Verify(
                t => t.VoidMethod("value1"),
                Times.Once);

            loggerFactory.Verify(
                t => t.CreateLogger(typeof(IDummyTarget).FullName),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.ErrorLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.VoidMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.ErrorLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.VoidMethod() | ERROR. Elapsed:")),
                    It.IsAny<FormatException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void WithAILoggerFactoryAndLoggerCategoryNameItShouldInterceptVoidMethods()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(e => e.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(loggerFactory.Object)
                .WithLoggerCategory<IDummyTarget, LoggingInterceptorTests>()
                .WithOptions(options);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            target.Verify(
                t => t.VoidMethod("value1"),
                Times.Once);

            loggerFactory.Verify(
                t => t.CreateLogger(typeof(LoggingInterceptorTests).FullName),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.VoidMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.VoidMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion [ ILoggerFactory ]

        #region [ ILogger ]

        [Fact]
        public void WithAILoggerBuildShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Strict);

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object);

            // When
            var interceptor = builder.Build();

            // Then
            interceptor.Should().NotBeNull();
        }

        [Fact]
        public void WithAILoggerAndILoggerFactoryBuildShouldFail()
        {
            // Given
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var logger = new Mock<ILogger>(MockBehavior.Loose);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(loggerFactory.Object)
                .WithLogger(logger.Object);

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void WithAILoggerAndStatefulLoggerFromMethodBuildShouldFail()
        {
            // Given
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var logger = new Mock<ILogger>(MockBehavior.Loose);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(method => logger.Object)
                .WithLogger(logger.Object);

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void WithAILoggerAndLoggerFromMethodBuildShouldFail()
        {
            // Given
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var logger = new Mock<ILogger>(MockBehavior.Loose);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory((factory, method) => logger.Object)
                .WithLogger(logger.Object);

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void WithAILoggerAndLoggerCategoryBuildShouldFail()
        {
            // Given
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var logger = new Mock<ILogger>(MockBehavior.Loose);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerCategory<IDummyTarget>("DummyTarget")
                .WithLogger(logger.Object);

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void WithAILoggerItShouldInterceptSyncMethods()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            intercepted.SyncMethod("value1");

            // Then
            target.Verify(
                t => t.SyncMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task WithAILoggerItShouldInterceptTaskMethodsAsync()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.TaskVoidMethod(It.IsAny<string>())).Returns(Task.CompletedTask);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            target.Verify(
                t => t.TaskVoidMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.TaskVoidMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.TaskVoidMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task WithAILoggerItShouldInterceptTaskOfTMethodsAsync()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.TaskMethod(It.IsAny<string>())).ReturnsAsync(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = await intercepted.TaskMethod("value1");

            // Then
            result.Should().Be(42);

            target.Verify(
                t => t.TaskMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.TaskMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.TaskMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task WithAILoggerItShouldInterceptValueTaskMethodsAsync()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.ValueTaskVoidMethod(It.IsAny<string>())).Returns(ValueTask.CompletedTask);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            await intercepted.ValueTaskVoidMethod("value1");

            // Then
            target.Verify(
                t => t.ValueTaskVoidMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.ValueTaskVoidMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.ValueTaskVoidMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task WithAILoggerItShouldInterceptValueTaskOfTMethodsAsync()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.ValueTaskMethod(It.IsAny<string>())).Returns(ValueTask.FromResult(42));

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = await intercepted.ValueTaskMethod("value1");

            // Then
            result.Should().Be(42);

            target.Verify(
                t => t.ValueTaskMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.ValueTaskMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.ValueTaskMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void WithAILoggerItShouldInterceptIEnumerableMethod()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .InterceptEnumerables();

            var interceptor = builder.Build();

            IEnumerable OneTwoThree()
            {
                yield return 1;
                yield return 2;
                yield return 3;
            }

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.EnumerableMethod(It.IsAny<string>())).Returns((string _) => OneTwoThree());

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.EnumerableMethod("value1");

            // Then
            result.Should().Equal(new[] { 1, 2, 3 });

            target.Verify(
                t => t.EnumerableMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.IsEnabled(options.NextLogLevel),
                Times.AtLeast(3));

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.EnumerableMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.NextLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.EnumerableMethod() | NEXT. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.EnumerableMethod() | COMPLETE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void WithAILoggerItShouldInterceptIEnumerableMethodThrowing()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .InterceptEnumerables();

            var interceptor = builder.Build();

            IEnumerable OneTwoThree()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                throw new FormatException();
                yield return 4;
            }

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.EnumerableMethod(It.IsAny<string>())).Returns((string _) => OneTwoThree());

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.EnumerableMethod("value1");
            Action action = () => result.Should().Equal(new[] { 1, 2, 3 });

            // Then
            action.Should().Throw<FormatException>();

            target.Verify(
                t => t.EnumerableMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.IsEnabled(options.NextLogLevel),
                Times.AtLeast(3));

            logger.Verify(
                t => t.IsEnabled(options.ErrorLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.EnumerableMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.NextLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.EnumerableMethod() | NEXT. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.ErrorLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.EnumerableMethod() | ERROR. Elapsed:")),
                    It.IsAny<FormatException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void WithAILoggerItShouldInterceptIEnumerableOfTMethod()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .InterceptEnumerables();

            var interceptor = builder.Build();

            IEnumerable<int> OneTwoThree()
            {
                yield return 1;
                yield return 2;
                yield return 3;
            }

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.EnumerableOfTMethod(It.IsAny<string>())).Returns((string _) => OneTwoThree());

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.EnumerableOfTMethod("value1");

            // Then
            result.Should().Equal(new[] { 1, 2, 3 });

            target.Verify(
                t => t.EnumerableOfTMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.IsEnabled(options.NextLogLevel),
                Times.AtLeast(3));

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.EnumerableOfTMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.NextLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.EnumerableOfTMethod() | NEXT. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.EnumerableOfTMethod() | COMPLETE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void WithAILoggerItShouldInterceptIEnumerableOfTMethodThrowing()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .InterceptEnumerables();

            var interceptor = builder.Build();

            IEnumerable<int> OneTwoThree()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                throw new FormatException();
                yield return 4;
            }

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.EnumerableOfTMethod(It.IsAny<string>())).Returns((string _) => OneTwoThree());

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.EnumerableOfTMethod("value1");
            Action action = () => result.Should().Equal(new[] { 1, 2, 3 });

            // Then
            action.Should().Throw<FormatException>();

            target.Verify(
                t => t.EnumerableOfTMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.IsEnabled(options.NextLogLevel),
                Times.AtLeast(3));

            logger.Verify(
                t => t.IsEnabled(options.ErrorLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.EnumerableOfTMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.NextLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.EnumerableOfTMethod() | NEXT. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.ErrorLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.EnumerableOfTMethod() | ERROR. Elapsed:")),
                    It.IsAny<FormatException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task WithAILoggerItShouldInterceptIAsyncEnumerableMethod()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .InterceptEnumerables();

            var interceptor = builder.Build();

            async IAsyncEnumerable<int> OneTwoThree()
            {
                await Task.CompletedTask;
                yield return 1;
                yield return 2;
                yield return 3;
            }

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.AsyncEnumerableMethod(It.IsAny<string>())).Returns((string _) => OneTwoThree());

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = await intercepted.AsyncEnumerableMethod("value1").ToListAsync();

            // Then
            result.Should().Equal(new[] { 1, 2, 3 });

            target.Verify(
                t => t.AsyncEnumerableMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.IsEnabled(options.NextLogLevel),
                Times.AtLeast(3));

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.AsyncEnumerableMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.NextLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.AsyncEnumerableMethod() | NEXT. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.AsyncEnumerableMethod() | COMPLETE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void WithAILoggerItShouldInterceptIAsyncEnumerableMethodThrowing()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .InterceptEnumerables();

            var interceptor = builder.Build();

            async IAsyncEnumerable<int> OneTwoThree()
            {
                await Task.CompletedTask;
                yield return 1;
                yield return 2;
                yield return 3;
                throw new FormatException();
                yield return 4;
            }

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.AsyncEnumerableMethod(It.IsAny<string>())).Returns((string _) => OneTwoThree());

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.AsyncEnumerableMethod("value1");
            Func<Task> action = async () => (await result.ToListAsync()).Should().Equal(new[] { 1, 2, 3 });

            // Then
            action.Should().ThrowAsync<FormatException>();

            target.Verify(
                t => t.AsyncEnumerableMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.IsEnabled(options.NextLogLevel),
                Times.AtLeast(3));

            logger.Verify(
                t => t.IsEnabled(options.ErrorLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.AsyncEnumerableMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.NextLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.AsyncEnumerableMethod() | NEXT. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.ErrorLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.AsyncEnumerableMethod() | ERROR. Elapsed:")),
                    It.IsAny<FormatException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task WithAILoggerItShouldInterceptIObservableMethod()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .InterceptEnumerables();

            var interceptor = builder.Build();

            IObservable<int> OneTwoThree()
            {
                return Observable.Range(1, 3);
            }

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.ObservableMethod(It.IsAny<string>())).Returns((string _) => OneTwoThree());

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = await intercepted.ObservableMethod("value1").ToArray();

            // Then
            result.Should().Equal(new[] { 1, 2, 3 });

            target.Verify(
                t => t.ObservableMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.IsEnabled(options.NextLogLevel),
                Times.AtLeast(3));

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.ObservableMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.NextLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.ObservableMethod() | NEXT. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.ObservableMethod() | COMPLETE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void WithAILoggerItShouldInterceptIObservableMethodThrowing()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .InterceptEnumerables();

            var interceptor = builder.Build();

            IObservable<int> OneTwoThree()
            {
                return Observable
                    .Range(1, 3)
                    .Concat(Observable.Throw<int>(new FormatException()))
                    .Concat(Observable.Range(4, 2));
            }

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.ObservableMethod(It.IsAny<string>())).Returns((string _) => OneTwoThree());

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.ObservableMethod("value1");
            Func<Task> action = async () => (await result.ToArray()).Should().Equal(new[] { 1, 2, 3 });

            // Then
            action.Should().ThrowAsync<FormatException>();

            target.Verify(
                t => t.ObservableMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.AtLeastOnce);

            logger.Verify(
                t => t.IsEnabled(options.NextLogLevel),
                Times.AtLeast(3));

            logger.Verify(
                t => t.IsEnabled(options.ErrorLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(5));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.ObservableMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.NextLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.ObservableMethod() | NEXT. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.ErrorLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.ObservableMethod() | ERROR. Elapsed:")),
                    It.IsAny<FormatException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion [ ILogger ]

        #region [ StatefulLoggerFromMethod ]

        [Fact]
        public void WithAStatefulLoggerFromMethodBuildShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory((method) => logger.Object);

            // When
            var interceptor = builder.Build();

            // Then
            interceptor.Should().NotBeNull();
        }

        [Fact]
        public void WithAStatefulLoggerFromMethodAndILoggerFactoryBuildShouldFail()
        {
            // Given
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var logger = new Mock<ILogger>(MockBehavior.Loose);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(method => logger.Object)
                .WithLoggerFactory(loggerFactory.Object);

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void WithAStatefulLoggerFromMethodAndLoggerFromMethodBuildShouldFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(method => logger.Object)
                .WithLoggerFactory((factory, method) => logger.Object);

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void WithAStatefulLoggerFromMethodAndLoggerCategoryBuildShouldFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory(method => logger.Object)
                .WithLoggerCategory(typeof(IDummyTarget), "DummyTarget");

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        #endregion [ StatefulLoggerFromMethod ]

        #region [ LoggerFromMethod ]

        [Fact]
        public void WithALoggerFromMethodAndILoggerFactoryBuildShouldNotFail()
        {
            // Given
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory((factory, method) => default(ILogger)!)
                .WithLoggerFactory(loggerFactory.Object);

            // When
            var interceptor = builder.Build();

            // Then
            interceptor.Should().NotBeNull();
        }

        [Fact]
        public void WithALoggerFromMethodWithoutILoggerFactoryBuildShouldFail()
        {
            // Given
            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory((factory, method) => default(ILogger)!);

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void WithALoggerFromMethodAndLoggerCategoryBuildShouldFail()
        {
            // Given
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerFactory((factory, method) => default(ILogger)!)
                .WithLoggerFactory(loggerFactory.Object)
                .WithLoggerCategory(typeof(IDummyTarget), "DummyTarget");

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        #endregion [ StatefulLoggerFromMethod ]

        #endregion [ Builder.WithLogger... ]

        #region [ DoNotInterceptIf... ]

        [Fact]
        public void DoNotInterceptIfShouldPreventAGivenMethodFromBeingIntercepted()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .DoNotInterceptIf((method) => method.Name == nameof(IDummyTarget.VoidMethod));
            
            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            intercepted.VoidMethod("value1");
            var result = intercepted.SyncMethod("value2");

            // Then
            result.Should().Be(42);

            target.Verify(
                t => t.VoidMethod("value1"),
                Times.Once);

            target.Verify(
                t => t.SyncMethod("value2"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void DoNotInterceptIfNamedShouldPreventAGivenMethodFromBeingIntercepted()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .DoNotInterceptIfNamed(nameof(IDummyTarget.VoidMethod));

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            intercepted.VoidMethod("value1");
            var result = intercepted.SyncMethod("value2");

            // Then
            result.Should().Be(42);

            target.Verify(
                t => t.VoidMethod("value1"),
                Times.Once);

            target.Verify(
                t => t.SyncMethod("value2"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void DoNotInterceptIfRegexShouldPreventAGivenMethodFromBeingIntercepted()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .DoNotInterceptIfMatches(new Regex("^Void"));

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            intercepted.VoidMethod("value1");
            var result = intercepted.SyncMethod("value2");

            // Then
            result.Should().Be(42);

            target.Verify(
                t => t.VoidMethod("value1"),
                Times.Once);

            target.Verify(
                t => t.SyncMethod("value2"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void DoNotInterceptIfPatternShouldPreventAGivenMethodFromBeingIntercepted()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .DoNotInterceptIfMatches("^Void");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            intercepted.VoidMethod("value1");
            var result = intercepted.SyncMethod("value2");

            // Then
            result.Should().Be(42);

            target.Verify(
                t => t.VoidMethod("value1"),
                Times.Once);

            target.Verify(
                t => t.SyncMethod("value2"),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(It.IsAny<LogLevel>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.IsEnabled(options.StartLogLevel),
                Times.Once);

            logger.Verify(
                t => t.IsEnabled(options.DoneLogLevel),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod() | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion [ DoNotInterceptIf... ]

        #region [ WithResult / LogResult ]

        [Fact]
        public void WithResultShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .WithResult((method, result) => new { Method = method.Name, Result = result });

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ({ Method = SyncMethod, Result = 42 }). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void WithResultPlusLogResultShouldFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithResult((method, result) => new { Method = method.Name, Result = result })
                .LogResult(nameof(IDummyTarget.SyncMethod), "Value");

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void LogResultRawShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var predicate = new Mock<Func<MethodInfo, Type, bool>>();
            predicate
                .Setup(e => e.Invoke(It.IsAny<MethodInfo>(), It.IsAny<Type>()))
                .Returns((MethodInfo method, Type type) => method.Name == nameof(IDummyTarget.SyncMethod));

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? result) => new[] { KeyValuePair.Create("Value", result) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult(predicate.Object, extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            predicate.Verify(
                t => t.Invoke(
                    It.Is<MethodInfo>(m => m.Name == "SyncMethod"),
                    typeof(int)));

            extract.Verify(t => t.Invoke(42));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultMethodNameAndTypeShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? result) => new { Value = result });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult("SyncMethod", typeof(int), extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke(42));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultTMethodNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? result) => new[] { ("Value", result) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult<int>("SyncMethod", extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke(42));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultMethodNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? result) => new[] { KeyValuePair.Create("Value", result) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult("SyncMethod", extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke(42));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultTypeShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? result) => new[] { KeyValuePair.Create("Value", result) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult(typeof(int), extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke(42));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultTShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? result) => new[] { KeyValuePair.Create("Value", result) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult<int>(extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            target.Verify(
                t => t.SyncMethod("value1"),
                Times.Once);

            extract.Verify(t => t.Invoke(42));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultMethodNameAndTypeWithOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult("SyncMethod", typeof(int), "Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultTMethodNameWithOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult<int>("SyncMethod", "Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultMethodNameWithOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult("SyncMethod", "Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultTypeWithOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult(typeof(int), "Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultTWithOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult<int>("Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogResultOnTaskOfTShouldNotFailAsync()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .InterceptAsync()
                .WithOptions(options)
                .LogResult<int>("Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.TaskMethod(It.IsAny<string>())).ReturnsAsync(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = await intercepted.TaskMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.TaskMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogResultOnValueTaskOfTShouldNotFailAsync()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .InterceptAsync()
                .WithOptions(options)
                .LogResult<int>("Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.ValueTaskMethod(It.IsAny<string>())).ReturnsAsync(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = await intercepted.ValueTaskMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.ValueTaskMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogResultOnTaskShouldNotFailAsync()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .InterceptAsync()
                .WithOptions(options)
                .LogResult("TaskVoidMethod", "Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.TaskVoidMethod(It.IsAny<string>()));

            var intercepted = interceptor.Intercept(target.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.TaskVoidMethod() | RESULT = ([Value, ]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogResultOnIEnumerableOfTShouldNotFailAsync()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .InterceptEnumerables()
                .WithOptions(options)
                .LogResult<int>("Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.EnumerableOfTMethod(It.IsAny<string>())).Returns(Enumerable.Range(1, 3));

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.EnumerableOfTMethod("value1");

            // Then
            result.Should().Equal(new[] { 1, 2, 3 });

            for (int value = 1; value <= 3; value++)
            {
                logger.Verify(
                    t => t.Log<It.IsAnyType>(
                        options.NextLogLevel,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith($"IDummyTarget.EnumerableOfTMethod() | NEXT = ([Value, {value}]). Elapsed:")),
                        default(Exception),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.EnumerableOfTMethod() | COMPLETE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogResultOnIAsyncEnumerableShouldNotFailAsync()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .InterceptEnumerables()
                .WithOptions(options)
                .LogResult<int>("Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.AsyncEnumerableMethod(It.IsAny<string>())).Returns(AsyncEnumerable.Range(1, 3));

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = await intercepted.AsyncEnumerableMethod("value1").ToArrayAsync();

            // Then
            result.Should().Equal(new[] { 1, 2, 3 });

            for (int value = 1; value <= 3; value++)
            {
                logger.Verify(
                    t => t.Log<It.IsAnyType>(
                        options.NextLogLevel,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith($"IDummyTarget.AsyncEnumerableMethod() | NEXT = ([Value, {value}]). Elapsed:")),
                        default(Exception),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.AsyncEnumerableMethod() | COMPLETE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogResultOnIObservableShouldNotFailAsync()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .InterceptEnumerables()
                .WithOptions(options)
                .LogResult<int>("Value");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.ObservableMethod(It.IsAny<string>())).Returns(Observable.Range(1, 3));

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = await intercepted.ObservableMethod("value1").ToArray();

            // Then
            result.Should().Equal(new[] { 1, 2, 3 });

            for (int value = 1; value <= 3; value++)
            {
                logger.Verify(
                    t => t.Log<It.IsAnyType>(
                        options.NextLogLevel,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith($"IDummyTarget.ObservableMethod() | NEXT = ([Value, {value}]). Elapsed:")),
                        default(Exception),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.ObservableMethod() | COMPLETE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CombinedLogResultWithDifferentKeysShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult<int>(result => new[] { KeyValuePair.Create("Value", result) })
                .LogResult("SyncMethod", result => new[] { KeyValuePair.Create("Index", result) });

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            target.Verify(
                t => t.SyncMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42], [Index, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CombinedLogResultWithDuplicateKeysShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogResult<int>(result => new[] { KeyValuePair.Create("Value", result) })
                .LogResult("SyncMethod", result => new[] { KeyValuePair.Create("Value", result) });

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            target.Verify(
                t => t.SyncMethod("value1"),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod() | RESULT = ([Value, 42]). Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion [ WithResult ]

        #region [ WithParameters / LogParameter ]

        [Fact]
        public void WithParametersShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .WithParameters((method, args) => new { Method = method.Name, Arg1 = args![0] });

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod({ Method = SyncMethod, Arg1 = value1 }) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod({ Method = SyncMethod, Arg1 = value1 }) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void WithParametersPlusLogParametersShouldFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithParameters((method, args) => new { Method = method.Name, Arg1 = args![0] })
                .LogParameter(nameof(IDummyTarget.SyncMethod), "arg1");

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void LogParameterRawShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var predicate = new Mock<Func<MethodInfo, Type, string, bool>>();
            predicate
                .Setup(e => e.Invoke(It.IsAny<MethodInfo>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Returns((MethodInfo method, Type type, string name) => name == "arg1");

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? value) => new[] { KeyValuePair.Create("Arg1", value) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter(predicate.Object, extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            predicate.Verify(
                t => t.Invoke(
                    It.Is<MethodInfo>(m => m.Name == "SyncMethod"),
                    typeof(string),
                    "arg1"));

            extract.Verify(t => t.Invoke("value1"));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterWithMethodTypeAndNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? value) => new { Arg1 = value });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter("SyncMethod", typeof(string), "arg1", extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke("value1"));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterTWithMethodAndNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? value) => new[] { ("Arg1", value) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter<string>("SyncMethod", "arg1", extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke("value1"));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterWithMethodAndNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? value) => new[] { KeyValuePair.Create("Arg1", value) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter("SyncMethod", "arg1", extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke("value1"));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterWithTypeAndNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? value) => new[] { KeyValuePair.Create("Arg1", value) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter(typeof(string), "arg1", extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke("value1"));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterTWithNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? value) => new[] { KeyValuePair.Create("Arg1", value) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter<string>("arg1", extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke("value1"));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterTShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? value) => new[] { KeyValuePair.Create("Arg1", value) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter(typeof(string), extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke("value1"));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterWithTypeShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? value) => new[] { KeyValuePair.Create("Arg1", value) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter<string>(extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke("value1"));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterWithNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var extract = new Mock<Func<object?, object?>>();
            extract
                .Setup(e => e.Invoke(It.IsAny<object>()))
                .Returns((object? value) => new[] { KeyValuePair.Create("Arg1", value) });

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter("arg1", extract.Object);

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            extract.Verify(t => t.Invoke("value1"));

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterWithMethodTypeAndNameToOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter("SyncMethod", typeof(string), "arg1", "Arg1");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterTWithMethodAndNameToOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter<string>("SyncMethod", "arg1", "Arg1");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterWithMethodAndNameToOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter("SyncMethod", "arg1", "Arg1");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterWithTypeAndNameToOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter(typeof(string), "arg1", "Arg1");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterTWithNameToOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter<string>("arg1", "Arg1");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogParameterWithNameToOutputNameShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);

            var options = new LoggingInterceptorOptions();

            var builder = new LoggingInterceptorBuilder()
                .WithLogger(logger.Object)
                .WithOptions(options)
                .LogParameter("arg1", "Arg1");

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.StartLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString() == "IDummyTarget.SyncMethod([Arg1, value1]) | START"),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            logger.Verify(
                t => t.Log<It.IsAnyType>(
                    options.DoneLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, _) => message.ToString().StartsWith("IDummyTarget.SyncMethod([Arg1, value1]) | DONE. Elapsed:")),
                    default(Exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion [ WithParameters / LogParameter ]

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

    }
}
