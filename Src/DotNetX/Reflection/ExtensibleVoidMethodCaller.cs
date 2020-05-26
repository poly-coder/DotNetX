using System;

namespace DotNetX.Reflection
{
    public class ExtensibleVoidMethodCaller<T> : IExtensibleVoidMethodCaller<T>
    {
        private readonly ExtensibleMethodCaller innerCaller;

        public ExtensibleVoidMethodCaller(
            ExtensibleMethodCaller innerCaller)
        {
            this.innerCaller = innerCaller ?? throw new ArgumentNullException(nameof(innerCaller));
        }

        public ExtensibleMethodCaller.SpecificTypeInfo GetInfo(Type type)
        {
            return innerCaller.GetInfo(type);
        }

        public void Call(T instance, IServiceProvider services, object[] requiredInputs, object[] optionalInputs)
        {
            innerCaller.Call(instance, services, requiredInputs, optionalInputs);
        }
    }

    public class ExtensibleVoidMethodCaller<T, TInput> : IExtensibleVoidMethodCaller<T, TInput>
    {
        private readonly ExtensibleMethodCaller innerCaller;

        public ExtensibleVoidMethodCaller(
            ExtensibleMethodCaller innerCaller)
        {
            this.innerCaller = innerCaller ?? throw new ArgumentNullException(nameof(innerCaller));
        }

        public ExtensibleMethodCaller.SpecificTypeInfo GetInfo(Type type)
        {
            return innerCaller.GetInfo(type);
        }

        public void Call(T instance, IServiceProvider services, TInput input, params object[] optionalInputs)
        {
            innerCaller.Call(instance, services, new object[] { input }, optionalInputs);
        }
    }
}
