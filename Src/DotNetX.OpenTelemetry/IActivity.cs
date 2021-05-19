using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetX.OpenTelemetry
{
    public interface IActivity
    {
        DateTime StartTimeUtc { get; }
        string? Id { get; }
        string? ParentId { get; }
        string? RootId { get; }
        IEnumerable<KeyValuePair<string, string?>> Tags { get; }
        IEnumerable<KeyValuePair<string, object?>> TagObjects { get; }
        IEnumerable<ActivityEvent> Events { get; }
        IEnumerable<ActivityLink> Links { get; }
        IEnumerable<KeyValuePair<string, string?>> Baggage { get; }
        ActivityContext Context { get; }
        string? TraceStateString { get; set; }
        ActivitySpanId SpanId { get; }
        ActivityTraceId TraceId { get; }
        bool Recorded { get; }
        bool IsAllDataRequested { get; set; }
        ActivityTraceFlags ActivityTraceFlags { get; set; }
        ActivitySpanId ParentSpanId { get; }
        TimeSpan Duration { get; }
        IActivity? Parent { get; }
        IActivitySource Source { get; }
        string DisplayName { get; set; }
        string OperationName { get; }
        ActivityKind Kind { get; }
        ActivityIdFormat IdFormat { get; }

        IActivity AddBaggage(string key, string? value);
        IActivity AddEvent(ActivityEvent e);
        IActivity AddTag(string key, string? value);
        IActivity AddTag(string key, object? value);
        string? GetBaggageItem(string key);
        object? GetCustomProperty(string propertyName);
        void SetCustomProperty(string propertyName, object? propertyValue);
        IActivity SetEndTime(DateTime endTimeUtc);
        IActivity SetIdFormat(ActivityIdFormat format);
        IActivity SetParentId(ActivityTraceId traceId, ActivitySpanId spanId, ActivityTraceFlags activityTraceFlags = ActivityTraceFlags.None);
        IActivity SetParentId(string parentId);
        IActivity SetStartTime(DateTime startTimeUtc);
        IActivity SetTag(string key, object? value);
        IActivity Start();
        void Stop();

    }
}
