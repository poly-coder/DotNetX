using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetX.Reflection
{
    public static class ExpressionExtensions
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
                    throw new ArgumentException(Resource.Error_ExpectedPropertyExpression, nameof(propertyExpression));
            }
        }
    }
}
