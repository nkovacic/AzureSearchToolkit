using Microsoft.Spatial;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureSearchToolkit.Json
{
    public class GeographyPointJsonConverter : JsonConverter<GeographyPoint>
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(GeographyPoint).IsAssignableFrom(objectType);
        }

        public override GeographyPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
            var properties = new Dictionary<string, double>(2);
            reader.Read();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                var propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "latitude":
                        properties.Add("latitude", reader.GetDouble());
                        break;
                    case "longitude":
                        properties.Add("longitude", reader.GetDouble());
                        break;
                }
            }
            return GeographyPoint.Create(properties["latitude"], properties["longitude"]);
        }

        public override void Write(Utf8JsonWriter writer, GeographyPoint value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("latitude", value.Latitude);
            writer.WriteNumber("longitude", value.Longitude);
            writer.WriteEndObject();
        }
    }
}
