using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace AzureSearchToolkit.Utilities
{
    static class ExpressionHelper
    {
        public static PropertyInfo GetPropertyInfo(Expression propertyLambda)
        {
            return GetPropertyInfo(propertyLambda as Expression<Func<object, object>>);
        }

        public static PropertyInfo GetPropertyInfo<TSource>(Expression<Func<TSource, object>> propertyLambda)
        {
            Argument.EnsureNotNull(nameof(propertyLambda), propertyLambda);

            var type = typeof(TSource);
            MemberExpression member = propertyLambda.Body as MemberExpression;

            if (member == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");
            }

            var propInfo = member.Member as PropertyInfo;

            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
            }

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a property that is not from type {type}.");
            }

            return propInfo;
        }

        public static string GetPropertyName(Expression propertyLambda)
        {
            return GetPropertyInfo(propertyLambda).Name;
        }

        public static string GetPropertyName<TSource>(Expression<Func<TSource, object>> propertyLambda)
        {
            return GetPropertyInfo(propertyLambda).Name;
        }
    }
}
