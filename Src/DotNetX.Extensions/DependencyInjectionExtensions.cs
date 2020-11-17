using Microsoft.Extensions.DependencyInjection;
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
    }
}
