namespace BlockiFinAid.Helpers;

using System;
using Newtonsoft.Json;

/// <summary>
/// A custom JsonConverter for Newtonsoft.Json that safely deserializes
/// various input types into a Guid, returning Guid.Empty for invalid values.
/// </summary>
public class GuidSafeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Guid) || objectType == typeof(Guid?);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Handle null values directly.
        if (reader.TokenType == JsonToken.Null)
        {
            return Guid.Empty;
        }

        // Try to parse the string value into a Guid.
        if (reader.TokenType == JsonToken.String)
        {
            if (Guid.TryParse(reader.Value.ToString(), out Guid guid))
            {
                return guid;
            }
        }
        
        // If parsing fails for any other reason, return Guid.Empty.
        return Guid.Empty;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // For serialization, just write the Guid value.
        writer.WriteValue(value.ToString());
    }
}