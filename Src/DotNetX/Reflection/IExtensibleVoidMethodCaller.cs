using System;

namespace DotNetX.Reflection
{
    public interface IExtensibleVoidMethodCaller<T>
    {
#pragma warning disable CA1716
        void Call(T instance, IServiceProvider services, object[] requiredInputs, object[] optionalInputs);
#pragma warning restore CA1716
    }

    public interface IExtensibleVoidMethodCaller<T, TInput>
    {
#pragma warning disable CA1716
        void Call(T instance, IServiceProvider services, TInput input, params object[] optionalInputs);
#pragma warning restore CA1716
    }
}
