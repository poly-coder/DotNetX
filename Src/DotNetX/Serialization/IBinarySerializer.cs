using System;

namespace DotNetX.Serialization
{
    public interface IBinarySerializer
    {
        byte[] Serialize<T>(T value);
        byte[] Serialize(Type type, object value);

        T Deserialize<T>(byte[] buffer);
        object Deserialize(Type type, byte[] buffer);
    }
}
