using DotNetX.Plugins;
using DotNetX.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetX
{
    public interface IServiceCollectionInitializer :
        IInitializer<ServiceCollectionInitializerContext>
    {
    }

    public record ServiceCollectionInitializerContext(
        IServiceCollection Services,
        IConfiguration Configuration);

    public static class ServiceCollectionInitializerExtensions
    {
        public static void InitializeServiceCollection(
            this IEnumerable<Type> exportedTypes,
            IServiceCollection services,
            IConfiguration configuration)
        {
            if (exportedTypes is null)
            {
                throw new ArgumentNullException(nameof(exportedTypes));
            }

            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var context = new ServiceCollectionInitializerContext(services, configuration);

            exportedTypes
                .ConcreteClassesImplementing<IServiceCollectionInitializer>()
                .ActivateAs<IServiceCollectionInitializer>()
                .Initialize(context);
        }

        public static void InitializeServiceCollection(
            this IEnumerable<Assembly> assemblies,
            IServiceCollection services,
            IConfiguration configuration)
        {
            if (assemblies is null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var context = new ServiceCollectionInitializerContext(services, configuration);

            assemblies
                .ExportedTypes()
                .ConcreteClassesImplementing<IServiceCollectionInitializer>()
                .ActivateAs<IServiceCollectionInitializer>()
                .Initialize(context);
        }
    }

}
