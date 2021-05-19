using DotNetX.Reflection;
using System;
using System.Reflection;

namespace DotNetX.OpenTelemetry
{
    public class OpenTelemetryInterceptor :
        IInterceptSyncMethod<OpenTelemetryInterceptorState>
    {
        private readonly IOpenTelemetryInterceptorSetup setup;

        public OpenTelemetryInterceptor(IOpenTelemetryInterceptorSetup setup)
        {
            this.setup = setup ?? throw new ArgumentNullException(nameof(setup));
        }

        public T Intercept<T>(T target) where T : class
        {
            var options = InterceptorOptions.Default;

            if (setup.InterceptAsync)
            {
                options = options
                    .Add(InterceptAsyncMethod<OpenTelemetryInterceptorState>.Default.With(this));
            }

            options = options
                .Add(InterceptSyncMethod<OpenTelemetryInterceptorState>.Default.With(this));

            return options.CreateInterceptor(target);

        }
        public OpenTelemetryInterceptorState Before(object target, MethodInfo targetMethod, object?[]? args)
        {
            throw new NotImplementedException();
        }

        public void After(OpenTelemetryInterceptorState state, object target, MethodInfo targetMethod, object?[]? args,
            object? result)
        {
            throw new NotImplementedException();
        }

        public void Error(OpenTelemetryInterceptorState state, object target, MethodInfo targetMethod, object?[]? args,
            Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}
