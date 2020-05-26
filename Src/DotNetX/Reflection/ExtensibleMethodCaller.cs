using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNetX.Reflection
{
    public delegate object CallExtensibleMethod(object instance, IServiceProvider services, object[] requiredInputs, object[] optionalInputs);

    public class ExtensibleMethodCaller : IExtensibleMethodCaller
    {
        private readonly ConcurrentDictionary<Type, SpecificTypeInfo> cache = new ConcurrentDictionary<Type, SpecificTypeInfo>();
        private readonly Func<string, bool> isValidMethodName;
        private readonly Func<ParameterInfo[], bool> isValidInputType;
        private readonly Func<ParameterInfo, bool> isValidReturnType;
        private readonly CallExtensibleMethod onNotFound;

        private bool RequiredServices { get; }
        private string Name { get; }
        private BindingFlags BindingFlags { get; }

        public ExtensibleMethodCaller(
            string name,
            Func<string, bool> isValidMethodName = null,
            Func<ParameterInfo[], bool> isValidInputType = null,
            Func<ParameterInfo, bool> isValidReturnType = null,
            CallExtensibleMethod onNotFound = null,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance,
            bool requiredServices = true)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            RequiredServices = requiredServices;

            this.BindingFlags = bindingFlags | BindingFlags.Instance;

            this.isValidMethodName = isValidMethodName ?? name.IsSameOrdinalPredicate();
            this.isValidInputType = isValidInputType ?? Predicate.True;
            this.isValidReturnType = isValidReturnType ?? Predicate.True;
            this.onNotFound = onNotFound ?? DefaultOnNotFound;
        }

        public SpecificTypeInfo GetInfo(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var info = cache.GetOrAdd(type, CreateSpecificTypeInfo);
            return info;
        }

        public object Call(object instance, IServiceProvider services, object[] requiredInputs, object[] optionalInputs)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (RequiredServices && services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (HaveNullInputs(requiredInputs))
            {
                throw new ArgumentNullException(nameof(requiredInputs));
            }

            if (HaveNullInputs(optionalInputs))
            {
                throw new ArgumentNullException(nameof(optionalInputs));
            }

            return CallSpecific(instance, services, requiredInputs, optionalInputs);
        }

        private object CallSpecific(object instance, IServiceProvider services, object[] requiredInputs, object[] optionalInputs)
        {
            var instanceType = instance.GetType();

            var instanceInfo = GetInfo(instanceType);

            requiredInputs ??= Array.Empty<object>();
            optionalInputs ??= Array.Empty<object>();

            if (instanceInfo.IsEmpty)
            {
                return onNotFound(instance, services, requiredInputs, optionalInputs);
            }

            (MethodInfo method, object[] parameters) = instanceInfo.GetMethodToCall(services, requiredInputs, optionalInputs);

            if (method == null || parameters == null)
            {
                return onNotFound(instance, services, requiredInputs, optionalInputs);
            }

            try
            {
                return method.Invoke(instance, parameters);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        private object DefaultOnNotFound(object instance, IServiceProvider services, object[] requiredInputs, object[] optionalInputs)
        {
            throw new NotImplementedException(
                $"Method caller {Name} did not found a method for type {instance.GetType().FullName} {FormatInputTypes(requiredInputs, "required")} and {FormatInputTypes(optionalInputs, "optional")}");
        }

        private bool HaveNullInputs(object[] inputs)
        {
            if (inputs == null || inputs.Length == 0)
            {
                return false;
            }
            return inputs.Any(e => e == null);
        }

        private string FormatInputTypes(object[] inputs, string inputType)
        {
            if (inputs == null || inputs.Length == 0)
            {
                return $"with no {inputType} inputs";
            }
            var types = inputs.Select(e => e.GetType().FormatName()).Concatenate();
            return $"with {inputType} inputs of types ({types})";
        }

        private SpecificTypeInfo CreateSpecificTypeInfo(Type type)
        {
            var methods = type
                .SelectMethods(BindingFlags, isValidMethodName, isValidInputType, isValidReturnType)
                .ToArray();

            return new SpecificTypeInfo(type, methods.Length == 0 ? null : methods);
        }

        public class SpecificTypeInfo
        {
            private readonly ConcurrentDictionary<InputParameterTypesKey, InputParameterSignature> cache = new ConcurrentDictionary<InputParameterTypesKey, InputParameterSignature>();

            internal SpecificTypeInfo(Type declaringType, MethodInfo[] methods)
            {
                DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
                Methods = methods;
            }

            public bool IsEmpty => Methods == null;
            public readonly Type DeclaringType;
            public readonly IEnumerable<MethodInfo> Methods;

            public (MethodInfo method, object[] parameters) GetMethodToCall(IServiceProvider services, object[] requiredInputs, object[] optionalInputs)
            {
                var callKey = new InputParameterTypesKey(
                    requiredInputs.Select(e => e.GetType()).ToArray(),
                    optionalInputs.Select(e => e.GetType()).ToArray());
                
                var signature = cache.GetOrAdd(callKey, CreateSignature);

                if (signature.IsEmpty)
                {
                    return (null, null);
                }

                return (signature.Method, signature.CreateParameters(services, requiredInputs, optionalInputs));
            }

            private InputParameterSignature CreateSignature(InputParameterTypesKey callKey)
            {
                var candidates = Methods
                    .Select(method =>
                    {
                        var parameters = method.GetParameters();
                        var positions = new InputParameterPosition[parameters.Length];
                        var requiredCheckCount = 0;
                        var requiredChecks = new bool[callKey.RequiredInputTypes.Count];
                        var optionalChecks = new bool[callKey.OptionalInputTypes.Count];
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var type = parameters[i].ParameterType;
                            var index = callKey.RequiredInputTypes.IndexOf(type.IsAssignableFromPredicate());
                            if (index >= 0 || requiredChecks[index])
                            {
                                positions[i] = new InputParameterPosition(index, true);
                                requiredChecks[index] = true;
                                requiredCheckCount++;
                            }
                            else
                            {
                                index = callKey.OptionalInputTypes.IndexOf(type.IsAssignableFromPredicate());
                                if (index >= 0 || optionalChecks[index])
                                {
                                    positions[i] = new InputParameterPosition(index, false);
                                    optionalChecks[index] = true;
                                }
                                else
                                {
                                    positions[i] = new InputParameterPosition(type);
                                }
                            }
                        }

                        if (requiredCheckCount != callKey.RequiredInputTypes.Count)
                        {
                            return (method: null, positions: null);
                        }

                        return (method, positions);
                    })
                    .Where(item => item.positions != null)
                    .ToArray();

                if (candidates.Length > 1)
                {
                    var requiredTypes = callKey.RequiredInputTypes.Select(t => t.FormatName()).Concatenate();
                    var optionalTypes = callKey.OptionalInputTypes.Select(t => t.FormatName()).Concatenate();
                    throw new InvalidOperationException(
                        $"More than one candidate was found for:\n" +
                        "  required types: [{requiredTypes}]\n" + 
                        "  optional types: [{optionalTypes}]");
                }

                if (candidates.Length == 0)
                {
                    return InputParameterSignature.Empty;
                }

                return new InputParameterSignature(candidates[0].method, candidates[0].positions);
            }
        }

        public class InputParameterTypesKey : IEquatable<InputParameterTypesKey>
        {
            static readonly IEqualityComparer<IReadOnlyList<Type>> comparer = StructuralReadOnlyListEqualityComparer<Type>.Default;

            public readonly IReadOnlyList<Type> RequiredInputTypes;
            public readonly IReadOnlyList<Type> OptionalInputTypes;

            internal InputParameterTypesKey(IReadOnlyList<Type> requiredInputTypes, IReadOnlyList<Type> optionalInputTypes)
            {
                RequiredInputTypes = requiredInputTypes ?? throw new ArgumentNullException(nameof(requiredInputTypes));
                OptionalInputTypes = optionalInputTypes ?? throw new ArgumentNullException(nameof(optionalInputTypes));
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as InputParameterTypesKey);
            }

            public bool Equals(InputParameterTypesKey other)
            {
                return other != null &&
                    comparer.Equals(RequiredInputTypes, other.RequiredInputTypes) &&
                    comparer.Equals(OptionalInputTypes, other.OptionalInputTypes);
            }

            public override int GetHashCode()
            {
                var hasher = new HashCode();
                hasher.Add(RequiredInputTypes, comparer);
                hasher.Add(OptionalInputTypes, comparer);
                return hasher.ToHashCode();
            }

            public static bool operator ==(InputParameterTypesKey left, InputParameterTypesKey right)
            {
                return EqualityComparer<InputParameterTypesKey>.Default.Equals(left, right);
            }

            public static bool operator !=(InputParameterTypesKey left, InputParameterTypesKey right)
            {
                return !(left == right);
            }
        }

        public class InputParameterSignature
        {
            public static readonly InputParameterSignature Empty = new InputParameterSignature();

            public readonly MethodInfo Method;
            public readonly IReadOnlyList<InputParameterPosition> Positions;

            private InputParameterSignature()
            {
            }

            public InputParameterSignature(MethodInfo method, IReadOnlyList<InputParameterPosition> positions)
            {
                Method = method ?? throw new ArgumentNullException(nameof(method));
                Positions = positions ?? throw new ArgumentNullException(nameof(positions));
            }

            public bool IsEmpty => Method is null || Positions is null;

            public object[] CreateParameters(IServiceProvider services, object[] requiredInputs, object[] optionalInputs)
            {
                int count = Positions.Count;
                var parameters = new object[count];
                for (int i = 0; i < count; i++)
                {
                    var parameter = Positions[i];
                    switch (parameter.ParameterSource)
                    {
                        case InputParameterPositionType.Provided:
                            parameters[i] = services.GetService(parameter.ServiceType);
                            break;
                        case InputParameterPositionType.Required:
                            parameters[i] = requiredInputs[parameter.Index];
                            break;
                        case InputParameterPositionType.Optional:
                            parameters[i] = optionalInputs[parameter.Index];
                            break;
                        default:
                            throw new NotImplementedException($"This case is not supported: {parameter.ParameterSource}");
                    }
                }
                return parameters;
            }
        }

        public enum InputParameterPositionType
        {
            Provided,
            Required,
            Optional,
        }

        public class InputParameterPosition
        {
            public InputParameterPosition(Type serviceType)
            {
                ParameterSource = InputParameterPositionType.Provided;
                ServiceType = serviceType;
                Index = -1;
            }

            public InputParameterPosition(int index, bool isRequired)
            {
                ParameterSource = isRequired ? InputParameterPositionType.Required : InputParameterPositionType.Optional;
                Index = index;
                ServiceType = null;
            }

            public InputParameterPositionType ParameterSource { get; }
            public Type ServiceType { get; }
            public int Index { get; }
        }
    }

    public class ExtensibleMethodCaller<T, TResult> : IExtensibleMethodCaller<T, TResult>
    {
        private readonly ExtensibleMethodCaller innerCaller;
        private readonly Func<object, TResult> toResult;

        public ExtensibleMethodCaller(
            ExtensibleMethodCaller innerCaller,
            Func<object, TResult> toResult = null)
        {
            this.innerCaller = innerCaller ?? throw new ArgumentNullException(nameof(innerCaller));
            this.toResult = toResult ?? (obj => (TResult)obj);
        }

        public ExtensibleMethodCaller.SpecificTypeInfo GetInfo(Type type)
        {
            return innerCaller.GetInfo(type);
        }

        public TResult Call(T instance, IServiceProvider services, object[] requiredInputs, object[] optionalInputs)
        {
            var obj = innerCaller.Call(instance, services, requiredInputs, optionalInputs);

            return toResult(obj);
        }
    }

    public class ExtensibleMethodCaller<T, TInput, TResult> : IExtensibleMethodCaller<T, TInput, TResult>
    {
        private readonly ExtensibleMethodCaller innerCaller;
        private readonly Func<object, TResult> toResult;

        public ExtensibleMethodCaller(
            ExtensibleMethodCaller innerCaller,
            Func<object, TResult> toResult = null)
        {
            this.innerCaller = innerCaller ?? throw new ArgumentNullException(nameof(innerCaller));
            this.toResult = toResult ?? (obj => (TResult)obj);
        }

        public ExtensibleMethodCaller.SpecificTypeInfo GetInfo(Type type)
        {
            return innerCaller.GetInfo(type);
        }

        public TResult Call(T instance, IServiceProvider services, TInput input, params object[] optionalInputs)
        {
            var obj = innerCaller.Call(instance, services, new object[] { input }, optionalInputs);

            return toResult(obj);
        }
    }
}
