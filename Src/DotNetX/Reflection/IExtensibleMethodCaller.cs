using System;

namespace DotNetX.Reflection
{
    public interface IExtensibleMethodCaller
    {
#pragma warning disable CA1716
        object Call(object instance, IServiceProvider services, object[] requiredInputs, object[] optionalInputs);
#pragma warning restore CA1716
    }

    public interface IExtensibleMethodCaller<T, TResult>
    {
#pragma warning disable CA1716
        TResult Call(T instance, IServiceProvider services, object[] requiredInputs, object[] optionalInputs);
#pragma warning restore CA1716
    }

    public interface IExtensibleMethodCaller<T, TInput, TResult>
    {
#pragma warning disable CA1716
        TResult Call(T instance, IServiceProvider services, TInput input, params object[] optionalInputs);
#pragma warning restore CA1716
    }
}