using AzureSearchToolkit.Json;
using AzureSearchToolkit.Request.Criteria;
using AzureSearchToolkit.Utilities;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static Dictionary<Type, Dictionary<string, string>> mappedPropertiesCache = new Dictionary<Type, Dictionary<string, string>>();
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new GeographyPointJsonConverter() },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None
        };
        private static JsonSerializer jsonSerializer = JsonSerializer.Create(jsonSettings);

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
        /// Get the AzureSearch field name for a given member.
        /// </summary>
        /// <param name="type">The prefix to put in front of this field name, if the field is
        /// an ongoing part of the document search.</param>
        /// <param name="memberInfo">The member whose field name is required.</param>
        /// <returns>The AzureSearch field name that matches the member.</returns>
        public virtual string GetFieldName(Type type, MemberInfo memberInfo)
        {
            Argument.EnsureNotNull(nameof(type), type);
            Argument.EnsureNotNull(nameof(memberInfo), memberInfo);

            var propertyName = memberInfo.Name;

            var mappedProperties = GetMappedPropertiesForType(type);

            var mappedProperty = mappedProperties.FirstOrDefault(q => q.Value == propertyName);

            if (!string.IsNullOrWhiteSpace(mappedProperty.Value))
            {
                return mappedProperty.Key;
            }
            else
            {
                throw new KeyNotFoundException($"Property {propertyName} was not found on {type}.");
            }
        }

        /// <inheritdoc/>
        public ICriteria GetTypeSelectionCriteria(Type type)
        {
            Argument.EnsureNotNull(nameof(type), type);

            return null;
        }

        public object Materialize(Document sourceDocument, Type sourceType)
        {
            return JObject.FromObject(sourceDocument, jsonSerializer).ToObject(sourceType, jsonSerializer);
        }

        private Dictionary<string, string> GetMappedPropertiesForType(Type sourceType)
        {
            if (!mappedPropertiesCache.ContainsKey(sourceType))
            {
                mappedPropertiesCache.Add(sourceType, new Dictionary<string, string>());

                var camelCasePropertyAttribute = sourceType.GetCustomAttribute<SerializePropertyNamesAsCamelCaseAttribute>(inherit: true);

                foreach (var property in sourceType.GetProperties())
                {
                    var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                    if(jsonPropertyAttribute != null)
                    {
                        mappedPropertiesCache[sourceType].Add(jsonPropertyAttribute.PropertyName, property.Name);
                    }
                    else if (camelCasePropertyAttribute != null)
                    {
                        var camelCasePropertyName = MappingHelper.ToCamelCase(property.Name);

                        mappedPropertiesCache[sourceType].Add(camelCasePropertyName, property.Name);
                    }
                    else
                    {
                        mappedPropertiesCache[sourceType].Add(property.Name, property.Name);
                    }
                }
            }

            return mappedPropertiesCache[sourceType];
        }
    }
}
