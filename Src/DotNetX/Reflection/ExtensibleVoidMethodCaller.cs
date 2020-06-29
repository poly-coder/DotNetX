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
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (requiredInputs is null)
            {
                throw new ArgumentNullException(nameof(requiredInputs));
            }

            if (optionalInputs is null)
            {
                throw new ArgumentNullException(nameof(optionalInputs));
            }

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
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (optionalInputs is null)
            {
                throw new ArgumentNullException(nameof(optionalInputs));
            }

            innerCaller.Call(instance, services, new object[] { input }, optionalInputs);
        }
    }
}
