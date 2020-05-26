using System;

namespace DotNetX
{
    public class GuidIdentifierGenerator : IIdentifierGenerator
    {
        public string Format { get; }

        public GuidIdentifierGenerator(string format = "N")
        {
            Format = format;
        }

        public string NewId() => Guid.NewGuid().ToString(Format);
    }
}
