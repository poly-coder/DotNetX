using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX
{
    public interface IReliableAsyncService<TService>
    {
        Task<TService> GetServiceAsync(CancellationToken cancellationToken = default);
        
        void Invalidate();

        Task InvalidateAsync(CancellationToken cancellationToken = default);

        bool IsInvalidatingError(Exception exception);

        public async Task<T> WithService<T>(
            Func<TService, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var service = await GetServiceAsync(cancellationToken);

                return await action(service);
            }
            catch (Exception exception)
            {
                if (IsInvalidatingError(exception))
                {
                    await InvalidateAsync(cancellationToken);
                }

                throw;
            }
        }

        public async Task WithService(
            Func<TService, Task> action,
            CancellationToken cancellationToken = default)
        {
            await WithService(
                async container =>
                {
                    await action(container);
                    return true;
                },
                cancellationToken);
        }
    }
}