using DotNetX.Serialization;
using System;
using System.Text.Json;

namespace DotNetX
{
    public class SystemJsonTextSerializer : ITextSerializer
    {
        public SystemJsonTextSerializer(JsonSerializerOptions options)
        {
            Options = options;
        }

        public SystemJsonTextSerializer()
        {
            Options = new();
        }

        public JsonSerializerOptions Options { get; }

        public object? Deserialize(Type type, string text)
        {
            return JsonSerializer.Deserialize(text, type, Options);
        }

        public string Serialize(Type type, object? value)
        {
            return JsonSerializer.Serialize(value, type, Options);
        }
    }
}
