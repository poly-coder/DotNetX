using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX
{
    public abstract class ReliableAsyncService<TService>
    {
        //private readonly object _locker = new ();
        private readonly SemaphoreSlim _locker = new(1);
        private Guid _serial = Guid.Empty;
        private Task<TService>? _currentTask;

        public void Invalidate()
        {
            _locker.Wait();

            _currentTask = null;
            _serial = Guid.Empty;

            _locker.Release();
        }

        public async Task InvalidateAsync(CancellationToken cancellationToken = default)
        {
            await _locker.WaitAsync(cancellationToken);

            try
            {
                _currentTask = null;
                _serial = Guid.Empty;
            }
            finally
            {
                _locker.Release();
            }
        }

        public async Task<TService> GetServiceAsync(CancellationToken cancellationToken = default)
        {
            var task = _currentTask;
            if (task == null)
            {
                await _locker.WaitAsync(cancellationToken);

                try
                {
                    task = _currentTask;
                    if (task == null)
                    {
                        task = CreateServiceInternal(_serial = Guid.NewGuid(), cancellationToken);
                        _currentTask = task;
                    }
                }
                finally
                {
                    _locker.Release();
                }
            }

            return await task;
        }

        private async Task<TService> CreateServiceInternal(Guid newSerial, CancellationToken cancellationToken)
        {
            try
            {
                return await CreateService(cancellationToken);
            }
            catch
            {
                if (newSerial == _serial)
                {
                    await InvalidateAsync(default);
                }

                throw;
            }
        }

        protected abstract Task<TService> CreateService(CancellationToken cancellationToken);
    }

    public class DelegateReliableAsyncService<TService> : ReliableAsyncService<TService>
    {
        private readonly Func<CancellationToken, Task<TService>> _createService;

        public DelegateReliableAsyncService(Func<CancellationToken, Task<TService>> createService)
        {
            _createService = createService ?? throw new ArgumentNullException(nameof(createService));
        }

        protected override Task<TService> CreateService(CancellationToken cancellationToken) =>
            _createService(cancellationToken);
    }
}