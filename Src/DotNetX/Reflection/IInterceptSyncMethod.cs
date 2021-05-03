using System;
using System.Reflection;

namespace DotNetX.Reflection
{
    public interface IInterceptMethodBase
    {
        public bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args) => true;
    }

    public interface IInterceptSyncMethod : IInterceptMethodBase
    {
        void Before(object target, MethodInfo targetMethod, object?[]? args);
        void After(object target, MethodInfo targetMethod, object?[]? args, object? result);
        void Error(object target, MethodInfo targetMethod, object?[]? args, Exception exception);
    }

    public interface IInterceptSyncMethod<TState> : IInterceptMethodBase
    {
        TState Before(object target, MethodInfo targetMethod, object?[]? args);
        void After(TState state, object target, MethodInfo targetMethod, object?[]? args, object? result);
        void Error(TState state, object target, MethodInfo targetMethod, object?[]? args, Exception exception);
    }
}
