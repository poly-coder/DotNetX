using FluentAssertions;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DotNetX.Logging.Tests
{
    public class LoggingInterceptorTests
    {
        [Fact]
        public void WithDefaultConfigurationItShouldNotFail()
        {
            // Given
            var builder = new LoggingInterceptorBuilder();

            // When
            var interceptor = builder.Build();

            // Then
            interceptor.Should().NotBeNull();
        }

        [Fact]
        public void WithDefaultConfigurationCallingInterceptShouldNotFail()
        {
            // Given
            var builder = new LoggingInterceptorBuilder();

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);

            // When
            var intercepted = interceptor.Intercept(target.Object);

            // Then
            intercepted.Should().NotBeNull();
        }


        [Fact]
        public void WithDefaultConfigurationItShouldInterceptVoidMethodsAsSuch()
        {
            // Given
            var builder = new LoggingInterceptorBuilder();

            var interceptor = builder.Build();

            var target = new Mock<IDummyTarget>(MockBehavior.Loose);
            
            var intercepted = interceptor.Intercept(target.Object);

            // When
            intercepted.VoidMethod("value1");

            // Then
            target.Verify(
                t => t.VoidMethod("value1"),
                Times.Once);
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

    }
}
