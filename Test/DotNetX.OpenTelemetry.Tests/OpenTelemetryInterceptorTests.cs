using FluentAssertions;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace DotNetX.OpenTelemetry.Tests
{
    public class OpenTelemetryInterceptorTests
    {
        #region [ Builder.WithActivitySource... ]

        [Fact]
        public void WithDefaultConfigurationBuildShouldFail()
        {
            // Given
            var builder = new OpenTelemetryInterceptorBuilder();

            // When
            Action action = () => builder.Build();

            // Then
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void WithActivitySourceBuildShouldNotFail()
        {
            // Given
            var activitySource = new Mock<IActivitySource>(MockBehavior.Loose);

            var builder = new OpenTelemetryInterceptorBuilder()
                .WithActivitySource(activitySource.Object);

            // When
            var interceptor = builder.Build();

            // Then
            interceptor.Should().NotBeNull();
        }

        #endregion [ Builder.WithActivitySource... ]

        #region [ Calling methods ]
        
        [Fact]
        public void CallVoidMethod()
        {
            // Given
            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.Database).Returns("mydb");
            target.Setup(e => e.Container).Returns("mycontainer");

            var activity = new Mock<IActivity>(MockBehavior.Loose);
            
            var activitySource = new Mock<IActivitySource>(MockBehavior.Loose);
            activitySource
                .Setup(e => e.StartActivity(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<ActivityContext>(),
                    It.IsAny<ActivityTagsCollection>(),
                    It.IsAny<IEnumerable<ActivityLink>>(),
                    It.IsAny<DateTimeOffset>()))
                .Returns(activity.Object);

            var options = new OpenTelemetryInterceptorOptions();

            var builder = new OpenTelemetryInterceptorBuilder()
                .WithActivitySource(activitySource.Object)
                .WithActivityKind(ActivityKind.Consumer)
                .WithTypeName("CustomType")
                .WithOptions(options)
                .TagWith("common.tag", "common value")
                .TagTarget<IDummyTarget>(t => new { t.Database, t.Container })
                .TagParameter<string>("arg1", arg1 => new { Arg1 = arg1 })
                .TagResult<int>(result => new { Result = result })
                .TagError<FormatException>(e => new { Message = e.Message });

            var interceptor = builder.Build();

            var intercepted = interceptor.Intercept(target.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            target.Verify(e => e.VoidMethod("value1"), Times.Once);

            activitySource.Verify(
                e => e.StartActivity(
                    "CustomType.VoidMethod", 
                    ActivityKind.Consumer,
                    It.IsAny<ActivityContext>(),
                    It.Is<ActivityTagsCollection>(tags => 
                        tags.Count == 4 && 
                        tags["common.tag"] == "common value" && 
                        tags["Database"] == "mydb" && 
                        tags["Container"] == "mycontainer" && 
                        tags["Arg1"] == "value1"),
                    null,
                    It.IsAny<DateTimeOffset>()),
                Times.Once);

            activity.Verify(
                e => e.SetTag(It.IsAny<string>(), It.IsAny<object>()),
                Times.Exactly(2));

            activity.Verify(
                e => e.SetTag("otel.status_code", 200),
                Times.Once);

            activity.Verify(
                e => e.SetTag("error", false),
                Times.Once);

            activity.Verify(
                e => e.Stop(),
                Times.Once);
        }

        [Fact]
        public void CallVoidMethodThrow()
        {
            // Given
            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.Database).Returns("mydb");
            target.Setup(e => e.Container).Returns("mycontainer");
            target
                .Setup(e => e.VoidMethod(It.IsAny<string>()))
                .Throws(new FormatException("Oops"));

            var activity = new Mock<IActivity>(MockBehavior.Loose);

            var activitySource = new Mock<IActivitySource>(MockBehavior.Loose);
            activitySource
                .Setup(e => e.StartActivity(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<ActivityContext>(),
                    It.IsAny<ActivityTagsCollection>(),
                    It.IsAny<IEnumerable<ActivityLink>>(),
                    It.IsAny<DateTimeOffset>()))
                .Returns(activity.Object);

            var options = new OpenTelemetryInterceptorOptions();

            var builder = new OpenTelemetryInterceptorBuilder()
                .WithActivitySource(activitySource.Object)
                .WithActivityKind(ActivityKind.Consumer)
                .WithTypeName("CustomType")
                .WithOptions(options)
                .TagWith("common.tag", "common value")
                .TagTarget<IDummyTarget>(t => new { t.Database, t.Container })
                .TagParameter<string>("arg1", arg1 => new { Arg1 = arg1 })
                .TagResult<int>(result => new { Result = result })
                .TagError<FormatException>(e => new { Message = e.Message });

            var interceptor = builder.Build();

            var intercepted = interceptor.Intercept(target.Object);

            // When
            Action action = () => intercepted.VoidMethod("value1");

            // Then
            action.Should().Throw<FormatException>();

            target.Verify(e => e.VoidMethod("value1"), Times.Once);

            activitySource.Verify(
                e => e.StartActivity(
                    "CustomType.VoidMethod",
                    ActivityKind.Consumer,
                    It.IsAny<ActivityContext>(),
                    It.Is<ActivityTagsCollection>(tags =>
                        tags.Count == 4 &&
                        tags["common.tag"] == "common value" &&
                        tags["Database"] == "mydb" &&
                        tags["Container"] == "mycontainer" &&
                        tags["Arg1"] == "value1"),
                    null,
                    It.IsAny<DateTimeOffset>()),
                Times.Once);

            activity.Verify(
                e => e.SetTag(It.IsAny<string>(), It.IsAny<object>()),
                Times.Exactly(3));

            activity.Verify(
                e => e.SetTag("Message", "Oops"),
                Times.Once);

            activity.Verify(
                e => e.SetTag("otel.status_code", 500),
                Times.Once);

            activity.Verify(
                e => e.SetTag("error", true),
                Times.Once);

            activity.Verify(
                e => e.Stop(),
                Times.Once);
        }

        [Fact]
        public void CallSyncMethod()
        {
            // Given
            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.Database).Returns("mydb");
            target.Setup(e => e.Container).Returns("mycontainer");
            target.Setup(e => e.SyncMethod(It.IsAny<string>())).Returns(42);

            var activity = new Mock<IActivity>(MockBehavior.Loose);
            
            var activitySource = new Mock<IActivitySource>(MockBehavior.Loose);
            activitySource
                .Setup(e => e.StartActivity(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<ActivityContext>(),
                    It.IsAny<ActivityTagsCollection>(),
                    It.IsAny<IEnumerable<ActivityLink>>(),
                    It.IsAny<DateTimeOffset>()))
                .Returns(activity.Object);

            var options = new OpenTelemetryInterceptorOptions();

            var builder = new OpenTelemetryInterceptorBuilder()
                .WithActivitySource(activitySource.Object)
                // .WithActivityKind(ActivityKind.Consumer)
                .WithTypeName("CustomType")
                .WithOptions(options)
                .TagWith("common.tag", "common value")
                .TagTarget<IDummyTarget>(t => new { t.Database, t.Container })
                .TagParameter<string>("arg1", arg1 => new { Arg1 = arg1 })
                .TagResult<int>(result => new { Result = result })
                .TagError<FormatException>(e => new { Message = e.Message });

            var interceptor = builder.Build();

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.SyncMethod("value1");

            // Then
            result.Should().Be(42);

            target.Verify(e => e.SyncMethod("value1"), Times.Once);

            activitySource.Verify(
                e => e.StartActivity(
                    "CustomType.SyncMethod", 
                    ActivityKind.Internal,
                    It.IsAny<ActivityContext>(),
                    It.Is<ActivityTagsCollection>(tags => 
                        tags.Count == 4 && 
                        tags["common.tag"] == "common value" && 
                        tags["Database"] == "mydb" && 
                        tags["Container"] == "mycontainer" && 
                        tags["Arg1"] == "value1"),
                    null,
                    It.IsAny<DateTimeOffset>()),
                Times.Once);

            activity.Verify(
                e => e.SetTag("Result", 42),
                Times.Once);

            activity.Verify(
                e => e.Stop(),
                Times.Once);
        }

        [Fact]
        public void CallProperty()
        {
            // Given
            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.Database).Returns("mydb");
            target.Setup(e => e.Container).Returns("mycontainer");

            var activity = new Mock<IActivity>(MockBehavior.Loose);

            var activitySource = new Mock<IActivitySource>(MockBehavior.Loose);
            activitySource
                .Setup(e => e.StartActivity(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<ActivityContext>(),
                    It.IsAny<ActivityTagsCollection>(),
                    It.IsAny<IEnumerable<ActivityLink>>(),
                    It.IsAny<DateTimeOffset>()))
                .Returns(activity.Object);

            var options = new OpenTelemetryInterceptorOptions();

            var builder = new OpenTelemetryInterceptorBuilder()
                .WithActivitySource(activitySource.Object)
                .WithActivityKind(ActivityKind.Consumer)
                .WithTypeName("CustomType")
                .WithOptions(options)
                .TagWith("common.tag", "common value")
                .TagTarget<IDummyTarget>(t => new { t.Database, t.Container })
                .TagParameter<string>("arg1", arg1 => new { Arg1 = arg1 })
                .TagResult<int>(result => new { Result = result })
                .TagError<FormatException>(e => new { Message = e.Message });

            var interceptor = builder.Build();

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = intercepted.Database;

            // Then
            result.Should().Be("mydb");

            target.Verify(e => e.Database, Times.Once);

            activitySource.Verify(
                e => e.StartActivity(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<ActivityContext>(),
                    It.IsAny<ActivityTagsCollection>(),
                    It.IsAny<IEnumerable<ActivityLink>>(),
                    It.IsAny<DateTimeOffset>()),
                Times.Never);

            activity.Verify(
                e => e.SetTag(It.IsAny<string>(), It.IsAny<object>()),
                Times.Never);

            activity.Verify(
                e => e.Stop(),
                Times.Never);
        }

        [Fact]
        public async Task CallTaskVoidMethod()
        {
            // Given
            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.Database).Returns("mydb");
            target.Setup(e => e.Container).Returns("mycontainer");
            target.Setup(e => e.TaskVoidMethod(It.IsAny<string>())).Returns(Task.CompletedTask);

            var activity = new Mock<IActivity>(MockBehavior.Loose);

            var activitySource = new Mock<IActivitySource>(MockBehavior.Loose);
            activitySource
                .Setup(e => e.StartActivity(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<ActivityContext>(),
                    It.IsAny<ActivityTagsCollection>(),
                    It.IsAny<IEnumerable<ActivityLink>>(),
                    It.IsAny<DateTimeOffset>()))
                .Returns(activity.Object);

            var options = new OpenTelemetryInterceptorOptions();

            var builder = new OpenTelemetryInterceptorBuilder()
                .WithActivitySource(activitySource.Object)
                .WithActivityKind(ActivityKind.Producer)
                .WithTypeName("CustomType")
                .WithOptions(options)
                .TagWith("common.tag", "common value")
                .TagTarget<IDummyTarget>(t => new { t.Database, t.Container })
                .TagParameter<string>("arg1", arg1 => new { Arg1 = arg1 })
                .TagResult<int>(result => new { Result = result })
                .TagError<FormatException>(e => new { Message = e.Message });

            var interceptor = builder.Build();

            var intercepted = interceptor.Intercept(target.Object);

            // When
            await intercepted.TaskVoidMethod("value1");

            // Then
            target.Verify(e => e.TaskVoidMethod("value1"), Times.Once);

            activitySource.Verify(
                e => e.StartActivity(
                    "CustomType.TaskVoidMethod",
                    ActivityKind.Producer,
                    It.IsAny<ActivityContext>(),
                    It.Is<ActivityTagsCollection>(tags =>
                        tags.Count == 4 &&
                        tags["common.tag"] == "common value" &&
                        tags["Database"] == "mydb" &&
                        tags["Container"] == "mycontainer" &&
                        tags["Arg1"] == "value1"),
                    null,
                    It.IsAny<DateTimeOffset>()),
                Times.Once);

            activity.Verify(
                e => e.SetTag(It.IsAny<string>(), It.IsAny<object>()),
                Times.Exactly(2));

            activity.Verify(
                e => e.SetTag("otel.status_code", 200),
                Times.Once);

            activity.Verify(
                e => e.SetTag("error", false),
                Times.Once);

            activity.Verify(
                e => e.Stop(),
                Times.Once);
        }

        [Fact]
        public async Task CallTaskMethod()
        {
            // Given
            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.Database).Returns("mydb");
            target.Setup(e => e.Container).Returns("mycontainer");
            target.Setup(e => e.TaskMethod(It.IsAny<string>())).Returns(Task.FromResult(42));

            var activity = new Mock<IActivity>(MockBehavior.Loose);

            var activitySource = new Mock<IActivitySource>(MockBehavior.Loose);
            activitySource
                .Setup(e => e.StartActivity(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<ActivityContext>(),
                    It.IsAny<ActivityTagsCollection>(),
                    It.IsAny<IEnumerable<ActivityLink>>(),
                    It.IsAny<DateTimeOffset>()))
                .Returns(activity.Object);

            var options = new OpenTelemetryInterceptorOptions();

            var builder = new OpenTelemetryInterceptorBuilder()
                .WithActivitySource(activitySource.Object)
                .WithActivityKind(ActivityKind.Producer)
                .WithTypeName("CustomType")
                .WithOptions(options)
                .TagWith("common.tag", "common value")
                .TagTarget<IDummyTarget>(t => new { t.Database, t.Container })
                .TagParameter<string>("arg1", arg1 => new { Arg1 = arg1 })
                .TagResult<int>(result => new { Result = result })
                .TagError<FormatException>(e => new { Message = e.Message });

            var interceptor = builder.Build();

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = await intercepted.TaskMethod("value1");

            // Then
            result.Should().Be(42);

            target.Verify(e => e.TaskMethod("value1"), Times.Once);

            activitySource.Verify(
                e => e.StartActivity(
                    "CustomType.TaskMethod",
                    ActivityKind.Producer,
                    It.IsAny<ActivityContext>(),
                    It.Is<ActivityTagsCollection>(tags =>
                        tags.Count == 4 &&
                        tags["common.tag"] == "common value" &&
                        tags["Database"] == "mydb" &&
                        tags["Container"] == "mycontainer" &&
                        tags["Arg1"] == "value1"),
                    null,
                    It.IsAny<DateTimeOffset>()),
                Times.Once);

            activity.Verify(
                e => e.SetTag("Result", 42),
                Times.Once);

            activity.Verify(
                e => e.Stop(),
                Times.Once);
        }

        [Fact]
        public async Task CallValueTaskVoidMethod()
        {
            // Given
            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.Database).Returns("mydb");
            target.Setup(e => e.Container).Returns("mycontainer");
            target
                .Setup(e => e.ValueTaskVoidMethod(It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask);

            var activity = new Mock<IActivity>(MockBehavior.Loose);

            var activitySource = new Mock<IActivitySource>(MockBehavior.Loose);
            activitySource
                .Setup(e => e.StartActivity(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<ActivityContext>(),
                    It.IsAny<ActivityTagsCollection>(),
                    It.IsAny<IEnumerable<ActivityLink>>(),
                    It.IsAny<DateTimeOffset>()))
                .Returns(activity.Object);

            var options = new OpenTelemetryInterceptorOptions();

            var builder = new OpenTelemetryInterceptorBuilder()
                .WithActivitySource(activitySource.Object)
                .WithActivityKind(ActivityKind.Producer)
                .WithTypeName("CustomType")
                .WithOptions(options)
                .TagWith("common.tag", "common value")
                .TagTarget<IDummyTarget>(t => new { t.Database, t.Container })
                .TagParameter<string>("arg1", arg1 => new { Arg1 = arg1 })
                .TagResult<int>(result => new { Result = result })
                .TagError<FormatException>(e => new { Message = e.Message });

            var interceptor = builder.Build();

            var intercepted = interceptor.Intercept(target.Object);

            // When
            await intercepted.ValueTaskVoidMethod("value1");

            // Then
            target.Verify(e => e.ValueTaskVoidMethod("value1"), Times.Once);

            activitySource.Verify(
                e => e.StartActivity(
                    "CustomType.ValueTaskVoidMethod",
                    ActivityKind.Producer,
                    It.IsAny<ActivityContext>(),
                    It.Is<ActivityTagsCollection>(tags =>
                        tags.Count == 4 &&
                        tags["common.tag"] == "common value" &&
                        tags["Database"] == "mydb" &&
                        tags["Container"] == "mycontainer" &&
                        tags["Arg1"] == "value1"),
                    null,
                    It.IsAny<DateTimeOffset>()),
                Times.Once);

            activity.Verify(
                e => e.SetTag(It.IsAny<string>(), It.IsAny<object>()),
                Times.Exactly(2));

            activity.Verify(
                e => e.SetTag("otel.status_code", 200),
                Times.Once);

            activity.Verify(
                e => e.SetTag("error", false),
                Times.Once);

            activity.Verify(
                e => e.Stop(),
                Times.Once);
        }

        [Fact]
        public async Task CallValueTaskMethod()
        {
            // Given
            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            target.Setup(e => e.Database).Returns("mydb");
            target.Setup(e => e.Container).Returns("mycontainer");
            target
                .Setup(e => e.ValueTaskMethod(It.IsAny<string>()))
                .Returns(ValueTask.FromResult(42));

            var activity = new Mock<IActivity>(MockBehavior.Loose);

            var activitySource = new Mock<IActivitySource>(MockBehavior.Loose);
            activitySource
                .Setup(e => e.StartActivity(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<ActivityContext>(),
                    It.IsAny<ActivityTagsCollection>(),
                    It.IsAny<IEnumerable<ActivityLink>>(),
                    It.IsAny<DateTimeOffset>()))
                .Returns(activity.Object);

            var options = new OpenTelemetryInterceptorOptions();

            var builder = new OpenTelemetryInterceptorBuilder()
                .WithActivitySource(activitySource.Object)
                .WithActivityKind(ActivityKind.Producer)
                .WithTypeName("CustomType")
                .WithOptions(options)
                .TagWith("common.tag", "common value")
                .TagTarget<IDummyTarget>(t => new { t.Database, t.Container })
                .TagParameter<string>("arg1", arg1 => new { Arg1 = arg1 })
                .TagResult<int>(result => new { Result = result })
                .TagError<FormatException>(e => new { Message = e.Message });

            var interceptor = builder.Build();

            var intercepted = interceptor.Intercept(target.Object);

            // When
            var result = await intercepted.ValueTaskMethod("value1");

            // Then
            result.Should().Be(42);

            target.Verify(e => e.ValueTaskMethod("value1"), Times.Once);

            activitySource.Verify(
                e => e.StartActivity(
                    "CustomType.ValueTaskMethod",
                    ActivityKind.Producer,
                    It.IsAny<ActivityContext>(),
                    It.Is<ActivityTagsCollection>(tags =>
                        tags.Count == 4 &&
                        tags["common.tag"] == "common value" &&
                        tags["Database"] == "mydb" &&
                        tags["Container"] == "mycontainer" &&
                        tags["Arg1"] == "value1"),
                    null,
                    It.IsAny<DateTimeOffset>()),
                Times.Once);

            activity.Verify(
                e => e.SetTag("Result", 42),
                Times.Once);

            activity.Verify(
                e => e.Stop(),
                Times.Once);
        }

        #endregion [ Calling methods ]

        public interface IDummyTarget
        {
            string Database { get; }
            string Container { get; }

            void VoidMethod(string arg1);

            int SyncMethod(string arg1);

            Task TaskVoidMethod(string arg1);

            Task<int> TaskMethod(string arg1);

            ValueTask ValueTaskVoidMethod(string arg1);

            ValueTask<int> ValueTaskMethod(string arg1);
        }
    }
}
