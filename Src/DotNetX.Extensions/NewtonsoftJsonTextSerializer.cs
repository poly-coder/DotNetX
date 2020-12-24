using DotNetX.Serialization;
using System;
using Newtonsoft.Json;
using System.IO;

namespace DotNetX
{
    public class NewtonsoftJsonTextSerializer : ITextSerializer
    {
        public NewtonsoftJsonTextSerializer(JsonSerializer serializer)
        {
            Serializer = serializer;
        }

        public NewtonsoftJsonTextSerializer(JsonSerializerSettings settings)
        {
            Serializer = JsonSerializer.CreateDefault(settings);
        }

        public NewtonsoftJsonTextSerializer()
        {
            Serializer = JsonSerializer.CreateDefault();
        }

        public JsonSerializer Serializer { get; }

        public object? Deserialize(Type type, string text)
        {
            using var reader = new StringReader(text);
            using var jsonReader = new JsonTextReader(reader);

            return Serializer.Deserialize(jsonReader, type);
        }

        public string Serialize(Type type, object? value)
        {
            using var writer = new StringWriter();
            using var jsonWriter = new JsonTextWriter(writer);

            Serializer.Serialize(jsonWriter, value, type);

            return writer.ToString();
        }
    }
}
