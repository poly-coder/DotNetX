using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetX.OpenTelemetry
{
    public record OpenTelemetryInterceptorState(
        string ActivityName,
        ActivityKind ActivityKind,
        ActivityTagsCollection Tags,
        Func<object?, IEnumerable<KeyValuePair<string, object?>>?> GetResultTags,
        Func<Exception, IEnumerable<KeyValuePair<string, object?>>?> GetErrorTags,
        IActivity? Activity = null);
}
