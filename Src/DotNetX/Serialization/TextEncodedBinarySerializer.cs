using System;
using System.Text;

namespace DotNetX.Serialization
{
    public class TextEncodedBinarySerializer : IBinarySerializer
    {
        private readonly ITextSerializer textSerializer;
        private readonly Encoding encoding;

        public TextEncodedBinarySerializer(ITextSerializer textSerializer, Encoding? encoding = null)
        {
            this.textSerializer = textSerializer ?? throw new ArgumentNullException(nameof(textSerializer));
            this.encoding = encoding ?? Encoding.UTF8;
        }

        public T Deserialize<T>(byte[] buffer)
        {
            return (T)Deserialize(typeof(T), buffer);
        }

        public object Deserialize(Type type, byte[] buffer)
        {
            var text = encoding.GetString(buffer);
            return textSerializer.Deserialize(type, text);
        }

        public byte[] Serialize<T>(T value)
        {
            return Serialize(typeof(T), value!);
        }

        public byte[] Serialize(Type type, object value)
        {
            var text = textSerializer.Serialize(type, value);
            return encoding.GetBytes(text);
        }
    }
}
