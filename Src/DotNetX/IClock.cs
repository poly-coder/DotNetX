using System;

namespace DotNetX
{
    public interface IClock
    {
        DateTime GetUtcNow();
        DateTime GetNow();
    }
}
