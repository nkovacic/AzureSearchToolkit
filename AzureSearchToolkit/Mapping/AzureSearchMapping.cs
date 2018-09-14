﻿using AzureSearchToolkit.Request.Criteria;
using AzureSearchToolkit.Utilities;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace AzureSearchToolkit.Mapping
{
    /// <summary>
    /// A base class for mapping Elasticsearch values that can camel-case field names
    /// (and respects <see cref="SerializePropertyNamesAsCamelCase"/> to opt-in the camel-casing), 
    /// </summary>
    public class AzureSearchMapping : IAzureSearchMapping
    {
        private static Dictionary<Type, Dictionary<string, string>> mappedProperties = new Dictionary<Type, Dictionary<string, string>>();

        public JToken FormatValue(MemberInfo member, object value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string GetFieldName(Type type, MemberExpression memberExpression)
        {
            Argument.EnsureNotNull(nameof(memberExpression), memberExpression);

            switch (memberExpression.Expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                case ExpressionType.Parameter:
                    return GetFieldName(type, memberExpression.Member);

                default:
                    throw new NotSupportedException($"Unknown expression type {memberExpression.Expression.NodeType} for left hand side of expression {memberExpression}");
            }
        }

        /// <summary>
        /// Get the Elasticsearch field name for a given member.
        /// </summary>
        /// <param name="type">The prefix to put in front of this field name, if the field is
        /// an ongoing part of the document search.</param>
        /// <param name="memberInfo">The member whose field name is required.</param>
        /// <returns>The Elasticsearch field name that matches the member.</returns>
        public virtual string GetFieldName(Type type, MemberInfo memberInfo)
        {
            Argument.EnsureNotNull(nameof(type), type);
            Argument.EnsureNotNull(nameof(memberInfo), memberInfo);

            var propertyName = memberInfo.Name;

            if (!mappedProperties.ContainsKey(type))
            {
                mappedProperties.Add(type, new Dictionary<string, string>());
            }

            if (mappedProperties[type].ContainsKey(propertyName))
            {
                return mappedProperties[type][propertyName];
            }

            var camelCasePropertyAttribute = type.GetCustomAttribute<SerializePropertyNamesAsCamelCaseAttribute>(inherit: true);

            if (camelCasePropertyAttribute != null)
            {
                var camelCasePropertyName = MappingHelper.ToCamelCase(propertyName);

                mappedProperties[type].Add(propertyName, camelCasePropertyName);
            }
            else
            {
                mappedProperties[type].Add(propertyName, propertyName);
            }

            return mappedProperties[type][propertyName];
        }

        /// <inheritdoc/>
        public ICriteria GetTypeSelectionCriteria(Type type)
        {
            Argument.EnsureNotNull(nameof(type), type);

            return null;
        }
    }
}