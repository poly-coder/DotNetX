using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DotNetX.OpenTelemetry
{
    public class ActivitySourceWrapper : IActivitySource
    {
        private readonly ActivitySource activitySource;

        public ActivitySourceWrapper(ActivitySource activitySource)
        {
            this.activitySource = activitySource;
        }

        public string Name => activitySource.Name;

        public string? Version => activitySource.Version;

        public IActivity? StartActivity(
            string name,
            ActivityKind kind = ActivityKind.Internal) =>
            Wrap(activitySource.StartActivity(name, kind));

        public IActivity? StartActivity(
            string name,
            ActivityKind kind,
            ActivityContext parentContext,
            IEnumerable<KeyValuePair<string, object?>>? tags = null,
            IEnumerable<ActivityLink>? links = null,
            DateTimeOffset startTime = default) =>
            Wrap(activitySource.StartActivity(name, kind, parentContext, tags, links, startTime));

        public IActivity? StartActivity(
            string name,
            ActivityKind kind,
            string parentId,
            IEnumerable<KeyValuePair<string, object?>>? tags = null,
            IEnumerable<ActivityLink>? links = null,
            DateTimeOffset startTime = default) =>
            Wrap(activitySource.StartActivity(name, kind, parentId, tags, links, startTime));

        [return: NotNullIfNotNull("activity")]
        public IActivity? Wrap(Activity? activity)
        {
            if (activity != null)
            {
                return new ActivityWrapper(activity, this);
            }

            return null;
        }
    }
}