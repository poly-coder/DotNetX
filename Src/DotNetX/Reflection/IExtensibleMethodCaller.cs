using System;

namespace DotNetX.Reflection
{
    public interface IExtensibleMethodCaller
    {
        object? Call(object instance, IServiceProvider services, object[]? requiredInputs, object[]? optionalInputs);
    }

    public interface IExtensibleMethodCaller<T, TResult>
    {
        TResult Call(T instance, IServiceProvider services, object[]? requiredInputs, object[]? optionalInputs);
    }

    public interface IExtensibleMethodCaller<T, TInput, TResult>
    {
        TResult Call(T instance, IServiceProvider services, TInput input, params object[] optionalInputs);
    }
}