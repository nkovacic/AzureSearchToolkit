using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AzureSearchToolkit.Utilities
{
    static class DictionaryHelper
    {
        public static T ToObject<T>(IDictionary<string, object> source) where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
            {
                var foundProperty = someObjectType.GetProperty(item.Key);

                if (foundProperty != null)
                {
                    foundProperty.SetValue(someObject, item.Value, null);
                }
            }

            return someObject;
        }

        public static object ToObject(IDictionary<string, object> source, Type typeToConvert)
        {
            var someObject = Activator.CreateInstance(typeToConvert);

            foreach (var item in source)
            {
                var foundProperty = typeToConvert.GetProperty(item.Key);

                if (foundProperty != null)
                {
                    foundProperty.SetValue(someObject, item.Value, null);
                }      
            }

            return someObject;
        }

        public static IDictionary<string, object> AsDictionary(object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
        }
    }
}
