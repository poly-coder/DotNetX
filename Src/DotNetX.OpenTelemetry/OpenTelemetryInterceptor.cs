﻿using DotNetX.Reflection;
using System;
using System.Reflection;

namespace DotNetX.OpenTelemetry
{
    public class OpenTelemetryInterceptor :
        IInterceptSyncMethod<OpenTelemetryInterceptorState>
    {
        private readonly IOpenTelemetryInterceptorSetup setup;

        public OpenTelemetryInterceptor(IOpenTelemetryInterceptorSetup setup)
        {
            this.setup = setup ?? throw new ArgumentNullException(nameof(setup));
        }

        public T Intercept<T>(T target) where T : class
        {
            var options = InterceptorOptions.Default;

            if (setup.InterceptAsync)
            {
                options = options
                    .Add(InterceptAsyncMethod<OpenTelemetryInterceptorState>.Default.With(this));
            }

            options = options
                .Add(InterceptSyncMethod<OpenTelemetryInterceptorState>.Default.With(this));

            return options.CreateInterceptor(target);
        }

        public bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args)
        {
            return setup.ShouldIntercept(target, targetMethod, args);
        }

        public OpenTelemetryInterceptorState Before(object target, MethodInfo targetMethod, object?[]? args)
        {
            return setup.Before(target, targetMethod, args);
        }

        public void After(OpenTelemetryInterceptorState state, object target, MethodInfo targetMethod, object?[]? args,
            object? result)
        {
            setup.After(state, target, targetMethod, args, result);
        }

        public void Error(OpenTelemetryInterceptorState state, object target, MethodInfo targetMethod, object?[]? args,
            Exception exception)
        {
            setup.Error(state, target, targetMethod, args, exception);
        }
    }
}
