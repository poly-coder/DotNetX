using System;

namespace DotNetX.OpenTelemetry
{
    public class OpenTelemetryInterceptorOptions
    {
        public Func<string, string, string> ActivityNameFormat { get; set; } =
            (typeName, methodName) => $"{typeName}.{methodName}";

        public string UnknownTypeName { get; set; } = "UnknownType";

    }
}
