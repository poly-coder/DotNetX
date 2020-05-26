namespace DotNetX
{
    public class HexIdentifierGenerator : IIdentifierGenerator
    {
        public int Length { get; }

        public HexIdentifierGenerator(int length = 12)
        {
            Length = length;
        }

        public string NewId() => Length.GetRandomHexString();
    }
}
