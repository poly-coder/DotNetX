using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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


        #region [ Traversals ]

        public static IEnumerable<Type> GetClassHierarchy(this Type type)
        {
            return type.Unfold(t => t.BaseType);
        }

        public static IEnumerable<Type> GetTypeHierarchy(this Type type)
        {
            return type.DepthFirstSearch(t => t.BaseType.SingletonNonNull().Concat(t.GetInterfaces()));
        }

        #endregion [ Traversals ]


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
    }
}
