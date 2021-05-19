using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetX.Reflection;

namespace DotNetX.OpenTelemetry
{
    public class OpenTelemetryInterceptorBuilder
    {
        private bool built;

        #region [ ActivitySource ]

        private IActivitySource activitySource;

        public OpenTelemetryInterceptorBuilder WithActivitySource(IActivitySource activitySource)
        {
            if (activitySource is null)
            {
                throw new ArgumentNullException(nameof(activitySource));
            }

            CheckNotBuilt();
            this.activitySource = activitySource;
            return this;
        }

        #endregion [ ActivitySource ]

        #region [ ActivityKind ]

        private ActivityKind activityKind = ActivityKind.Internal;

        public OpenTelemetryInterceptorBuilder WithActivityKind(ActivityKind activityKind)
        {
            CheckNotBuilt();
            this.activityKind = activityKind;
            return this;
        }

        #endregion [ ActivityKind ]

        #region [ TypeName ]

        private string? typeName;

        public OpenTelemetryInterceptorBuilder WithTypeName(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));

            CheckNotBuilt();
            this.typeName = typeName;
            return this;
        }

        #endregion [ ActivityKind ]

        #region [ Options ]

        private OpenTelemetryInterceptorOptions options = new OpenTelemetryInterceptorOptions();

        public OpenTelemetryInterceptorBuilder WithOptions(OpenTelemetryInterceptorOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            CheckNotBuilt();
            this.options = options;
            return this;
        }

        #endregion [ Options ]

        #region [ Flags ]

        private bool interceptAsync = true;
        private bool interceptProperties = false;

        public OpenTelemetryInterceptorBuilder DoNotInterceptAsync()
        {
            CheckNotBuilt();
            interceptAsync = false;
            return this;
        }

        public OpenTelemetryInterceptorBuilder InterceptAsync()
        {
            CheckNotBuilt();
            interceptAsync = true;
            return this;
        }

        public OpenTelemetryInterceptorBuilder DoNotInterceptProperties()
        {
            CheckNotBuilt();
            interceptProperties = false;
            return this;
        }

        public OpenTelemetryInterceptorBuilder InterceptProperties()
        {
            CheckNotBuilt();
            interceptProperties = true;
            return this;
        }

        #endregion [ Flags ]

        #region [ Filters ]

        private bool includeByDefault = true;
        private List<Func<MethodInfo, bool>> methodsPredicates = new List<Func<MethodInfo, bool>>();

        public OpenTelemetryInterceptorBuilder DoNotInterceptIf(Func<MethodInfo, bool> predicate)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            CheckNotBuilt();

            if (!includeByDefault && methodsPredicates.Any())
            {
                throw new InvalidOperationException("Cannot use DoNotIntercept methods after using DoIntercept");
            }

            includeByDefault = true;
            methodsPredicates.Add(predicate);
            return this;
        }

        public OpenTelemetryInterceptorBuilder DoNotInterceptIfNamed(string methodName)
        {
            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            return DoNotInterceptIf(method => method.Name == methodName);
        }

        public OpenTelemetryInterceptorBuilder DoNotInterceptIfMatches(Regex pattern)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return DoNotInterceptIf(method => pattern.IsMatch(method.Name));
        }

        public OpenTelemetryInterceptorBuilder DoNotInterceptIfMatches(string pattern)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return DoNotInterceptIf(method => Regex.IsMatch(method.Name, pattern));
        }

        public OpenTelemetryInterceptorBuilder DoInterceptIf(Func<MethodInfo, bool> predicate)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            CheckNotBuilt();

            if (includeByDefault && methodsPredicates.Any())
            {
                throw new InvalidOperationException("Cannot use DoIntercept methods after using DoNotIntercept");
            }

            includeByDefault = false;
            methodsPredicates.Add(predicate);
            return this;
        }

        public OpenTelemetryInterceptorBuilder DoInterceptIfNamed(string methodName)
        {
            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            return DoInterceptIf(method => method.Name == methodName);
        }

        public OpenTelemetryInterceptorBuilder DoInterceptIfMatches(Regex pattern)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return DoInterceptIf(method => pattern.IsMatch(method.Name));
        }

        public OpenTelemetryInterceptorBuilder DoInterceptIfMatches(string pattern)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return DoInterceptIf(method => Regex.IsMatch(method.Name, pattern));
        }

        #endregion [ Filters ]

        #region [ Predicates and Extractors ]

        private static Func<MethodInfo, Type, string, bool> WhenInternal(string? methodName, Type? type, string? name) =>
            (method, parameterType, parameterName) =>
                (methodName == null || method.Name   == methodName) &&
                (type       == null || parameterType == type) &&
                (name       == null || parameterName == name);

        public static Func<MethodInfo, Type, string, bool> When(string methodName, Type type, string name)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentException($"'{nameof(methodName)}' cannot be null or empty.", nameof(methodName));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            return WhenInternal(methodName, type, name);
        }

        public static Func<MethodInfo, Type, string, bool> When<T>(string methodName, string name) =>
            When(methodName, typeof(T), name);

        public static Func<MethodInfo, Type, string, bool> When(Type type, string name)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            return WhenInternal(null, type, name);
        }

        public static Func<MethodInfo, Type, string, bool> When(string methodName, string name)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentException($"'{nameof(methodName)}' cannot be null or empty.", nameof(methodName));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            return WhenInternal(methodName, null, name);
        }

        public static Func<MethodInfo, Type, string, bool> When<T>(string name) =>
            When(typeof(T), name);

        public static Func<MethodInfo, Type, string, bool> When(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            return WhenInternal(null, null, name);
        }

        public static Func<MethodInfo, Type, string, bool> When(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return WhenInternal(null, type, null);
        }

        public static Func<MethodInfo, Type, string, bool> When<T>() => When(typeof(T));


        private static Func<MethodInfo, Type, bool> WhenReturnsInternal(string? methodName, Type? type) =>
            (method, resultType) =>
                (methodName == null || method.Name   == methodName) &&
                (type       == null || resultType == type);

        public static Func<MethodInfo, Type, bool> WhenReturns(string methodName, Type type)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentException($"'{nameof(methodName)}' cannot be null or empty.", nameof(methodName));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return WhenReturnsInternal(methodName, type);
        }

        public static Func<MethodInfo, Type, bool> WhenReturns<T>(string methodName) =>
            WhenReturns(methodName, typeof(T));

        public static Func<MethodInfo, Type, bool> WhenReturns(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return WhenReturnsInternal(null, type);
        }

        public static Func<MethodInfo, Type, bool> WhenReturns<T>() =>
            WhenReturns(typeof(T));

        public static Func<MethodInfo, Type, bool> WhenReturns(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentException($"'{nameof(methodName)}' cannot be null or empty.", nameof(methodName));
            }

            return WhenReturnsInternal(methodName, null);
        }


        private static Func<MethodInfo, Type, bool> WhenThrowsInternal(string? methodName, Type? type) =>
            (method, resultType) =>
                (methodName == null || method.Name   == methodName) &&
                (type       == null || resultType == type);

        public static Func<MethodInfo, Type, bool> WhenThrows(string methodName, Type type)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentException($"'{nameof(methodName)}' cannot be null or empty.", nameof(methodName));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return WhenThrowsInternal(methodName, type);
        }

        public static Func<MethodInfo, Type, bool> WhenThrows<T>(string methodName) where T : Exception =>
            WhenThrows(methodName, typeof(T));

        public static Func<MethodInfo, Type, bool> WhenThrows(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return WhenThrowsInternal(null, type);
        }

        public static Func<MethodInfo, Type, bool> WhenThrows<T>() where T : Exception =>
            WhenThrows(typeof(T));

        public static Func<MethodInfo, Type, bool> WhenThrows(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentException($"'{nameof(methodName)}' cannot be null or empty.", nameof(methodName));
            }

            return WhenThrowsInternal(methodName, null);
        }


        public static Func<object?, object?> ExtractAs(string outputName)
        {
            return obj => new[] {KeyValuePair.Create(outputName, obj)};
        }

        #endregion [ Predicates and Extractors ]

        #region [ CommonTags ]

        private List<KeyValuePair<string, object?>> commonTags = new();

        public OpenTelemetryInterceptorBuilder TagWith(
            string name,
            object? value)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            CheckNotBuilt();
            this.commonTags.Add(KeyValuePair.Create(name, value));
            return this;
        }


        #endregion [ CommonTags ]

        #region [ TargetTags ]

        private Func<object, object?>? tagTarget;

        record TargetExtractor(
            Func<Type, bool> Predicate,
            Func<object, object?> Extract);

        private List<TargetExtractor> targetExtractors = new();

        public OpenTelemetryInterceptorBuilder TagTarget(Func<object, object?> tagTarget)
        {
            if (tagTarget is null)
            {
                throw new ArgumentNullException(nameof(tagTarget));
            }

            CheckNotBuilt();
            this.tagTarget = tagTarget;
            return this;
        }

        public OpenTelemetryInterceptorBuilder TagTarget(
            Func<Type, bool> predicate,
            Func<object, object?> extract)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            CheckNotBuilt();
            this.targetExtractors.Add(new TargetExtractor(predicate, extract));
            return this;
        }

        public OpenTelemetryInterceptorBuilder TagTarget(
            Type type,
            Func<object, object?> extract) =>
            TagTarget(
                actualType => type.IsAssignableFrom(actualType),
                extract);

        public OpenTelemetryInterceptorBuilder TagTarget<T>(
            Func<T, object?> extract) =>
            TagTarget(
                typeof(T),
                obj => obj is T t ? extract(t) : null);

        #endregion [ TargetTags ]

        #region [ TagParameters ]

        private Func<MethodInfo, object?[]?, object?>? tagParameters;

        record ParametersExtractor(
            Func<MethodInfo, Type, string, bool> Predicate,
            Func<object?, object?> Extract);

        private List<ParametersExtractor> parametersExtractors = new();

        public OpenTelemetryInterceptorBuilder TagMethodParameters(Func<MethodInfo, object?[]?, object?> tagParameters)
        {
            if (tagParameters is null)
            {
                throw new ArgumentNullException(nameof(tagParameters));
            }

            CheckNotBuilt();
            this.tagParameters = tagParameters;
            return this;
        }

        public OpenTelemetryInterceptorBuilder TagParameter(
            Func<MethodInfo, Type, string, bool> predicate,
            Func<object?, object?> extract)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            CheckNotBuilt();
            this.parametersExtractors.Add(new ParametersExtractor(predicate, extract));
            return this;
        }

        public OpenTelemetryInterceptorBuilder TagParameter(
            string methodName,
            Type type,
            string name,
            Func<object?, object?> extract) =>
            TagParameter(
                When(methodName, type, name),
                extract);

        public OpenTelemetryInterceptorBuilder TagParameter<T>(
            string methodName,
            string name,
            Func<T, object?> extract) =>
            TagParameter(
                When<T>(methodName, name),
                obj => obj is T t ? extract(t) : null);

        public OpenTelemetryInterceptorBuilder TagParameter(
            Type type,
            string name,
            Func<object?, object?> extract) =>
            TagParameter(
                When(type, name),
                extract);

        public OpenTelemetryInterceptorBuilder TagParameter<T>(
            string name,
            Func<T, object?> extract) =>
            TagParameter(
                When<T>(name),
                obj => obj is T t ? extract(t) : null);

        public OpenTelemetryInterceptorBuilder TagParameter(
            string methodName,
            string name,
            Func<object?, object?> extract) =>
            TagParameter(
                When(methodName, name),
                extract);

        public OpenTelemetryInterceptorBuilder TagParameter(
            Type type,
            Func<object?, object?> extract) =>
            TagParameter(
                When(type),
                extract);

        public OpenTelemetryInterceptorBuilder TagParameter<T>(
            Func<T, object?> extract) =>
            TagParameter(
                When<T>(),
                obj => obj is T t ? extract(t) : null);

        public OpenTelemetryInterceptorBuilder TagParameter(
            string methodName,
            Type type,
            string name,
            string? outputName = null) =>
            TagParameter(
                When(methodName, type, name),
                ExtractAs(outputName ?? name));

        public OpenTelemetryInterceptorBuilder TagParameter<T>(
            string methodName,
            string name,
            string? outputName = null) =>
            TagParameter(
                When<T>(methodName, name),
                ExtractAs(outputName ?? name));

        public OpenTelemetryInterceptorBuilder TagParameter(
            string methodName,
            string name,
            string? outputName = null) =>
            TagParameter(
                When(methodName, name),
                ExtractAs(outputName ?? name));

        public OpenTelemetryInterceptorBuilder TagParameter(
            Type type,
            string name,
            string? outputName = null) =>
            TagParameter(
                When(type, name),
                ExtractAs(outputName ?? name));


        public OpenTelemetryInterceptorBuilder TagParameter(
            string name,
            string? outputName = null) =>
            TagParameter(
                When(name),
                ExtractAs(outputName ?? name));


        public OpenTelemetryInterceptorBuilder TagParameter<T>(
            string name,
            string? outputName = null) =>
            TagParameter(
                When<T>(name),
                ExtractAs(outputName ?? name));


        #endregion [ TagParameters ]

        #region [ TagResult ]

        private Func<MethodInfo, object?, object?>? tagResult;

        record ResultExtractor(
            Func<MethodInfo, Type, bool> Predicate,
            Func<object?, object?> Extract);

        private List<ResultExtractor> resultExtractors = new List<ResultExtractor>();

        public OpenTelemetryInterceptorBuilder TagMethodResult(Func<MethodInfo, object?, object?> tagResult)
        {
            if (tagResult is null)
            {
                throw new ArgumentNullException(nameof(tagResult));
            }

            CheckNotBuilt();
            this.tagResult = tagResult;
            return this;
        }

        public OpenTelemetryInterceptorBuilder TagResult(
            Func<MethodInfo, Type, bool> predicate,
            Func<object?, object?> extract)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            CheckNotBuilt();
            this.resultExtractors.Add(new ResultExtractor(predicate, extract));
            return this;
        }

        public OpenTelemetryInterceptorBuilder TagResult(
            string methodName,
            Type type,
            Func<object?, object?> extract) =>
            TagResult(
                WhenReturns(methodName, type),
                extract);

        public OpenTelemetryInterceptorBuilder TagResult<T>(
            string methodName,
            Func<T, object?> extract) =>
            TagResult(
                WhenReturns<T>(methodName),
                obj => obj is T t ? extract(t) : null);

        public OpenTelemetryInterceptorBuilder TagResult(
            Type type,
            Func<object?, object?> extract) =>
            TagResult(
                WhenReturns(type),
                extract);

        public OpenTelemetryInterceptorBuilder TagResult<T>(
            Func<T, object?> extract) =>
            TagResult(
                WhenReturns<T>(),
                obj => obj is T t ? extract(t) : null);

        public OpenTelemetryInterceptorBuilder TagResult(
            string methodName,
            Func<object?, object?> extract) =>
            TagResult(
                WhenReturns(methodName),
                extract);
        
        public OpenTelemetryInterceptorBuilder TagResult(
            string methodName,
            Type type,
            string outputName) =>
            TagResult(
                WhenReturns(methodName, type),
                ExtractAs(outputName));

        public OpenTelemetryInterceptorBuilder TagResult<T>(
            string methodName,
            string outputName) =>
            TagResult(
                WhenReturns<T>(methodName),
                ExtractAs(outputName));

        public OpenTelemetryInterceptorBuilder TagResult(
            string methodName,
            string outputName) =>
            TagResult(
                WhenReturns(methodName),
                ExtractAs(outputName));

        public OpenTelemetryInterceptorBuilder TagResult(
            Type type,
            string outputName) =>
            TagResult(
                WhenReturns(type),
                ExtractAs(outputName));


        public OpenTelemetryInterceptorBuilder TagResult(
            string outputName) =>
            TagResult(
                (_, _) => true,
                ExtractAs(outputName));


        public OpenTelemetryInterceptorBuilder TagResult<T>(
            string outputName) =>
            TagResult(
                WhenReturns<T>(),
                ExtractAs(outputName));


        #endregion [ TagResult ]

        #region [ TagError ]

        private Func<MethodInfo, Exception, object?>? tagError;

        record ErrorExtractor(
            Func<MethodInfo, Type, bool> Predicate,
            Func<Exception, object?> Extract);

        private List<ErrorExtractor> errorExtractors = new();

        public OpenTelemetryInterceptorBuilder TagMethodError(Func<MethodInfo, object?, object?> tagError)
        {
            if (tagError is null)
            {
                throw new ArgumentNullException(nameof(tagError));
            }

            CheckNotBuilt();
            this.tagError = tagError;
            return this;
        }

        public OpenTelemetryInterceptorBuilder TagError(
            Func<MethodInfo, Type, bool> predicate,
            Func<object?, object?> extract)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            CheckNotBuilt();
            this.errorExtractors.Add(new ErrorExtractor(predicate, extract));
            return this;
        }

        public OpenTelemetryInterceptorBuilder TagError(
            string methodName,
            Type type,
            Func<object?, object?> extract) =>
            TagError(
                WhenThrows(methodName, type),
                extract);

        public OpenTelemetryInterceptorBuilder TagError<T>(
            string methodName,
            Func<T, object?> extract) where T : Exception =>
            TagError(
                WhenThrows<T>(methodName),
                obj => obj is T t ? extract(t) : null);

        public OpenTelemetryInterceptorBuilder TagError(
            Type type,
            Func<object?, object?> extract) =>
            TagError(
                WhenThrows(type),
                extract);

        public OpenTelemetryInterceptorBuilder TagError<T>(
            Func<T, object?> extract) where T : Exception =>
            TagError(
                WhenThrows<T>(),
                obj => obj is T t ? extract(t) : null);

        public OpenTelemetryInterceptorBuilder TagError(
            string methodName,
            Func<object?, object?> extract) =>
            TagError(
                WhenThrows(methodName),
                extract);
        
        public OpenTelemetryInterceptorBuilder TagError(
            string methodName,
            Type type,
            string outputName) =>
            TagError(
                WhenThrows(methodName, type),
                ExtractAs(outputName));

        public OpenTelemetryInterceptorBuilder TagError<T>(
            string methodName,
            string outputName) where T : Exception =>
            TagError(
                WhenThrows<T>(methodName),
                ExtractAs(outputName));

        public OpenTelemetryInterceptorBuilder TagError(
            string methodName,
            string outputName) =>
            TagError(
                WhenThrows(methodName),
                ExtractAs(outputName));

        public OpenTelemetryInterceptorBuilder TagError(
            Type type,
            string outputName) =>
            TagError(
                WhenThrows(type),
                ExtractAs(outputName));


        public OpenTelemetryInterceptorBuilder TagError(
            string outputName) =>
            TagError(
                (_, _) => true,
                ExtractAs(outputName));


        public OpenTelemetryInterceptorBuilder TagError<T>(
            string outputName) where T : Exception =>
            TagError(
                WhenThrows<T>(),
                ExtractAs(outputName));


        #endregion [ TagError ]

        public OpenTelemetryInterceptor Build()
        {
            CheckNotBuilt();

            built = true;

            return new OpenTelemetryInterceptor(new InterceptorSetup(this));
        }

        private void CheckNotBuilt()
        {
            if (built)
            {
                throw new InvalidOperationException("OpenTelemetryInterceptor is already built");
            }
        }

        class InterceptorSetup : IOpenTelemetryInterceptorSetup
        {
            private readonly IActivitySource activitySource;
            private readonly bool interceptAsync;
            private readonly bool interceptProperties;
            private readonly OpenTelemetryInterceptorOptions options;

            private readonly ActivityKind activityKind;
            private readonly string? typeName;

            private readonly bool includeByDefault;
            private readonly ReadOnlyCollection<Func<MethodInfo, bool>> methodsPredicates;

            private readonly ReadOnlyCollection<KeyValuePair<string, object?>> commonTags;

            private readonly Func<object, MethodInfo, object?[]?, ActivityTagsCollection> getBeforeTags;
            private readonly Func<MethodInfo, object?, IEnumerable<KeyValuePair<string, object?>>> getResultTags;
            private readonly Func<MethodInfo, Exception, IEnumerable<KeyValuePair<string, object?>>> getErrorTags;

            public InterceptorSetup(
                OpenTelemetryInterceptorBuilder builder)
            {
                interceptAsync = builder.interceptAsync;
                interceptProperties = builder.interceptProperties;

                activityKind = builder.activityKind;
                typeName = builder.typeName;

                includeByDefault = builder.includeByDefault;
                methodsPredicates = builder.methodsPredicates.AsReadOnly();
                commonTags = builder.commonTags.AsReadOnly();

                options = builder.options;

                activitySource = builder.activitySource ?? throw new InvalidOperationException("IActivitySource is required. Use builder.WithActivitySource(...)");
                getBeforeTags = CreateBeforeTags(builder);
                getResultTags = CreateGetResultTags(builder);
                getErrorTags = CreateGetErrorTags(builder);
            }

            public bool InterceptAsync => interceptAsync;

            #region [ ShouldIntercept ]

            private ConcurrentDictionary<MethodInfo, bool> shouldInterceptCache =
                new ConcurrentDictionary<MethodInfo, bool>();

            public bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args)
            {
                return shouldInterceptCache.GetOrAdd(
                    targetMethod,
                    ComputeShouldIntercept);
            }

            private bool ComputeShouldIntercept(MethodInfo targetMethod)
            {
                if (!interceptProperties && targetMethod.TryGetDeclaringProperty(out var _))
                {
                    return false;
                }

                var passFilter = methodsPredicates.Any(predicate => predicate(targetMethod));

                return includeByDefault ? !passFilter : passFilter;
            }

            #endregion [ ShouldIntercept ]

            #region [ Stages ]

            public OpenTelemetryInterceptorState Before(object target, MethodInfo targetMethod, object?[]? args)
            {
                var state = PrepareState(target, targetMethod, args);

                var activity = activitySource.StartActivity(
                    state.ActivityName,
                    state.ActivityKind,
                    parentContext: default(ActivityContext),
                    tags: state.Tags,
                    links: default,
                    default(DateTimeOffset));

                state = state with { Activity = activity };

                return state;
            }

            public void After(OpenTelemetryInterceptorState state, object target, MethodInfo targetMethod, object?[]? args,
                object? result)
            {
                if (state.Activity != null)
                {
                    state.Activity.SetTag(OTEL_STATUS_CODE, 200);
                    state.Activity.SetTag(ERROR, false);

                    try
                    {
                        var resultTags = state.GetResultTags(result);

                        if (resultTags != null)
                        {
                            foreach (var pair in resultTags)
                            {
                                state.Activity.SetTag(pair.Key, pair.Value);
                            }
                        }
                    }
                    catch
                    {
                    }

                    state.Activity.Stop();
                }
            }

            private const string OTEL_STATUS_CODE = "otel.status_code";
            private const string ERROR = "error";

            public void Error(OpenTelemetryInterceptorState state, object target, MethodInfo targetMethod, object?[]? args,
                Exception exception)
            {
                if (state.Activity != null)
                {
                    state.Activity.SetTag(OTEL_STATUS_CODE, 500);
                    state.Activity.SetTag(ERROR, true);

                    try
                    {
                        var errorTags = state.GetErrorTags(exception);

                        if (errorTags != null)
                        {
                            foreach (var pair in errorTags)
                            {
                                state.Activity.SetTag(pair.Key, pair.Value);
                            }
                        }
                    }
                    catch
                    {
                    }

                    state.Activity.Stop();
                }
            }

            #endregion [ Stages ]

            #region [ Internal ]

            private OpenTelemetryInterceptorState PrepareState(object target, MethodInfo targetMethod, object?[]? args)
            {
                string typeName = this.typeName ?? targetMethod.DeclaringType?.Name ?? options.UnknownTypeName;
                string methodName = targetMethod.Name;

                var activityName = options.ActivityNameFormat(typeName, methodName);

                var tags = getBeforeTags(target, targetMethod, args);

                IEnumerable<KeyValuePair<string, object?>>? GetResultTagsLocal(object? result) =>
                    getResultTags(targetMethod, result);

                IEnumerable<KeyValuePair<string, object?>>? GetErrorTagsLocal(Exception exception) =>
                    getErrorTags(targetMethod, exception);

                return new OpenTelemetryInterceptorState(
                    ActivityName: activityName,
                    ActivityKind: activityKind,
                    Tags: tags,
                    GetResultTags: GetResultTagsLocal,
                    GetErrorTags: GetErrorTagsLocal);
            }

            private Func<object, MethodInfo, object?[]?, ActivityTagsCollection> CreateBeforeTags(OpenTelemetryInterceptorBuilder builder)
            {
                Func<MethodInfo, object?[]?, IEnumerable<KeyValuePair<string, object?>>> GetParameterExtractor()
                {
                    if (builder.tagParameters is { } tagParameters)
                    {
                        if (builder.parametersExtractors.Any())
                        {
                            throw new InvalidOperationException("TagMethodParameters and TagParameter cannot be used together.");
                        }

                        IEnumerable<KeyValuePair<string, object?>> Local(MethodInfo method, object?[]? args)
                        {
                            var tagsObj = tagParameters(method, args);

                            var tagEnum = ExtractPairs(tagsObj);

                            return tagEnum ?? Enumerable.Empty<KeyValuePair<string, object?>>();
                        }

                        return Local;
                    }
                    
                    if (builder.parametersExtractors is { } parametersExtractors && parametersExtractors.Any())
                    {
                        ConcurrentDictionary<MethodInfo, Func<object?[]?, IEnumerable<KeyValuePair<string, object?>>>> extractorsCache =
                            new ConcurrentDictionary<MethodInfo, Func<object?[]?, IEnumerable<KeyValuePair<string, object?>>>>();

                        Func<object?[]?, IEnumerable<KeyValuePair<string, object?>>> GetExtractors(MethodInfo method)
                        {
                            var parameters = method.GetParameters();

                            var extractors = parameters
                                .SelectMany((parameter, index) => parametersExtractors
                                    .Where(e => e.Predicate(method, parameter.ParameterType, parameter.Name!))
                                    .Select(e => (Index: index, e.Extract)))
                                .GroupBy(p => p.Index)
                                .ToDictionary(
                                    g => g.Key,
                                    g => g.Select(e => e.Extract).ToArray());

                            if (extractors.Any())
                            {
                                IEnumerable<KeyValuePair<string, object?>> Extract(object?[]? args)
                                {
                                    if (args == null || args.Length != parameters!.Length)
                                    {
                                        return Enumerable.Empty<KeyValuePair<string, object?>>();
                                    }

                                    var dict = new Dictionary<string, object?>();

                                    foreach (var paramExtractors in extractors)
                                    {
                                        var value = args[paramExtractors.Key];

                                        foreach (var extractor in paramExtractors.Value)
                                        {
                                            try
                                            {
                                                var data = ExtractPairs(extractor(value));

                                                if (data != null)
                                                {
                                                    foreach (var pair in data)
                                                    {
                                                        dict[pair.Key] = pair.Value;
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }

                                    return dict;
                                }

                                return Extract;
                            }

                            return _ => Enumerable.Empty<KeyValuePair<string, object?>>();
                        }

                        return (method, args) => extractorsCache.GetOrAdd(method, GetExtractors)(args); ;
                    }

                    return (_, _) => Enumerable.Empty<KeyValuePair<string, object?>>();
                }

                Func<object, IEnumerable<KeyValuePair<string, object?>>> GetTargetTagExtractor()
                {
                    if (builder.tagTarget is { } tagTarget)
                    {
                        IEnumerable<KeyValuePair<string, object?>> Local(object target)
                        {
                            var tagsObj = tagTarget(target);

                            var tagEnum = ExtractPairs(tagsObj);

                            return tagEnum ?? Enumerable.Empty<KeyValuePair<string, object?>>();
                        }

                        return Local;
                    }
                    
                    if (builder.targetExtractors is { } targetExtractors && targetExtractors.Any())
                    {
                        ConcurrentDictionary<Type, Func<object, IEnumerable<KeyValuePair<string, object?>>>> extractorsCache =
                            new ConcurrentDictionary<Type, Func<object, IEnumerable<KeyValuePair<string, object?>>>>();

                        Func<object, IEnumerable<KeyValuePair<string, object?>>> GetExtractors(Type targetType)
                        {
                            var extractors = targetExtractors
                                .Where(e => e.Predicate(targetType))
                                .Select(e => e.Extract)
                                .ToArray();

                            if (extractors.Any())
                            {
                                IEnumerable<KeyValuePair<string, object?>> Extract(object target)
                                {
                                    if (target == null || !targetType.IsInstanceOfType(target))
                                    {
                                        return Enumerable.Empty<KeyValuePair<string, object?>>();
                                    }
                                    
                                    var dict = new Dictionary<string, object?>();

                                    foreach (var extractor in extractors)
                                    {
                                        try
                                        {
                                            var data = ExtractPairs(extractor(target));

                                            if (data != null)
                                            {
                                                foreach (var pair in data)
                                                {
                                                    dict[pair.Key] = pair.Value;
                                                }
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    return dict;
                                }

                                return Extract;
                            }

                            return _ => Enumerable.Empty<KeyValuePair<string, object?>>();
                        }

                        return target => extractorsCache.GetOrAdd(target.GetType(), GetExtractors)(target);
                    }

                    return _ => Enumerable.Empty<KeyValuePair<string, object?>>();
                }


                var targetExtractor = GetTargetTagExtractor();
                var parameterExtractor = GetParameterExtractor();

                ActivityTagsCollection CreateBeforeTagsLocal(object target, MethodInfo method, object?[]? args)
                {
                    var tags = new ActivityTagsCollection(commonTags);

                    foreach (var (key, value) in targetExtractor(target))
                    {
                        tags.Add(key, value);
                    }

                    foreach (var (key, value) in parameterExtractor(method, args))
                    {
                        tags.Add(key, value);
                    }

                    return tags;
                }

                return CreateBeforeTagsLocal;
            }

            private Func<MethodInfo, object?, IEnumerable<KeyValuePair<string, object?>>> CreateGetResultTags(OpenTelemetryInterceptorBuilder builder)
            {
                Func<MethodInfo, object?, IEnumerable<KeyValuePair<string, object?>>> GetResultExtractor()
                {
                    if (builder.tagResult is { } tagResult)
                    {
                        if (builder.resultExtractors.Any())
                        {
                            throw new InvalidOperationException(
                                "TagMethodResult and TagResult cannot be used together");
                        }

                        IEnumerable<KeyValuePair<string, object?>> Local(MethodInfo method, object? result)
                        {
                            var tagsObj = tagResult(method, result);

                            var tagEnum = ExtractPairs(tagsObj);

                            return tagEnum ?? Enumerable.Empty<KeyValuePair<string, object?>>();
                        }

                        return Local;
                    }
                    
                    if (builder.resultExtractors is { } resultExtractors && resultExtractors.Any())
                    {
                        ConcurrentDictionary<MethodInfo, Func<object?, IEnumerable<KeyValuePair<string, object?>>>> extractorsCache =
                            new ConcurrentDictionary<MethodInfo, Func<object?, IEnumerable<KeyValuePair<string, object?>>>>();

                        Type GetMethodResultType(MethodInfo method)
                        {
                            var returnType = method.ReturnType;

                            if (interceptAsync)
                            {
                                if (returnType == typeof(Task) || returnType == typeof(ValueTask))
                                {
                                    return typeof(void);
                                }

                                if (returnType.TryGetGenericParameters(typeof(Task<>), out var resultType) ||
                                    returnType.TryGetGenericParameters(typeof(ValueTask<>), out resultType))
                                {
                                    return resultType;
                                }
                            }

                            return returnType;
                        }

                        Func<object?, IEnumerable<KeyValuePair<string, object?>>> GetExtractors(MethodInfo methodInfo)
                        {
                            var methodResult = GetMethodResultType(methodInfo);

                            var extractors = resultExtractors
                                .Where(e => e.Predicate(methodInfo, methodResult))
                                .Select(e => e.Extract)
                                .ToArray();

                            if (extractors.Any())
                            {
                                IEnumerable<KeyValuePair<string, object?>> Extract(object? result)
                                {
                                    if (result == null)
                                    {
                                        return Enumerable.Empty<KeyValuePair<string, object?>>();
                                    }

                                    var dict = new Dictionary<string, object?>();

                                    foreach (var extractor in extractors)
                                    {
                                        try
                                        {
                                            var data = ExtractPairs(extractor(result));

                                            if (data != null)
                                            {
                                                foreach (var pair in data)
                                                {
                                                    dict[pair.Key] = pair.Value;
                                                }
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    return dict;
                                }

                                return Extract;
                            }

                            return _ => Enumerable.Empty<KeyValuePair<string, object?>>();
                        }


                        return (method, result) =>
                        {
                            var extractorFn = extractorsCache.GetOrAdd(method, GetExtractors);

                            return result == null 
                                ? Enumerable.Empty<KeyValuePair<string, object?>>() 
                                : extractorFn(result);
                        };
                    }

                    return (_, _) => Enumerable.Empty<KeyValuePair<string, object?>>();
                }

                var resultExtractor = GetResultExtractor();

                IEnumerable<KeyValuePair<string, object?>> CreateResultTagsLocal(MethodInfo method, object? result)
                {
                    return resultExtractor!(method, result);
                }

                return CreateResultTagsLocal;
            }

            private Func<MethodInfo, Exception, IEnumerable<KeyValuePair<string, object?>>> CreateGetErrorTags(OpenTelemetryInterceptorBuilder builder)
            {
                Func<MethodInfo, Exception, IEnumerable<KeyValuePair<string, object?>>> GetErrorExtractor()
                {
                    if (builder.tagError is { } tagError)
                    {
                        if (builder.errorExtractors.Any())
                        {
                            throw new InvalidOperationException(
                                "TagMethodError and TagError cannot be used together");
                        }

                        IEnumerable<KeyValuePair<string, object?>> Local(MethodInfo method, Exception exception)
                        {
                            var tagsObj = tagError(method, exception);

                            var tagEnum = ExtractPairs(tagsObj);

                            return tagEnum ?? Enumerable.Empty<KeyValuePair<string, object?>>();
                        }

                        return Local;
                    }

                    if (builder.errorExtractors is { } errorExtractors && errorExtractors.Any())
                    {
                        ConcurrentDictionary<(MethodInfo method, Type exceptionType), Func<Exception, IEnumerable<KeyValuePair<string, object?>>>> extractorsCache =
                            new ConcurrentDictionary<(MethodInfo method, Type exceptionType), Func<Exception, IEnumerable<KeyValuePair<string, object?>>>>();

                        Func<Exception, IEnumerable<KeyValuePair<string, object?>>> GetExtractors((MethodInfo method, Type exceptionType) key)
                        {
                            var extractors = errorExtractors
                                .Where(e => e.Predicate(key.method, key.exceptionType))
                                .Select(e => e.Extract)
                                .ToArray();

                            if (extractors.Any())
                            {
                                IEnumerable<KeyValuePair<string, object?>> Extract(Exception exception)
                                {
                                    var dict = new Dictionary<string, object?>();

                                    foreach (var extractor in extractors)
                                    {
                                        try
                                        {
                                            var data = ExtractPairs(extractor(exception));

                                            if (data != null)
                                            {
                                                foreach (var pair in data)
                                                {
                                                    dict[pair.Key] = pair.Value;
                                                }
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    return dict;
                                }

                                return Extract;
                            }

                            return _ => Enumerable.Empty<KeyValuePair<string, object?>>();
                        }


                        return (method, exception) =>
                        {
                            var key = (method, exception.GetType());

                            var extractorFn = extractorsCache.GetOrAdd(key, GetExtractors);

                            return extractorFn(exception);
                        };
                    }

                    return (_, _) => Enumerable.Empty<KeyValuePair<string, object?>>();
                }

                var resultExtractor = GetErrorExtractor();

                IEnumerable<KeyValuePair<string, object?>> CreateErrorTagsLocal(MethodInfo method, Exception exception)
                {
                    return resultExtractor!(method, exception);
                }

                return CreateErrorTagsLocal;
            }

            private static readonly ConcurrentDictionary<Type, PropertyInfo[]> metadataPropertiesCache =
                new ConcurrentDictionary<Type, PropertyInfo[]>();

            private static readonly HashSet<Type> SystemTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(char),
                typeof(bool),
                typeof(Int64),
                typeof(Int32),
                typeof(Int16),
                typeof(SByte),
                typeof(UInt64),
                typeof(UInt32),
                typeof(UInt16),
                typeof(Byte),
                typeof(Double),
                typeof(Single),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
            };

            private static IEnumerable<KeyValuePair<string, object?>>? ExtractPairs(object? metadata)
            {
                if (metadata == null)
                {
                    return null;
                }

                if (SystemTypes.Contains(metadata.GetType()))
                {
                    return new[] { KeyValuePair.Create("Value", (object?)metadata) };
                }

                if (metadata is IEnumerable enumerable)
                {
                    if (metadata is IEnumerable<KeyValuePair<string, object?>> keyValuePairs)
                    {
                        return keyValuePairs;
                    }

                    if (metadata is IEnumerable<Tuple<string, object?>> tuples)
                    {
                        return tuples
                            .Select(t => KeyValuePair.Create(t.Item1, t.Item2));
                    }

                    if (metadata is IEnumerable<(string key, object? value)> valueTuples)
                    {
                        return valueTuples
                            .Select(t => KeyValuePair.Create(t.key, t.value));
                    }

                    if (metadata.GetType().TryGetHierarchyGenericParameters(typeof(IEnumerable<>), out var tupleType))
                    {
                        if (tupleType.TryGetGenericParameters(typeof(KeyValuePair<,>), out var kvpString,
                                out var _) &&
                            kvpString == typeof(string))
                        {
                            return enumerable
                                .Cast<object>()
                                .Select(obj =>
                                {
                                    var key = tupleType.InvokeMember(
                                        "Key",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                                        binder: null,
                                        target: obj,
                                        args: null) as string;

                                    var value = tupleType.InvokeMember(
                                        "Value",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                                        binder: null,
                                        target: obj,
                                        args: null);

                                    return KeyValuePair.Create(key!, value);
                                });
                        }

                        if (tupleType.TryGetGenericParameters(typeof(Tuple<,>), out var tupleString,
                                out var _) &&
                            tupleString == typeof(string))
                        {
                            return enumerable
                                .Cast<object>()
                                .Select(obj =>
                                {
                                    var key = tupleType.InvokeMember(
                                        "Item1",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                                        binder: null,
                                        target: obj,
                                        args: null) as string;

                                    var value = tupleType.InvokeMember(
                                        "Item2",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                                        binder: null,
                                        target: obj,
                                        args: null);

                                    return KeyValuePair.Create(key!, value);
                                });
                        }

                        if (tupleType.TryGetGenericParameters(typeof(ValueTuple<,>), out var valueTupleString,
                                out var _) &&
                            valueTupleString == typeof(string))
                        {
                            return enumerable
                                .Cast<object>()
                                .Select(obj =>
                                {
                                    var key = tupleType.InvokeMember(
                                        "Item1",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField,
                                        binder: null,
                                        target: obj,
                                        args: null) as string;

                                    var value = tupleType.InvokeMember(
                                        "Item2",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField,
                                        binder: null,
                                        target: obj,
                                        args: null);

                                    return KeyValuePair.Create(key!, value);
                                });
                        }
                    }
                }

                var type = metadata.GetType();

                var properties = metadataPropertiesCache.GetOrAdd(
                    type,
                    t => t
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanRead)
                        .ToArray());

                return properties
                    .SelectMany(p =>
                    {
                        try
                        {
                            var value = p.GetValue(metadata);

                            return KeyValuePair.Create(p.Name, value).Singleton();
                        }
                        catch
                        {
                            return Enumerable.Empty<KeyValuePair<string, object?>>();
                        }
                    });
            }

            #endregion [ Internal ]

        }
    }
}
