using System;

namespace DotNetX.Serialization
{
    public interface ITextSerializer
    {
        string Serialize<T>(T value);
        string Serialize(Type type, object value);

        T Deserialize<T>(string text);
        object Deserialize(Type type, string text);
    }
}
