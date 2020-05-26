using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNetX.Reflection
{
    public static class ReflectionExtensions
    {
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
                var elementType = type.GetElementType().FormatName(fullName);
                var rank = type.GetArrayRank();
                var commas = new String(',', rank - 1);
                return $"{elementType}[{commas}]";
            }

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                return "Its a delegate: " + type.Name;
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
            return typeName;
        }

        public static IEnumerable<MethodInfo> SelectMethods(this Type type,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance,
            Func<string, bool> isValidMethodName = null,
            Func<ParameterInfo[], bool> isValidInputType = null,
            Func<ParameterInfo, bool> isValidReturnType = null)
        {
            IEnumerable<MethodInfo> query = type.GetMethods(bindingFlags);
            query = isValidMethodName == null ? query : query.Where(method => isValidMethodName(method.Name));
            query = isValidReturnType == null ? query : query.Where(method => isValidReturnType(method.ReturnParameter));
            query = isValidInputType == null ? query : query.Where(method => isValidInputType(method.GetParameters()));
            return query;
        }

        public static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider provider, Type attributeType, bool inherit)
        {
            return provider.GetCustomAttributes(attributeType, inherit).Cast<Attribute>();
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ICustomAttributeProvider provider, bool inherit)
            where TAttribute : Attribute
        {
            return provider.GetAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }

        public static Attribute GetAttribute(this ICustomAttributeProvider provider, Type attributeType, bool inherit)
        {
            return provider.GetAttributes(attributeType, inherit).FirstOrDefault();
        }

        public static TAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider provider, bool inherit)
            where TAttribute : Attribute
        {
            return provider.GetAttributes<TAttribute>(inherit).FirstOrDefault();
        }

        public static TValue GetConventionValue<TProvider, TAttribute, TValue>(
            this TProvider provider, 
            bool inherit, 
            Func<TProvider, TValue> ofConvention,
            Func<TAttribute, TValue> ofAttribute)
            where TProvider : ICustomAttributeProvider
            where TAttribute : Attribute
        {
            var attr = provider.GetAttribute<TAttribute>(inherit);
            return attr == null ? ofConvention(provider) : ofAttribute(attr);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="TGenericContainer"></typeparam>
        ///// <param name="type"></param>
        ///// <returns></returns>
        ///// <example>
        ///// typeof(List<)
        ///// </example>
        //public static Type IsAssignableFrom<TGenericContainer>(this Type type)
        //{
        //    return null;
        //}
    }
}
