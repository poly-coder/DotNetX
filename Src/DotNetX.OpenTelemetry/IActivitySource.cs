using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DotNetX.OpenTelemetry
{
    public interface IActivitySource
    {
        public string Name { get; }
        public string? Version { get; }

        public IActivity? StartActivity(
            string name,
            ActivityKind kind = ActivityKind.Internal);

        public IActivity? StartActivity(
            string name,
            ActivityKind kind,
            ActivityContext parentContext,
            IEnumerable<KeyValuePair<string, object?>>? tags = null,
            IEnumerable<ActivityLink>? links = null,
            DateTimeOffset startTime = default);

        public IActivity? StartActivity(
            string name,
            ActivityKind kind,
            string parentId,
            IEnumerable<KeyValuePair<string, object?>>? tags = null,
            IEnumerable<ActivityLink>? links = null,
            DateTimeOffset startTime = default);

        [return: NotNullIfNotNull("activity")]
        IActivity? Wrap(Activity? activity);

    }
}