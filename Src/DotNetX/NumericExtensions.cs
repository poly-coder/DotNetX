namespace DotNetX
{
    public static class NumericExtensions
    {
        public static bool IsBetween(this long value, long min, long max) => value >= min && value <= max;
        public static bool IsBetweenExclusive(this long value, long min, long max) => value.IsBetween(min + 1, max - 1);
        public static long Between(this long value, long min, long max) => value < min ? min : value > max ? max : value;

        public static bool IsBetween(this int value, int min, int max) => value >= min && value <= max;
        public static bool IsBetweenExclusive(this int value, int min, int max) => value.IsBetween(min + 1, max - 1);
        public static int Between(this int value, int min, int max) => value < min ? min : value > max ? max : value;
    }
}
