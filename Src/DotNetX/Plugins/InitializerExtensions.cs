using DotNetX.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Plugins
{
    public static class InitializerExtensions
    {
        // IInitializer<T>

        public static void Initialize<T>(this IEnumerable<IInitializer<T>> initializers, T context)
        {
            if (initializers is null)
            {
                throw new System.ArgumentNullException(nameof(initializers));
            }

            initializers.ForEach(i => i.Initialize(context));
        }

        public static void InitializeAllWith<T>(
            this IEnumerable<Assembly> assemblies, 
            T context, 
            Func<Type, IInitializer<T>>? activator)
        {
            if (assemblies is null)
            {
                throw new System.ArgumentNullException(nameof(assemblies));
            }

            assemblies
                .ExportedTypes()
                .ConcreteClassesImplementing<IInitializer<T>>()
                .ActivateAs(activator)
                .Initialize(context);
        }


        public static void InitializeAll<T>(
            this IEnumerable<Assembly> assemblies,
            T context) =>
            assemblies.InitializeAllWith(context, activator: null);

        // IInitializer

        public static void Initialize(this IEnumerable<IInitializer> initializers)
        {
            if (initializers is null)
            {
                throw new System.ArgumentNullException(nameof(initializers));
            }

            initializers.ForEach(i => i.Initialize());
        }

        public static void InitializeAllWith(
            this IEnumerable<Assembly> assemblies,
            Func<Type, IInitializer>? activator)
        {
            if (assemblies is null)
            {
                throw new System.ArgumentNullException(nameof(assemblies));
            }

            assemblies
                .ExportedTypes()
                .ConcreteClassesImplementing<IInitializer>()
                .ActivateAs(activator)
                .Initialize();
        }


        public static void InitializeAll(this IEnumerable<Assembly> assemblies) =>
            assemblies.InitializeAllWith(activator: null);

        // IAsyncInitializer<T>

        public static async Task InitializeAsync<T>(
            this IEnumerable<IAsyncInitializer<T>> initializers, 
            T context, 
            CancellationToken cancellationToken)
        {
            if (initializers is null)
            {
                throw new System.ArgumentNullException(nameof(initializers));
            }

            await initializers.ParallelForEachAsync(
                (i, ct) => i.InitializeAsync(context, ct), 
                cancellationToken);
        }

        public static async Task InitializeAllAsyncWith<T>(
            this IEnumerable<Assembly> assemblies, 
            T context,
            Func<Type, IAsyncInitializer<T>>? activator,
            CancellationToken cancellationToken)
        {
            if (assemblies is null)
            {
                throw new System.ArgumentNullException(nameof(assemblies));
            }

            await assemblies
                .ExportedTypes()
                .ConcreteClassesImplementing<IAsyncInitializer<T>>()
                .ActivateAs(activator)
                .InitializeAsync(context, cancellationToken);
        }

        public static Task InitializeAllAsync<T>(
            this IEnumerable<Assembly> assemblies,
            T context,
            CancellationToken cancellationToken) =>
            assemblies.InitializeAllAsyncWith(context, activator: null, cancellationToken);

        // IAsyncInitializer

        public static async Task InitializeAsync(
            this IEnumerable<IAsyncInitializer> initializers, 
            CancellationToken cancellationToken)
        {
            if (initializers is null)
            {
                throw new System.ArgumentNullException(nameof(initializers));
            }

            await initializers.ParallelForEachAsync(
                (i, ct) => i.InitializeAsync(ct), 
                cancellationToken);
        }

        public static async Task InitializeAllAsyncWith(
            this IEnumerable<Assembly> assemblies, 
            Func<Type, IAsyncInitializer>? activator,
            CancellationToken cancellationToken)
        {
            if (assemblies is null)
            {
                throw new System.ArgumentNullException(nameof(assemblies));
            }

            await assemblies
                .ExportedTypes()
                .ConcreteClassesImplementing<IAsyncInitializer>()
                .ActivateAs(activator)
                .InitializeAsync(cancellationToken);
        }

        public static Task InitializeAllAsync(
            this IEnumerable<Assembly> assemblies,
            CancellationToken cancellationToken) =>
            assemblies.InitializeAllAsyncWith(activator: null, cancellationToken);
    }
}
