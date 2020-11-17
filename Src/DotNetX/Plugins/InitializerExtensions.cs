using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Plugins
{
    public static class InitializerExtensions
    {
        public static void Initialize<T>(this IEnumerable<IInitializer<T>> initializers, T context)
        {
            if (initializers is null)
            {
                throw new System.ArgumentNullException(nameof(initializers));
            }

            initializers.ForEach(i => i.Initialize(context));
        }

        public static async Task InitializeAsync<T>(
            this IEnumerable<IAsyncInitializer<T>> initializers, 
            T context, 
            CancellationToken cancellationToken)
        {
            if (initializers is null)
            {
                throw new System.ArgumentNullException(nameof(initializers));
            }

            await initializers.ForEachAsync(
                (i, ct) => i.InitializeAsync(context, ct), 
                cancellationToken);
        }
    }
}
