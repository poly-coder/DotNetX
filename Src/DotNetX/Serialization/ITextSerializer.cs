using System;

namespace DotNetX.Serialization
{
    public interface ITextSerializer
    {
        public string Serialize<T>(T? value) => Serialize(typeof(T), value);

        string Serialize(Type type, object? value);

        T? Deserialize<T>(string text) => (T)Deserialize(typeof(T), text);

        object? Deserialize(Type type, string text);
    }
}
