using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetX.Reflection
{
    public interface IInterceptAsyncMethod
    {
        public bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args) => true;
        Task Before(object target, MethodInfo targetMethod, object?[]? args);
        Task After(object target, MethodInfo targetMethod, object?[]? args, object? result);
        Task Error(object target, MethodInfo targetMethod, object?[]? args, Exception exception);
    }

    public interface IInterceptAsyncMethod<TState>
    {
        public bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args) => true;
        Task<TState> Before(object target, MethodInfo targetMethod, object?[]? args);
        Task After(TState state, object target, MethodInfo targetMethod, object?[]? args, object? result);
        Task Error(TState state, object target, MethodInfo targetMethod, object?[]? args, Exception exception);
    }
}
