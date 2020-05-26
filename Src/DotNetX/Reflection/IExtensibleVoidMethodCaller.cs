using System;

namespace DotNetX.Reflection
{
    public interface IExtensibleVoidMethodCaller<T>
    {
        void Call(T instance, IServiceProvider services, object[] requiredInputs, object[] optionalInputs);
    }

    public interface IExtensibleVoidMethodCaller<T, TInput>
    {
        void Call(T instance, IServiceProvider services, TInput input, params object[] optionalInputs);
    }
}
