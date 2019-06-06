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
            if (reader.TokenType != JsonToken.Null)
            {
                var jo = JObject.Load(reader);

                var latitudeProperty = jo.GetValue("latitude");
                var longitudeProperty = jo.GetValue("longitude");

                if (latitudeProperty != null && longitudeProperty != null)
                {
                    return GeographyPoint.Create(latitudeProperty.ToObject<double>(), longitudeProperty.ToObject<double>());
                }
            }
            
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is GeographyPoint geographyPoint)
            {
                var geographyPointJson = new JObject
                {
                    { "latitude", geographyPoint.Latitude },
                    { "longitude", geographyPoint.Longitude }
                };

                geographyPointJson.WriteTo(writer);
            }
        }
    }
}
