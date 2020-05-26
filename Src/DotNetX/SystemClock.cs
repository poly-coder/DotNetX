using System;

namespace DotNetX
{
    public class SystemClock : IClock
    {
        public DateTime GetNow() => DateTime.Now;

        public DateTime GetUtcNow() => DateTime.UtcNow;
    }
}
