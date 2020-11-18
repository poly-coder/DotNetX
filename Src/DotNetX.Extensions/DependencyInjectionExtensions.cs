using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;

namespace DotNetX
{
    public static class DependencyInjectionExtensions
    {
        public static Func<Type, TInstance> ServiceActivator<TInstance>(
            this IServiceProvider serviceProvider)
        {
            return type => (TInstance)ActivatorUtilities.CreateInstance(serviceProvider, type);
        }

        public static IServiceCollection AddPassThroughStringLocalizer(
            this IServiceCollection services,
            ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(IStringLocalizer<>),
                    typeof(PassThroughStringLocalizer<>),
                    serviceLifetime));

            return services;
        }
    }
}
