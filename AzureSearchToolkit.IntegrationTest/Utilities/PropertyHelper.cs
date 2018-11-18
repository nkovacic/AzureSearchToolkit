using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest.Utilities
{
    class PropertyHelper
    {
        public static bool HasProperty<TSource>(string propertyName)
        {
            var type = typeof(TSource);

            return type.GetProperties().Any(q => q.Name == propertyName);
        }

        public static string GetPropertyName<TSource, TKey>(Expression<Func<TSource, TKey>> propertyLambda)
        {
            var propertyInfo = GetPropertyInfo(propertyLambda);

            if (propertyInfo != null)
            {
                return propertyInfo.Name;
            }

            return string.Empty;
        }

        public static PropertyInfo GetPropertyInfo<TSource, TKey>(Expression<Func<TSource, TKey>> propertyLambda)
        {
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
    }
}
