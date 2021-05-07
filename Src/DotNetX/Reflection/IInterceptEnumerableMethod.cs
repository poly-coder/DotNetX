using System;
using System.Reflection;

namespace DotNetX.Reflection
{
    public interface IInterceptEnumerableMethod
    {
        public bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args) => true;
        void Before(object target, MethodInfo targetMethod, object?[]? args);
        void Next(object target, MethodInfo targetMethod, object?[]? args, object? value);
        void Complete(object target, MethodInfo targetMethod, object?[]? args);
        void Error(object target, MethodInfo targetMethod, object?[]? args, Exception exception);
    }

    public interface IInterceptEnumerableMethod<TState>
    {
        public bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args) => true;
        TState Before(object target, MethodInfo targetMethod, object?[]? args);
        void Next(TState state, object target, MethodInfo targetMethod, object?[]? args, object? value);
        void Complete(TState state, object target, MethodInfo targetMethod, object?[]? args);
        void Error(TState state, object target, MethodInfo targetMethod, object?[]? args, Exception exception);
    }
}
