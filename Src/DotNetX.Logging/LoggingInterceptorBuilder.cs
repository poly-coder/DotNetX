using DotNetX.Reflection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetX.Logging
{
    public sealed class LoggingInterceptorBuilder : ICloneable
    {
        private bool built;

        #region [ Clone ]

        public LoggingInterceptorBuilder Clone()
        {
            var clone = (LoggingInterceptorBuilder)this.MemberwiseClone();

            clone.loggerCategoryNames.AddRange(loggerCategoryNames);

            clone.options = options.Clone();
            
            clone.methodsPredicates.AddRange(methodsPredicates);

            clone.treatErrorsPredicates.AddRange(treatErrorsPredicates);

            clone.parametersExtractors.AddRange(parametersExtractors);
            
            clone.resultExtractors.AddRange(resultExtractors);

            return clone;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

        #region [ Logger / LoggerFactory ]

        private ILoggerFactory? loggerFactory;
        private ILogger? logger;
        private Func<ILoggerFactory, MethodInfo, ILogger>? loggerFromMethod;
        private Func<MethodInfo, ILogger>? statefulLoggerFromMethod;
        private readonly Dictionary<Type, string> loggerCategoryNames = new();

        public LoggingInterceptorBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
        {
            if (loggerFactory is null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            CheckNotBuilt();
            this.loggerFactory = loggerFactory;
            return this;
        }

        public LoggingInterceptorBuilder WithLoggerFactory(Func<ILoggerFactory, MethodInfo, ILogger> loggerFromMethod)
        {
            if (loggerFromMethod is null)
            {
                throw new ArgumentNullException(nameof(loggerFromMethod));
            }

            CheckNotBuilt();
            this.loggerFromMethod = loggerFromMethod;
            return this;
        }

        public LoggingInterceptorBuilder WithLoggerFactory(Func<MethodInfo, ILogger> statefulLoggerFromMethod)
        {
            if (statefulLoggerFromMethod is null)
            {
                throw new ArgumentNullException(nameof(statefulLoggerFromMethod));
            }

            CheckNotBuilt();
            this.statefulLoggerFromMethod = statefulLoggerFromMethod;
            return this;
        }
        
        public LoggingInterceptorBuilder WithLogger(ILogger logger)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            CheckNotBuilt();
            this.logger = logger;
            return this;
        }
        
        public LoggingInterceptorBuilder WithLoggerCategory(Type interfaceType, string categoryName)
        {
            if (interfaceType is null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if (categoryName is null)
            {
                throw new ArgumentNullException(nameof(categoryName));
            }

            CheckNotBuilt();
            this.loggerCategoryNames[interfaceType] = categoryName;
            return this;
        }
        
        public LoggingInterceptorBuilder WithLoggerCategory(Type interfaceType, Type categoryType)
        {
            if (categoryType is null)
            {
                throw new ArgumentNullException(nameof(categoryType));
            }

            return WithLoggerCategory(interfaceType, categoryType.FullName!);
        }

        public LoggingInterceptorBuilder WithLoggerCategory<TInterface>(string categoryName)
            where TInterface : class =>
            WithLoggerCategory(typeof(TInterface), categoryName);

        public LoggingInterceptorBuilder WithLoggerCategory<TInterface>(Type categoryType)
            where TInterface : class =>
            WithLoggerCategory(typeof(TInterface), categoryType);

        public LoggingInterceptorBuilder WithLoggerCategory<TInterface, TCategory>()
            where TInterface : class =>
            WithLoggerCategory(typeof(TInterface), typeof(TCategory));

        #endregion [ Logger / LoggerFactory ]

        #region [ Options ]

        private LoggingInterceptorOptions options = new LoggingInterceptorOptions();

        public LoggingInterceptorBuilder WithOptions(LoggingInterceptorOptions options)
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

        private bool interceptEnumerables = false;
        private bool interceptAsync = true;
        private bool interceptProperties = false;

        public LoggingInterceptorBuilder DoNotInterceptEnumerables()
        {
            CheckNotBuilt();
            interceptEnumerables = false;
            return this;
        }

        public LoggingInterceptorBuilder InterceptEnumerables()
        {
            CheckNotBuilt();
            interceptEnumerables = true;
            return this;
        }

        public LoggingInterceptorBuilder DoNotInterceptAsync()
        {
            CheckNotBuilt();
            interceptAsync = false;
            return this;
        }

        public LoggingInterceptorBuilder InterceptAsync()
        {
            CheckNotBuilt();
            interceptAsync = true;
            return this;
        }

        public LoggingInterceptorBuilder DoNotInterceptProperties()
        {
            CheckNotBuilt();
            interceptProperties = false;
            return this;
        }

        public LoggingInterceptorBuilder InterceptProperties()
        {
            CheckNotBuilt();
            interceptProperties = true;
            return this;
        }

        #endregion [ Flags ]

        #region [ Filters ]

        private bool includeByDefault = true;
        private readonly List<Func<MethodInfo, bool>> methodsPredicates = new();

        public LoggingInterceptorBuilder DoNotInterceptIf(Func<MethodInfo, bool> predicate)
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

        public LoggingInterceptorBuilder DoNotInterceptIfNamed(string methodName)
        {
            if (methodName is null) throw new ArgumentNullException(nameof(methodName));

            return DoNotInterceptIf(method => method.Name == methodName);
        }

        public LoggingInterceptorBuilder DoNotInterceptIfMatches(Regex pattern)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return DoNotInterceptIf(method => pattern.IsMatch(method.Name));
        }

        public LoggingInterceptorBuilder DoNotInterceptIfMatches(string pattern)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return DoNotInterceptIf(method => Regex.IsMatch(method.Name, pattern));
        }

        public LoggingInterceptorBuilder DoInterceptIf(Func<MethodInfo, bool> predicate)
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

        public LoggingInterceptorBuilder DoInterceptIfNamed(string methodName)
        {
            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            return DoInterceptIf(method => method.Name == methodName);
        }

        public LoggingInterceptorBuilder DoInterceptIfMatches(Regex pattern)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return DoInterceptIf(method => pattern.IsMatch(method.Name));
        }

        public LoggingInterceptorBuilder DoInterceptIfMatches(string pattern)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return DoInterceptIf(method => Regex.IsMatch(method.Name, pattern));
        }

        #endregion [ Filters ]

        #region [ SkipException ]

        private readonly List<Func<MethodInfo, Exception, bool>> treatErrorsPredicates = new();

        public LoggingInterceptorBuilder SkipExceptionIf(Func<MethodInfo, Exception, bool> predicate)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            CheckNotBuilt();
            treatErrorsPredicates.Add(predicate);
            return this;
        }

        public LoggingInterceptorBuilder SkipExceptionIf(Func<Exception, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return SkipExceptionIf((_, exception) => predicate(exception));
        }

        public LoggingInterceptorBuilder SkipException(
            string methodName,
            Type exceptionType,
            Func<Exception, bool>? predicate = null)
        {
            if (methodName is null) throw new ArgumentNullException(nameof(methodName));
            if (exceptionType == null) throw new ArgumentNullException(nameof(exceptionType));

            Func<MethodInfo, Exception, bool> innerPredicate =
                predicate == null
                    ? (method, exception) =>
                        method.Name == methodName && 
                        exceptionType.IsInstanceOfType(exception)
                    : (method, exception) => 
                        method.Name == methodName && 
                        exceptionType.IsInstanceOfType(exception) && 
                        predicate(exception);

            return SkipExceptionIf(innerPredicate);
        }

        public LoggingInterceptorBuilder SkipException<TException>(
            string methodName,
            Func<TException, bool>? predicate = null)
            where TException : Exception
        {
            if (methodName is null) throw new ArgumentNullException(nameof(methodName));

            Func<MethodInfo, Exception, bool> innerPredicate =
                predicate == null
                    ? (method, exception) =>
                        method.Name == methodName &&
                        exception is TException
                    : (method, exception) =>
                        method.Name == methodName &&
                        exception is TException ex && 
                        predicate(ex);

            return SkipExceptionIf(innerPredicate);
        }

        public LoggingInterceptorBuilder SkipException(
            Regex pattern,
            Type exceptionType,
            Func<Exception, bool>? predicate = null)
        {
            if (pattern is null) throw new ArgumentNullException(nameof(pattern));
            if (exceptionType == null) throw new ArgumentNullException(nameof(exceptionType));

            Func<MethodInfo, Exception, bool> innerPredicate =
                predicate == null
                    ? (method, exception) =>
                        pattern.IsMatch(method.Name) && 
                        exceptionType.IsInstanceOfType(exception)
                    : (method, exception) =>
                        pattern.IsMatch(method.Name) && 
                        exceptionType.IsInstanceOfType(exception) && 
                        predicate(exception);

            return SkipExceptionIf(innerPredicate);
        }

        public LoggingInterceptorBuilder SkipException<TException>(
            Regex pattern,
            Func<TException, bool>? predicate = null)
            where TException : Exception
        {
            if (pattern is null) throw new ArgumentNullException(nameof(pattern));

            Func<MethodInfo, Exception, bool> innerPredicate =
                predicate == null
                    ? (method, exception) =>
                        pattern.IsMatch(method.Name) &&
                        exception is TException
                    : (method, exception) =>
                        pattern.IsMatch(method.Name) &&
                        exception is TException ex && 
                        predicate(ex);

            return SkipExceptionIf(innerPredicate);
        }

        public LoggingInterceptorBuilder SkipException(
            Type exceptionType, 
            Func<Exception, bool>? predicate = null)
        {
            if (exceptionType == null) throw new ArgumentNullException(nameof(exceptionType));

            Func<Exception, bool> innerPredicate =
                    predicate == null 
                        ? exceptionType.IsInstanceOfType
                        : exception => exceptionType.IsInstanceOfType(exception) && predicate(exception);

            return SkipExceptionIf(innerPredicate);
        }

        public LoggingInterceptorBuilder SkipException<TException>(
            Func<TException, bool>? predicate = null)
            where TException: Exception
        {
            Func<Exception, bool> innerPredicate =
                    predicate == null 
                        ? exception => exception is TException
                        : exception => exception is TException ex && predicate(ex);

            return SkipExceptionIf(innerPredicate);
        }

        #endregion

        #region [ WithParameters ]

        private Func<MethodInfo, object?[]?, object?>? getParameters;

        record ParametersExtractor(
            Func<MethodInfo, Type, string, bool> Predicate,
            Func<object?, object?> Extract);

        private readonly List<ParametersExtractor> parametersExtractors = new List<ParametersExtractor>();

        public LoggingInterceptorBuilder WithParameters(Func<MethodInfo, object?[]?, object?> getParameters)
        {
            if (getParameters is null)
            {
                throw new ArgumentNullException(nameof(getParameters));
            }

            CheckNotBuilt();
            this.getParameters = getParameters;
            return this;
        }

        public LoggingInterceptorBuilder LogParameter(
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

        public LoggingInterceptorBuilder LogParameter(
            string methodName,
            Type type,
            string name,
            Func<object?, object?> extract)
        {
            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            return LogParameter(
                (method, parameterType, parameterName) =>
                    method.Name == methodName &&
                    parameterType == type &&
                    parameterName == name,
                extract);
        }

        public LoggingInterceptorBuilder LogParameter<T>(
            string methodName,
            string name,
            Func<T, object?> extract) =>
            LogParameter(
                methodName, typeof(T), name,
                obj => obj is T t ? extract(t) : null);

        public LoggingInterceptorBuilder LogParameter(
            string methodName,
            string name,
            Func<object?, object?> extract)
        {
            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            return LogParameter(
                (method, parameterType, parameterName) =>
                    method.Name == methodName &&
                    parameterName == name,
                extract);
        }

        public LoggingInterceptorBuilder LogParameter(
            Type type,
            string name,
            Func<object?, object?> extract)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            return LogParameter(
                (method, parameterType, parameterName) =>
                    parameterType == type &&
                    parameterName == name,
                extract);
        }

        public LoggingInterceptorBuilder LogParameter<T>(
            string name,
            Func<T, object?> extract) =>
            LogParameter(
                typeof(T), name,
                obj => obj is T t ? extract(t) : null);

        public LoggingInterceptorBuilder LogParameter(
            Type type,
            Func<object?, object?> extract)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            return LogParameter(
                (method, parameterType, parameterName) =>
                    parameterType == type,
                extract);
        }

        public LoggingInterceptorBuilder LogParameter<T>(
            Func<T, object?> extract) =>
            LogParameter(
                typeof(T),
                obj => obj is T t ? extract(t) : null);

        public LoggingInterceptorBuilder LogParameter(
            string name,
            Func<object?, object?> extract)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            return LogParameter(
                (method, parameterType, parameterName) =>
                    parameterName == name,
                extract);
        }

        public LoggingInterceptorBuilder LogParameter(
            string methodName,
            Type type,
            string name,
            string? outputName = null) =>
            LogParameter(
                methodName, type, name,
                obj => new[] { KeyValuePair.Create(outputName ?? name, obj) });

        public LoggingInterceptorBuilder LogParameter<T>(
            string methodName,
            string name,
            string? outputName = null) =>
            LogParameter(methodName, typeof(T), name, outputName);

        public LoggingInterceptorBuilder LogParameter(
            string methodName,
            string name,
            string? outputName = null) =>
            LogParameter(
                methodName, name,
                obj => new[] { KeyValuePair.Create(outputName ?? name, obj) });

        public LoggingInterceptorBuilder LogParameter(
            Type type,
            string name,
            string? outputName = null) =>
            LogParameter(
                type, name,
                obj => new[] { KeyValuePair.Create(outputName ?? name, obj) });

        public LoggingInterceptorBuilder LogParameter<T>(
            string name,
            string? outputName = null) =>
            LogParameter(typeof(T), name, outputName);

        public LoggingInterceptorBuilder LogParameter(
            string name,
            string? outputName = null) =>
            LogParameter(
                name,
                obj => new[] { KeyValuePair.Create(outputName ?? name, obj) });

        #endregion [ WithParameters ]

        #region [ WithResult ]

        private Func<MethodInfo, object?, object?>? getResult;

        record ResultExtractor(
            Func<MethodInfo, Type, bool> Predicate,
            Func<object?, object?> Extract);

        private readonly List<ResultExtractor> resultExtractors = new();

        public LoggingInterceptorBuilder WithResult(Func<MethodInfo, object?, object?> getResult)
        {
            if (getResult is null)
            {
                throw new ArgumentNullException(nameof(getResult));
            }

            CheckNotBuilt();
            this.getResult = getResult;
            return this;
        }

        public LoggingInterceptorBuilder LogResult(
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

        public LoggingInterceptorBuilder LogResult(
            string methodName,
            Type type,
            Func<object?, object?> extract)
        {
            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            return LogResult(
                (method, resultType) => method.Name == methodName && resultType == type,
                extract);
        }

        public LoggingInterceptorBuilder LogResult<T>(
            string methodName,
            Func<T, object?> extract) =>
            LogResult(
                methodName,
                typeof(T),
                obj => obj is T t ? extract(t) : null);

        public LoggingInterceptorBuilder LogResult(
            string methodName,
            Func<object?, object?> extract)
        {
            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            return LogResult(
                (method, resultType) => method.Name == methodName,
                extract);
        }

        public LoggingInterceptorBuilder LogResult(
            Type type,
            Func<object?, object?> extract)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (extract is null)
            {
                throw new ArgumentNullException(nameof(extract));
            }

            return LogResult(
                (method, resultType) => resultType == type,
                extract);
        }

        public LoggingInterceptorBuilder LogResult<T>(
            Func<T, object?> extract) =>
            LogResult(
                typeof(T),
                obj => obj is T t ? extract(t) : null);

        public LoggingInterceptorBuilder LogResult(
            string methodName,
            Type type,
            string outputName)
        {
            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (outputName is null)
            {
                throw new ArgumentNullException(nameof(outputName));
            }

            return LogResult(
                (method, resultType) => method.Name == methodName && resultType == type,
                obj => new[] { KeyValuePair.Create(outputName, obj) });
        }

        public LoggingInterceptorBuilder LogResult<T>(
            string methodName,
            string outputName) =>
            LogResult(methodName, typeof(T), outputName);

        public LoggingInterceptorBuilder LogResult(
            string methodName,
            string outputName)
        {
            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            if (outputName is null)
            {
                throw new ArgumentNullException(nameof(outputName));
            }

            return LogResult(
                (method, resultType) => method.Name == methodName,
                obj => new[] { KeyValuePair.Create(outputName, obj) });
        }

        public LoggingInterceptorBuilder LogResult(
            Type type,
            string outputName)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (outputName is null)
            {
                throw new ArgumentNullException(nameof(outputName));
            }

            return LogResult(
                (method, resultType) => resultType == type,
                obj => new[] { KeyValuePair.Create(outputName, obj) });
        }

        public LoggingInterceptorBuilder LogResult<T>(
            string outputName) =>
            LogResult(typeof(T), outputName);


        #endregion [ WithResult ]

        public LoggingInterceptor Build()
        {
            CheckNotBuilt();

            built = true;

            return new LoggingInterceptor(new InterceptorSetup(this));
        }

        private void CheckNotBuilt()
        {
            if (built)
            {
                throw new InvalidOperationException("LoggingInterceptor is already built");
            }
        }

        class InterceptorSetup : ILoggingInterceptorSetup
        {
            private readonly bool interceptEnumerables;
            private readonly bool interceptAsync;
            private readonly bool interceptProperties;
            private readonly bool includeByDefault;
            private readonly LoggingInterceptorOptions options;

            private readonly IReadOnlyCollection<Func<MethodInfo, bool>> methodsPredicates;
            private readonly Func<MethodInfo, Exception, bool> treatErrorAsComplete;

            private readonly Func<MethodInfo, ILogger> loggerFactory;

            private readonly Func<MethodInfo, object?[]?, object?> getParameters;
            private readonly Func<MethodInfo, object?, object?> getResult;

            public InterceptorSetup(
                LoggingInterceptorBuilder builder)
            {
                interceptEnumerables = builder.interceptEnumerables;
                interceptAsync = builder.interceptAsync;
                interceptProperties = builder.interceptProperties;

                includeByDefault = builder.includeByDefault;
                methodsPredicates = builder.methodsPredicates.AsReadOnly();

                options = builder.options;

                loggerFactory = CreateLoggerFactory(builder);
                getParameters = CreateGetParameters(builder);
                getResult = CreateGetResult(builder);
                treatErrorAsComplete = CreateTreatErrorAsComplete(builder);
            }

            public bool InterceptEnumerables => interceptEnumerables;

            public bool InterceptAsync => interceptAsync;

            public bool InterceptProperties => interceptProperties;

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

            public LoggingInterceptorState Before(
                object target,
                MethodInfo targetMethod,
                object?[]? args)
            {
                var state = PrepareState(target, targetMethod, args);

                if (state.Logger.IsEnabled(options.StartLogLevel))
                {
                    if (state.Parameters != null)
                    {
                        state.Logger.Log(
                            options.StartLogLevel,
                            options.StartParametersLogMessage,
                            state.TypeName,
                            targetMethod.Name,
                            state.Parameters,
                            options.StartStage);
                    }
                    else
                    {
                        state.Logger.Log(
                            options.StartLogLevel,
                            options.StartLogMessage,
                            state.TypeName,
                            targetMethod.Name,
                            options.StartStage);
                    }
                }

                state.Watch.Start();

                return state;
            }

            public void After(
                LoggingInterceptorState state,
                object target,
                MethodInfo targetMethod,
                object?[]? args,
                object? result)
            {
                state.Watch.Stop();

                if (state.Logger.IsEnabled(options.DoneLogLevel))
                {
                    var elapsedMs = state.Watch.Elapsed.TotalMilliseconds;

                    var resultMetadata = state.GetResult(result);

                    if (state.Parameters != null)
                    {
                        if (resultMetadata != null)
                        {
                            state.Logger.Log(
                                options.DoneLogLevel,
                                options.ResultParametersLogMessage,
                                state.TypeName,
                                targetMethod.Name,
                                state.Parameters,
                                options.ResultStage,
                                resultMetadata,
                                elapsedMs);
                        }
                        else
                        {
                            state.Logger.Log(
                                options.DoneLogLevel,
                                options.DoneParametersLogMessage,
                                state.TypeName,
                                targetMethod.Name,
                                state.Parameters,
                                options.DoneStage,
                                elapsedMs);
                        }
                    }
                    else
                    {
                        if (resultMetadata != null)
                        {
                            state.Logger.Log(
                                options.DoneLogLevel,
                                options.ResultLogMessage,
                                state.TypeName,
                                targetMethod.Name,
                                options.ResultStage,
                                resultMetadata,
                                elapsedMs);
                        }
                        else
                        {
                            state.Logger.Log(
                                options.DoneLogLevel,
                                options.DoneLogMessage,
                                state.TypeName,
                                targetMethod.Name,
                                options.DoneStage,
                                elapsedMs);
                        }
                    }
                }
            }

            public void Next(
                LoggingInterceptorState state,
                object target,
                MethodInfo targetMethod,
                object?[]? args,
                object? value)
            {
                if (state.Logger.IsEnabled(options.NextLogLevel))
                {
                    var elapsedMs = state.Watch.Elapsed.TotalMilliseconds;

                    var resultMetadata = state.GetResult(value);

                    if (state.Parameters != null)
                    {
                        if (resultMetadata != null)
                        {
                            state.Logger.Log(
                                options.NextLogLevel,
                                options.ResultParametersLogMessage,
                                state.TypeName,
                                targetMethod.Name,
                                state.Parameters,
                                options.NextStage,
                                resultMetadata,
                                elapsedMs);
                        }
                        else
                        {
                            state.Logger.Log(
                                options.NextLogLevel,
                                options.DoneParametersLogMessage,
                                state.TypeName,
                                targetMethod.Name,
                                state.Parameters,
                                options.NextStage,
                                elapsedMs);
                        }
                    }
                    else
                    {
                        if (resultMetadata != null)
                        {
                            state.Logger.Log(
                                options.NextLogLevel,
                                options.ResultLogMessage,
                                state.TypeName,
                                targetMethod.Name,
                                options.NextStage,
                                resultMetadata,
                                elapsedMs);
                        }
                        else
                        {
                            state.Logger.Log(
                                options.NextLogLevel,
                                options.DoneLogMessage,
                                state.TypeName,
                                targetMethod.Name,
                                options.NextStage,
                                elapsedMs);
                        }
                    }
                }
            }

            public void Complete(
                LoggingInterceptorState state,
                object target,
                MethodInfo targetMethod,
                object?[]? args)
            {
                state.Watch.Stop();

                if (state.Logger.IsEnabled(options.DoneLogLevel))
                {
                    var elapsedMs = state.Watch.Elapsed.TotalMilliseconds;

                    if (state.Parameters != null)
                    {
                        state.Logger.Log(
                            options.DoneLogLevel,
                            options.DoneParametersLogMessage,
                            state.TypeName,
                            targetMethod.Name,
                            state.Parameters,
                            options.CompleteStage,
                            elapsedMs);
                    }
                    else
                    {
                        state.Logger.Log(
                            options.DoneLogLevel,
                            options.DoneLogMessage,
                            state.TypeName,
                            targetMethod.Name,
                            options.CompleteStage,
                            elapsedMs);
                    }
                }
            }

            public void Error(
                LoggingInterceptorState state,
                object target,
                MethodInfo targetMethod,
                object?[]? args,
                Exception exception)
            {
                state.Watch.Stop();

                if (treatErrorAsComplete(targetMethod, exception))
                {
                    Complete(state, target, targetMethod, args);
                    return;
                }

                if (state.Logger.IsEnabled(options.ErrorLogLevel))
                {
                    var elapsedMs = state.Watch.Elapsed.TotalMilliseconds;

                    if (state.Parameters != null)
                    {
                        state.Logger.Log(
                            options.ErrorLogLevel,
                            exception,
                            options.ErrorParametersLogMessage,
                            state.TypeName,
                            targetMethod.Name,
                            state.Parameters,
                            options.ErrorStage,
                            elapsedMs);
                    }
                    else
                    {
                        state.Logger.Log(
                            options.ErrorLogLevel,
                            exception,
                            options.ErrorLogMessage,
                            state.TypeName,
                            targetMethod.Name,
                            options.ErrorStage,
                            elapsedMs);
                    }
                }
            }

            #endregion [ Stages ]

            #region [ Internal ]

            private LoggingInterceptorState PrepareState(object target, MethodInfo targetMethod, object?[]? args)
            {
                var watch = new Stopwatch();

                var logger = loggerFactory(targetMethod);

                string typeName = targetMethod.DeclaringType?.Name ?? options.UnknownTypeName;

                object? parameters = getParameters(targetMethod, args);

                object? GetResultLocal(object? result) => getResult(targetMethod, result);

                return new LoggingInterceptorState(
                    Watch: watch,
                    Logger: logger,
                    TypeName: typeName,
                    Parameters: parameters,
                    GetResult: GetResultLocal);
            }

            private static Func<MethodInfo, ILogger> CreateLoggerFactory(LoggingInterceptorBuilder builder)
            {
                if (builder.logger != null)
                {
                    if (builder.loggerFactory != null ||
                        builder.statefulLoggerFromMethod != null ||
                        builder.loggerFromMethod != null ||
                        builder.loggerCategoryNames.Any())
                    {
                        throw new InvalidOperationException("When logger is set, no other logging method should be provided");
                    }

                    var logger = builder.logger;
                    return _ => logger;
                }
                else if (builder.statefulLoggerFromMethod != null)
                {
                    if (builder.loggerFactory != null ||
                        builder.loggerFromMethod != null ||
                        builder.loggerCategoryNames.Any())
                    {
                        throw new InvalidOperationException("When stateful logger factory is set, no other logging method should be provided");
                    }

                    return builder.statefulLoggerFromMethod;
                }
                else if (builder.loggerFromMethod != null)
                {
                    if (builder.loggerFactory == null)
                    {
                        throw new InvalidOperationException("No ILoggerFactory was provided");
                    }

                    if (builder.loggerCategoryNames.Any())
                    {
                        throw new InvalidOperationException("When logger factory is set, no other logging method should be provided");
                    }

                    var loggerSource = builder.loggerFactory;
                    var factory = builder.loggerFromMethod;

                    return method => factory(loggerSource, method);
                }
                else if (builder.loggerFactory != null)
                {
                    var loggerSource = builder.loggerFactory;
                    var loggerCategoryNames = builder.loggerCategoryNames;
                    var options = builder.options;

                    ConcurrentDictionary<string, ILogger> loggerCache =
                        new ConcurrentDictionary<string, ILogger>();

                    ILogger CreateLogger(MethodInfo targetMethod)
                    {
                        string? categoryName = null;

                        if (targetMethod.DeclaringType != null)
                        {
                            if (loggerCategoryNames.TryGetValue(targetMethod.DeclaringType, out categoryName))
                            {

                            }
                            else if ((categoryName = targetMethod.DeclaringType.FullName) != null)
                            {

                            }
                        }

                        categoryName ??= options.UnknownCategoryName;

                        return loggerCache.GetOrAdd(
                            categoryName,
                            loggerSource.CreateLogger);
                    }

                    return CreateLogger;
                }
                else
                {
                    throw new InvalidOperationException("No logger factory was provided");
                }
            }

            private static Func<MethodInfo, object?, object?> CreateGetResult(LoggingInterceptorBuilder builder)
            {
                if (builder.getResult != null)
                {
                    if (builder.resultExtractors.Any())
                    {
                        throw new InvalidOperationException("When getResult is provided, no other result extractor should be provided");
                    }

                    return builder.getResult;
                }
                else if (builder.resultExtractors.Any())
                {
                    ConcurrentDictionary<MethodInfo, Func<object?, object?>> extractorsCache =
                        new ConcurrentDictionary<MethodInfo, Func<object?, object?>>();

                    var allExtractors = builder.resultExtractors;
                    var interceptAsync = builder.interceptAsync;
                    var interceptEnumerables = builder.interceptEnumerables;

                    Type GetMethodResultType(MethodInfo method)
                    {
                        var returnType = method.ReturnType;

                        if (interceptEnumerables)
                        {
                            if (returnType == typeof(IEnumerable))
                            {
                                return typeof(object);
                            }

                            if (returnType.TryGetGenericParameters(typeof(IEnumerable<>), out var resultType) ||
                                returnType.TryGetGenericParameters(typeof(IAsyncEnumerable<>), out resultType) ||
                                returnType.TryGetGenericParameters(typeof(IObservable<>), out resultType))
                            {
                                return resultType;
                            }
                        }

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
                    
                    Func<object?, object?> GetExtractors(MethodInfo method)
                    {
                        var methodResult = GetMethodResultType(method);

                        var extractors = allExtractors
                            .Where(e => e.Predicate(method, methodResult))
                            .Select(e => e.Extract)
                            .ToArray();

                        if (extractors.Any())
                        {
                            object? Extract(object? result)
                            {
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

                                if (dict.Any())
                                {
                                    return dict;
                                }

                                return null;
                            }

                            return Extract;
                        }
                        
                        return _ => null;
                    }

                    return (method, result) => extractorsCache.GetOrAdd(method, GetExtractors)(result);
                }

                return (_, _) => null;
            }

            private static Func<MethodInfo, object?[]?, object?> CreateGetParameters(LoggingInterceptorBuilder builder)
            {
                if (builder.getParameters != null)
                {
                    if (builder.parametersExtractors.Any())
                    {
                        throw new InvalidOperationException("When getParameters is provided, no other parameter extractor should be provided");
                    }

                    return builder.getParameters;
                }
                else if (builder.parametersExtractors.Any())
                {
                    ConcurrentDictionary<MethodInfo, Func<object?[]?, object?>> extractorsCache =
                        new ConcurrentDictionary<MethodInfo, Func<object?[]?, object?>>();

                    var allExtractors = builder.parametersExtractors;

                    Func<object?[]?, object?> GetExtractors(MethodInfo method)
                    {
                        var parameters = method.GetParameters();

                        var extractors = parameters
                            .SelectMany((parameter, index) => allExtractors
                                .Where(e => e.Predicate(method, parameter.ParameterType, parameter.Name!))
                                .Select(e => (Index: index, e.Extract)))
                            .GroupBy(p => p.Index)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.Extract).ToArray());

                        if (extractors.Any())
                        {
                            object? Extract(object?[]? args)
                            {
                                if (args == null || args.Length != parameters!.Length)
                                {
                                    return null;
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

                                if (dict.Any())
                                {
                                    return dict;
                                }

                                return null;
                            }

                            return Extract;
                        }

                        return _ => null;
                    }

                    return (method, args) => extractorsCache.GetOrAdd(method, GetExtractors)(args);
                }

                return (_, _) => null;
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

            private Func<MethodInfo, Exception, bool> CreateTreatErrorAsComplete(LoggingInterceptorBuilder builder)
            {
                var predicates = builder.treatErrorsPredicates.AsReadOnly();

                bool ResultFunc(MethodInfo method, Exception exception) => 
                    predicates.Any(p => p(method, exception));

                return ResultFunc;
            }

            #endregion [ Internal ]
        }
    }
}
