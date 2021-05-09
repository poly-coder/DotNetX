using System;
using System.Reflection;

namespace DotNetX.Logging
{
    public interface ILoggingInterceptorSetup
    {
        bool InterceptEnumerables { get; }
        
        bool InterceptAsync { get; }
        
        bool InterceptProperties { get; }
        
        bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args);

        LoggingInterceptorState Before(object target, MethodInfo targetMethod, object?[]? args);

        void After(LoggingInterceptorState state, object target, MethodInfo targetMethod, object?[]? args, object? result);

        void Complete(LoggingInterceptorState state, object target, MethodInfo targetMethod, object?[]? args);

        void Error(LoggingInterceptorState state, object target, MethodInfo targetMethod, object?[]? args, Exception exception);

        void Next(LoggingInterceptorState state, object target, MethodInfo targetMethod, object?[]? args, object? value);
    }
}
