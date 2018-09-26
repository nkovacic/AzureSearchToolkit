using Microsoft.Spatial;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Json
{
    public class GeographyPointJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(GeographyPoint).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            return GeographyPoint.Create(jo.GetValue("latitude").ToObject<double>(), jo.GetValue("longitude").ToObject<double>());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
