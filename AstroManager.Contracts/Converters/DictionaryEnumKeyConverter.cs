using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Model.Converters
{
    /// <summary>
    /// JSON converter for Dictionary with enum keys - handles string serialization/deserialization of enum keys
    /// </summary>
    public class DictionaryEnumKeyConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>> where TKey : struct, Enum
    {
        public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dictionary = new Dictionary<TKey, TValue>();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token");
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected PropertyName token");
                }

                var keyString = reader.GetString();
                if (!Enum.TryParse<TKey>(keyString, ignoreCase: true, out var key))
                {
                    throw new JsonException($"Unable to parse enum key: {keyString}");
                }

                reader.Read();
                var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                dictionary[key] = value!;
            }

            throw new JsonException("Expected EndObject token");
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key.ToString());
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}
