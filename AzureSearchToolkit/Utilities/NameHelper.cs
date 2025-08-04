using AzureSearchToolkit.Mapping;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace AzureSearchToolkit.Utilities
{
    static class NameHelper
    {
        public static string GetMemberName(MemberInfo member)
        {
            var jsonPropertyAttribute = member.GetCustomAttribute<JsonPropertyAttribute>();

            if (jsonPropertyAttribute != null)
            {
                return jsonPropertyAttribute.PropertyName;
            }

            var camelCaseAttribute = member.DeclaringType.GetCustomAttribute<SerializePropertyNamesAsCamelCaseAttribute>();

            if (camelCaseAttribute != null)
            {
                return MappingHelper.ToCamelCase(member.Name);
            }

            return member.Name;
        }
    }
}
