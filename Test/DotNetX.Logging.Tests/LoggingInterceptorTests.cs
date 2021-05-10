using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
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
            var logger = new Mock<ILogger>(MockBehavior.Strict);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);
            logger
                .Setup(e => e.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));

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
            var logger = new Mock<ILogger>(MockBehavior.Strict);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);
            logger
                .Setup(e => e.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));

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
            var logger = new Mock<ILogger>(MockBehavior.Strict);
            logger
                .Setup(e => e.IsEnabled(
                    It.IsAny<LogLevel>()))
                .Returns(true);
            logger
                .Setup(e => e.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));

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
            var logger = new Mock<ILogger>(MockBehavior.Strict);

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
            var logger = new Mock<ILogger>(MockBehavior.Strict);

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
            var logger = new Mock<ILogger>(MockBehavior.Strict);

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
            var logger = new Mock<ILogger>(MockBehavior.Strict);

            var builder = new LoggingInterceptorBuilder()
                .WithLoggerCategory<IDummyTarget>("DummyTarget")
                .WithLogger(logger.Object);

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        #endregion [ ILogger ]

        #region [ StatefulLoggerFromMethod ]

        [Fact]
        public void WithAStatefulLoggerFromMethodBuildShouldNotFail()
        {
            // Given
            var logger = new Mock<ILogger>(MockBehavior.Strict);

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
            var logger = new Mock<ILogger>(MockBehavior.Strict);

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
            var logger = new Mock<ILogger>(MockBehavior.Strict);

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
            var logger = new Mock<ILogger>(MockBehavior.Strict);

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

        #region [ WithResult ]
        #endregion [ WithResult ]

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
