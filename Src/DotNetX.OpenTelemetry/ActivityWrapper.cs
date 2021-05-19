using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetX.OpenTelemetry
{
    public class ActivityWrapper : IActivity
    {
        private readonly Activity activity;
        private readonly IActivitySource activitySource;

        public ActivityWrapper(Activity activity, IActivitySource activitySource)
        {
            this.activity = activity ?? throw new ArgumentNullException(nameof(activity));
            this.activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
        }

        public DateTime StartTimeUtc => activity.StartTimeUtc;

        public string? Id => activity.Id;

        public string? ParentId => activity.ParentId;

        public string? RootId => activity.RootId;

        public IEnumerable<KeyValuePair<string, string?>> Tags => activity.Tags;

        public IEnumerable<KeyValuePair<string, object?>> TagObjects => activity.TagObjects;

        public IEnumerable<ActivityEvent> Events => activity.Events;

        public IEnumerable<ActivityLink> Links => activity.Links;

        public IEnumerable<KeyValuePair<string, string?>> Baggage => activity.Baggage;

        public ActivityContext Context => activity.Context;

        public string? TraceStateString { get => activity.TraceStateString; set => activity.TraceStateString = value; }

        public ActivitySpanId SpanId => activity.SpanId;

        public ActivityTraceId TraceId => activity.TraceId;

        public bool Recorded => activity.Recorded;

        public bool IsAllDataRequested { get => activity.IsAllDataRequested; set => activity.IsAllDataRequested = value; }

        public ActivityTraceFlags ActivityTraceFlags { get => activity.ActivityTraceFlags; set => activity.ActivityTraceFlags = value; }

        public ActivitySpanId ParentSpanId => activity.ParentSpanId;

        public TimeSpan Duration => activity.Duration;

        private IActivity? parent;
        private bool isParentSet;
        public IActivity? Parent
        {
            get
            {
                if (!isParentSet)
                {
                    isParentSet = true;
                    parent = activitySource.Wrap(activity.Parent);
                }
                return parent;
            }
        }

        public IActivitySource Source => activitySource;

        public string DisplayName { get => activity.DisplayName; set => activity.DisplayName = value; }

        public string OperationName => activity.OperationName;

        public ActivityKind Kind => activity.Kind;

        public ActivityIdFormat IdFormat => activity.IdFormat;

        public IActivity AddBaggage(string key, string? value)
        {
            activity.AddBaggage(key, value);
            return this;
        }

        public IActivity AddEvent(ActivityEvent e)
        {
            activity.AddEvent(e);
            return this;
        }

        public IActivity AddTag(string key, string? value)
        {
            activity.AddTag(key, value);
            return this;
        }

        public IActivity AddTag(string key, object? value)
        {
            activity.AddTag(key, value);
            return this;
        }

        public string? GetBaggageItem(string key)
        {
            return activity.GetBaggageItem(key);
        }

        public object? GetCustomProperty(string propertyName)
        {
            return activity.GetCustomProperty(propertyName);
        }

        public void SetCustomProperty(string propertyName, object? propertyValue)
        {
            activity.SetCustomProperty(propertyName, propertyValue);
        }

        public IActivity SetEndTime(DateTime endTimeUtc)
        {
            activity.SetEndTime(endTimeUtc);
            return this;
        }

        public IActivity SetIdFormat(ActivityIdFormat format)
        {
            activity.SetIdFormat(format);
            return this;
        }

        public IActivity SetParentId(ActivityTraceId traceId, ActivitySpanId spanId, ActivityTraceFlags activityTraceFlags = ActivityTraceFlags.None)
        {
            activity.SetParentId(traceId, spanId, activityTraceFlags);
            return this;
        }

        public IActivity SetParentId(string parentId)
        {
            activity.SetParentId(parentId);
            return this;
        }

        public IActivity SetStartTime(DateTime startTimeUtc)
        {
            activity.SetStartTime(startTimeUtc);
            return this;
        }

        public IActivity SetTag(string key, object? value)
        {
            activity.SetTag(key, value);
            return this;
        }

        public IActivity Start()
        {
            activity.Start();
            return this;
        }

        public void Stop()
        {
            activity.Stop();
        }
    }
}