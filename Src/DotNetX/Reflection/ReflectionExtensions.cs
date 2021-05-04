using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetX.Reflection
{
    public static class ReflectionExtensions
    {
        #region [ FormatName / FormatSignature ]

        private static readonly Dictionary<Type, string> StandardTypeShorFormats = new Dictionary<Type, string>
        { 
            [typeof(string)] = "string",
            [typeof(char)] = "char",
            [typeof(bool)] = "bool",
            [typeof(byte)] = "byte",
            [typeof(ushort)] = "ushort",
            [typeof(uint)] = "uint",
            [typeof(ulong)] = "ulong",
            [typeof(sbyte)] = "sbyte",
            [typeof(short)] = "short",
            [typeof(int)] = "int",
            [typeof(long)] = "long",
            [typeof(float)] = "float",
            [typeof(double)] = "double",
            [typeof(decimal)] = "decimal",
            [typeof(object)] = "object",
            [typeof(void)] = "void",
        };

        public static string FormatName(this Type type, bool fullName = false)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            // TODO: Check other special types like delegates, built-in, ...

            if (!fullName && StandardTypeShorFormats.TryGetValue(type, out var format))
            {
                return format;
            }

            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType()!.FormatName(fullName);
                var rank = type.GetArrayRank();
                var commas = new String(',', rank - 1);
                return $"{elementType}[{commas}]";
            }

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                var method = type.GetMethod("Invoke")!;
                return method.FormatSignature(type.Name, fullName);
            }

            var typeName = fullName ? type.FullName : type.Name;

            if (type.IsGenericType)
            {
                var builder = new StringBuilder();
                builder.Append(typeName.Before('`')).Append("<");

                var args = type.GetGenericArguments();
                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0) builder.Append(", ");
                    builder.Append(args[i].FormatName(fullName));
                }

                builder.Append(">");

                return builder.ToString();
            }
            return typeName!;
        }

        public static string FormatSignature(this MethodInfo method, string? methodName = null, bool fullName = false)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            methodName ??= fullName ? $"{method.DeclaringType!.FormatName(true)}.{method.Name}" : method.Name;
            var sb = new StringBuilder();
            sb.Append(method.ReturnType.FormatName(fullName)).Append(" ").Append(methodName).Append("(");
            var parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(parameter.ParameterType.FormatName(fullName)).Append(" ").Append(parameter.Name);
            }
            sb.Append(")");
            return sb.ToString();
        }

        #endregion [ FormatName / FormatSignature ]


        #region [ SelectMethods ]

        public static IEnumerable<MethodInfo> SelectMethods(this Type type,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance,
            Func<string, bool>? isValidMethodName = null,
            Func<ParameterInfo[], bool>? isValidInputType = null,
            Func<ParameterInfo, bool>? isValidReturnType = null)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            IEnumerable<MethodInfo> query = type.GetMethods(bindingFlags);
            query = isValidMethodName == null ? query : query.Where(method => isValidMethodName(method.Name));
            query = isValidReturnType == null ? query : query.Where(method => isValidReturnType(method.ReturnParameter));
            query = isValidInputType == null ? query : query.Where(method => isValidInputType(method.GetParameters()));
            return query;
        }

        #endregion [ SelectMethods ]


        #region [ GetAttributes / GetConventionValue ]

        public static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider provider, Type attributeType, bool inherit)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (attributeType is null)
            {
                throw new ArgumentNullException(nameof(attributeType));
            }

            return provider.GetCustomAttributes(attributeType, inherit).Cast<Attribute>();
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ICustomAttributeProvider provider, bool inherit)
            where TAttribute : Attribute
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return provider.GetAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }

        public static Attribute? GetAttribute(this ICustomAttributeProvider provider, Type attributeType, bool inherit)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (attributeType is null)
            {
                throw new ArgumentNullException(nameof(attributeType));
            }

            return provider.GetAttributes(attributeType, inherit).FirstOrDefault();
        }

        public static TAttribute? GetAttribute<TAttribute>(this ICustomAttributeProvider provider, bool inherit)
            where TAttribute : Attribute
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return provider.GetAttributes<TAttribute>(inherit).FirstOrDefault();
        }

        public static TValue GetConventionValue<TProvider, TAttribute, TValue>(
            this TProvider provider, 
            bool inherit, 
            Func<TProvider, TValue> ofConvention,
            Func<TAttribute, TValue> ofAttribute)
            where TProvider : ICustomAttributeProvider
            where TAttribute : Attribute
            where TValue : class
        {
            if (ofConvention is null)
            {
                throw new ArgumentNullException(nameof(ofConvention));
            }

            if (ofAttribute is null)
            {
                throw new ArgumentNullException(nameof(ofAttribute));
            }

            var attr = provider.GetAttribute<TAttribute>(inherit);
            if (attr != null)
            {
                var result = ofAttribute(attr);
                if (result != null)
                {
                    return result;
                }
            }
            return ofConvention(provider);
        }

        public static TValue GetConventionValue<TProvider, TAttribute, TValue>(
            this TProvider provider, 
            bool inherit, 
            Func<TProvider, TValue> ofConvention,
            Func<TAttribute, TValue?> ofAttribute)
            where TProvider : ICustomAttributeProvider
            where TAttribute : Attribute
            where TValue : struct
        {
            if (ofConvention is null)
            {
                throw new ArgumentNullException(nameof(ofConvention));
            }

            if (ofAttribute is null)
            {
                throw new ArgumentNullException(nameof(ofAttribute));
            }

            var attr = provider.GetAttribute<TAttribute>(inherit);
            if (attr != null)
            {
                var result = ofAttribute(attr);
                if (result.HasValue)
                {
                    return result.Value;
                }
            }
            return ofConvention(provider);
        }

        #endregion [ GetAttributes / GetConventionValue ]


        #region [ GetClassHierarchy / GetTypeHierarchy ]

        public static IEnumerable<Type> GetClassHierarchy(this Type type)
        {
            return type.Unfold(t => t.BaseType);
        }

        public static IEnumerable<Type> GetTypeHierarchy(this Type type)
        {
            return type.DepthFirstSearch(t => t.BaseType.SingletonNonNull().Concat(t.GetInterfaces()));
        }

        #endregion [ GetClassHierarchy / GetTypeHierarchy ]


        #region [ Predicates ]

        public static Func<Type, bool> IsAssignableFromPredicate(this Type baseType)
        {
            return type => baseType.IsAssignableFrom(type);
        }

        public static Func<Type, bool> HaveClassInHierarchyPredicate(this Type baseClass)
        {
            return type => type.GetClassHierarchy().Any(t => t == baseClass);
        }

        public static Func<Type, bool> HaveTypeInHierarchyPredicate(this Type baseClass)
        {
            return type => type.GetTypeHierarchy().Any(t => t == baseClass);
        }

        public static Func<Type, bool> ConformsToPredicate(this Type shapeType)
        {
            return type => type.ConformsTo(shapeType);
        }

        public static Func<Type, bool> ConformsToGenericPredicate(
            this Type typeDefinition,
            params Type[] arguments)
        {
            return type => type.ConformsToGeneric(typeDefinition, arguments);
        }

        public static Func<Type, bool> ConformsToGenericPredicate(
            this Type typeDefinition,
            params Func<Type, bool>[] argumentPredicates)
        {
            return type => type.ConformsToGeneric(typeDefinition.IsAssignableFromPredicate(), argumentPredicates);
        }

        public static Func<Type, bool> ConformsToGenericPredicate(
            this Func<Type, bool> typeDefinitionPredicate,
            params Func<Type, bool>[] argumentPredicates)
        {
            return type => type.ConformsToGeneric(typeDefinitionPredicate, argumentPredicates);
        }

        #endregion [ Predicates ]


        #region [ ConformsTo ]

        public static bool ConformsTo(this Type sourceType, Type shapeType)
        {
            if (sourceType is null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            if (shapeType is null)
            {
                throw new ArgumentNullException(nameof(shapeType));
            }

            if (shapeType.IsAssignableFrom(sourceType))
            {
                return true;
            }

            if (shapeType.IsGenericType)
            {
                return sourceType.ConformsToGeneric(
                    shapeType.GetGenericTypeDefinition(),
                    shapeType.GetGenericArguments());
            }

            return false;
        }

        public static bool ConformsToGeneric(
            this Type sourceType, 
            Type typeDefinition, 
            params Type[] arguments)
        {
            if (sourceType is null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            if (typeDefinition is null)
            {
                throw new ArgumentNullException(nameof(typeDefinition));
            }

            if (arguments is null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            if (!sourceType.IsGenericType) return false;

            Type sourceTypeDefinition = sourceType.GetGenericTypeDefinition();
            //if (typeDefinition == sourceTypeDefinition || typeDefinition.IsAssignableFrom(sourceTypeDefinition)) return false;
            if (sourceTypeDefinition.GetTypeHierarchy().All(t => t != typeDefinition)) return false;

            var sourceArgs = sourceType.GetGenericArguments();

            if (sourceArgs.Length != arguments.Length) return false;

            for (int i = 0; i < sourceArgs.Length; i++)
            {
                if (!sourceArgs[i].ConformsTo(arguments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ConformsToGeneric(
            this Type sourceType, 
            Func<Type, bool> typeDefinitionPredicate, 
            params Func<Type, bool>[] argumentPredicates)
        {
            if (typeDefinitionPredicate is null)
            {
                throw new ArgumentNullException(nameof(typeDefinitionPredicate));
            }

            if (argumentPredicates is null)
            {
                throw new ArgumentNullException(nameof(argumentPredicates));
            }

            if (sourceType is null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            if (!sourceType.IsGenericType) return false;

            if (!typeDefinitionPredicate(sourceType.GetGenericTypeDefinition())) return false;

            var sourceArgs = sourceType.GetGenericArguments();

            if (sourceArgs.Length != argumentPredicates.Length) return false;

            for (int i = 0; i < sourceArgs.Length; i++)
            {
                if (!argumentPredicates[i](sourceArgs[i]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion [ ConformsTo ]


        #region [ ExportedTypes / ConcreteClassesImplementing / ... ]

        public static IEnumerable<Type> ExportedTypes(this IEnumerable<Assembly> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.SelectMany(assembly => assembly.GetExportedTypes());
        }

        public static IEnumerable<Type> ConcreteClassesImplementing<TBase>(this IEnumerable<Type> source)
            where TBase : class
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Where(IsConcreteClassImplementing<TBase>);
        }

        public static bool IsConcreteClassImplementing<TBase>(this Type type)
            where TBase : class
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.IsClass && !type.IsAbstract && typeof(TBase).IsAssignableFrom(type);
        }

        public static IEnumerable<TInstance> ActivateAs<TInstance>(
            this IEnumerable<Type> types,
            Func<Type, TInstance>? activator = null) =>
            types.Select(activator ?? SystemActivator<TInstance>);

        public static TInstance SystemActivator<TInstance>(this Type type) =>
            (TInstance)Activator.CreateInstance(type)!;

        #endregion [ ExportedTypes / ConcreteClassesImplementing / ... ]


        #region [ InvokeWith ]
        
        private static object?[] ExtractInvokeParameters(
            MethodBase method,
            IServiceProvider? serviceProvider = null,
            IReadOnlyCollection<object>? instances = null,
            IReadOnlyDictionary<string, object>? namedInstances = null)
        {
            var args = method.GetParameters();

            return args.Select(p => GetValueFor(p)).ToArray();

            object? GetValueFor(ParameterInfo p)
            {
                var type = p.ParameterType;

                if (p.Name != null && 
                    namedInstances != null && 
                    namedInstances.TryGetValue(p.Name, out var namedInstanceValue))
                {
                    if (namedInstanceValue != null && !type.IsAssignableFrom(namedInstanceValue.GetType()))
                    {
                        throw new ArgumentException(
                            $"Found a named instance for '{p.Name}' with expected type '{type.Name}' but actual type '{namedInstanceValue.GetType().Name}'");
                    }

                    return namedInstanceValue;
                }

                var instanceValue = instances != null 
                    ?  instances.FirstOrDefault(e => type.IsAssignableFrom(e.GetType()))
                    : null;

                if (instanceValue != null)
                {
                    return instanceValue;
                }

                var serviceValue = serviceProvider?.GetService(type);

                if (serviceValue != null)
                {
                    return serviceValue;
                }

                if (!p.IsOptional)
                {
                    throw new ArgumentException(
                        $"Could not find a value for parameter '{p.Name}' with type '{type.Name}'");
                }

                return null;
            }
        }

        public static object? InvokeStaticWith(
            this MethodInfo method, 
            IServiceProvider? serviceProvider = null,
            IReadOnlyCollection<object>? instances = null,
            IReadOnlyDictionary<string, object>? namedInstances = null)
        {
            var parameters = ExtractInvokeParameters(method, serviceProvider, instances, namedInstances);

            return method.Invoke(null, parameters);
        }
        
        public static object? InvokeWith(
            this MethodInfo method, 
            object instance,
            IServiceProvider? serviceProvider = null,
            IReadOnlyCollection<object>? instances = null,
            IReadOnlyDictionary<string, object>? namedInstances = null)
        {
            var parameters = ExtractInvokeParameters(method, serviceProvider, instances, namedInstances);

            return method.Invoke(instance, parameters);
        }

        public static object InvokeStaticWith(
            this MethodInfo method, 
            IServiceProvider? serviceProvider,
            params object[] instances)
        {
            return InvokeStaticWith(method, serviceProvider: serviceProvider, instances: instances);
        }

        public static object InvokeStaticWith(
            this MethodInfo method, 
            params object[] instances)
        {
            return InvokeStaticWith(method, serviceProvider: null, instances: instances);
        }

        public static object InvokeWith(
            this MethodInfo method,
            object instance,
            IServiceProvider? serviceProvider,
            params object[] instances)
        {
            return InvokeWith(method, instance, serviceProvider: serviceProvider, instances: instances);
        }

        public static object InvokeWith(
            this MethodInfo method,
            object instance,
            params object[] instances)
        {
            return InvokeWith(method, instance, serviceProvider: null, instances: instances);
        }

        #endregion [ InvokeWith ]


        #region [ TryGetGenericParameters ]

        public static bool TryGetAllGenericParameters(
            this Type type,
            Type genericTypeDefinition,
            [MaybeNullWhen(false)]
            out Type[] parameters)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (genericTypeDefinition is null)
            {
                throw new ArgumentNullException(nameof(genericTypeDefinition));
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
            {
                parameters = type.GenericTypeArguments;
                return true;
            }

            parameters = null;
            return false;
        }

        public static bool TryGetGenericParameters(
            this Type type,
            Type genericTypeDefinition,
            [MaybeNullWhen(false)]
            out Type first)
        {
            if (type.TryGetAllGenericParameters(genericTypeDefinition, out var types) &&
                types.Length == 1)
            {
                first = types[0];
                return true;
            }

            first = null;
            return false;
        }

        public static bool TryGetGenericParameters(
            this Type type,
            Type genericTypeDefinition,
            [MaybeNullWhen(false)]
            out Type first,
            [MaybeNullWhen(false)]
            out Type second)
        {
            if (type.TryGetAllGenericParameters(genericTypeDefinition, out var types) &&
                types.Length == 2)
            {
                first = types[0];
                second = types[1];
                return true;
            }

            first = null;
            second = null;
            return false;
        }

        public static bool TryGetGenericParameters(
            this Type type,
            Type genericTypeDefinition,
            [MaybeNullWhen(false)]
            out Type first,
            [MaybeNullWhen(false)]
            out Type second,
            [MaybeNullWhen(false)]
            out Type third)
        {
            if (type.TryGetAllGenericParameters(genericTypeDefinition, out var types) &&
                types.Length == 3)
            {
                first = types[0];
                second = types[1];
                third = types[2];
                return true;
            }

            first = null;
            second = null;
            third = null;
            return false;
        }

        #endregion [ TryGetGenericParameters ]


        #region [ TryGetDeclaringProperty ]

        public static bool TryGetDeclaringProperty(
            this MethodInfo method, 
            [NotNullWhen(true)]
            out PropertyInfo? property)
        {
            var type = method.DeclaringType;

            if (type != null && method.IsSpecialName)
            {
                property = null;

                if (method.Name.StartsWith("get_"))
                {
                    property = type
                        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                        .Where(p => p.GetGetMethod() == method)
                        .FirstOrDefault();
                } else if (method.Name.StartsWith("set_"))
                {
                    property = type
                        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                        .Where(p => p.GetSetMethod() == method)
                        .FirstOrDefault();
                }

                return property != null;
            }

            property = null;
            return false;
        }

        #endregion [ TryGetDeclaringProperty ]
    }
}
