using DotNetX.Reflection;
using System;
using System.Reflection;

namespace DotNetX.Logging
{

    public class LoggingInterceptor :
        IInterceptSyncMethod<LoggingInterceptorState>,
        IInterceptEnumerableMethod<LoggingInterceptorState>
    {
        private readonly ILoggingInterceptorSetup setup;

        public LoggingInterceptor(ILoggingInterceptorSetup setup)
        {
            this.setup = setup ?? throw new ArgumentNullException(nameof(setup));
        }

        public T Intercept<T>(T target) where T : class
        {
            var options = InterceptorOptions.Default;

            if (setup.InterceptEnumerables)
            {
                options = options
                    .Add(InterceptEnumerableMethod<LoggingInterceptorState>.Default.With(this));
            }

            if (setup.InterceptAsync)
            {
                options = options
                    .Add(InterceptAsyncMethod<LoggingInterceptorState>.Default.With(this));
            }

            options = options
                .Add(InterceptSyncMethod<LoggingInterceptorState>.Default.With(this));

            return options.CreateInterceptor(target);

        }

        public bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args)
        {
            return setup.ShouldIntercept(target, targetMethod, args);
        }

        public void After(LoggingInterceptorState state, object target, MethodInfo targetMethod, object?[]? args, object? result)
        {
            setup.After(state, target, targetMethod, args, result);
        }

        public LoggingInterceptorState Before(object target, MethodInfo targetMethod, object?[]? args)
        {
            return setup.Before(target, targetMethod, args);
        }

        public void Complete(LoggingInterceptorState state, object target, MethodInfo targetMethod, object?[]? args)
        {
            setup.Complete(state, target, targetMethod, args);
        }

        public void Error(LoggingInterceptorState state, object target, MethodInfo targetMethod, object?[]? args, Exception exception)
        {
            setup.Error(state, target, targetMethod, args, exception);
        }

        public void Next(LoggingInterceptorState state, object target, MethodInfo targetMethod, object?[]? args, object? value)
        {
            setup.Next(state, target, targetMethod, args, value);
        }
    }
}
