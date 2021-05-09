using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace DotNetX.Logging
{
    public record LoggingInterceptorState(
        Stopwatch Watch,
        ILogger Logger,
        string TypeName,
        object? Parameters,
        Func<object?, object?> GetResult);
}
