using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DotNetX.Reactive
{
    public static class ReflectionExtensions
    {
        public static PropertyInfo GetPropertyInfo<T, R>(this Expression<Func<T, R>> propertyExpression)
        {
            var lambda = (LambdaExpression)propertyExpression;

            switch (lambda.Body)
            {
                case UnaryExpression unary:
                    return (PropertyInfo)((MemberExpression)unary.Operand).Member;

                case MemberExpression member:
                    return (PropertyInfo)member.Member;

                default:
                    throw new ArgumentException("Expected a property expression.", nameof(propertyExpression));
            }
        }

        public static Type AsTypeOrNull(this string typeName)
        {
            try
            {
                return typeName != null ? Type.GetType(typeName) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string PrintPropertyValues(this object instance)
        {
            var sb = new StringBuilder();
            var type = instance.GetType();
            sb.Append($"{type.Name}(").AppendLine();
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                sb.Append($"{prop.Name.Shorten(20, "...")} = ");
                var value = prop.GetValue(instance);
                if (value == null)
                {
                    sb.Append("null");
                }
                else
                {
                    sb.Append(value);
                }
                sb.AppendLine();
            }
            sb.Append(")");
            return sb.ToString();
        }
    }

}
