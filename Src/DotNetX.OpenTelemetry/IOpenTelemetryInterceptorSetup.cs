using System;
using System.Reflection;

namespace DotNetX.OpenTelemetry
{
    public interface IOpenTelemetryInterceptorSetup
    {
        bool InterceptAsync { get; }

        bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args);

        OpenTelemetryInterceptorState Before(object target, MethodInfo targetMethod, object?[]? args);

        void After(OpenTelemetryInterceptorState state, object target, MethodInfo targetMethod, object?[]? args, object? result);

        void Error(OpenTelemetryInterceptorState state, object target, MethodInfo targetMethod, object?[]? args, Exception exception);
    }
}
