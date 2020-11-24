using Microsoft.Extensions.DependencyInjection;
using System;

namespace DotNetX
{
    public static class ServiceProviderExtensions
    {
        public static TService GetServiceOrCreateInstance<TService>(this IServiceProvider serviceProvider)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance<TService>(serviceProvider);
        }

        public static TService CreateInstance<TService>(this IServiceProvider serviceProvider)
        {
            return ActivatorUtilities.CreateInstance<TService>(serviceProvider);
        }

        public static TService CreateInstance<TService>(this IServiceProvider serviceProvider, params object[] arguments)
        {
            return ActivatorUtilities.CreateInstance<TService>(serviceProvider, arguments);
        }

        public static object GetServiceOrCreateInstance(this IServiceProvider serviceProvider, Type serviceType)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, serviceType);
        }

        public static object CreateInstance(this IServiceProvider serviceProvider, Type serviceType)
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, serviceType);
        }

        public static object CreateInstance(this IServiceProvider serviceProvider, Type serviceType, params object[] arguments)
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, serviceType, arguments);
        }

        public static ObjectFactory CreateFactory(this Type serviceType, Type[] argumentTypes)
        {
            return ActivatorUtilities.CreateFactory(serviceType, argumentTypes);
        }
    }
}
