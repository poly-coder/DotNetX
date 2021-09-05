using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX
{
    public class DelegateReliableAsyncService<TService> :
        ReliableAsyncService<TService>
    {
        private readonly Func<CancellationToken, Task<TService>> createService;
        private readonly Func<Exception, bool> isInvalidatingError;

        public DelegateReliableAsyncService(
            Func<CancellationToken, Task<TService>> createService,
            Func<Exception, bool>? isInvalidatingError = null)
        {
            this.createService = createService ?? throw new ArgumentNullException(nameof(createService));
            this.isInvalidatingError = isInvalidatingError ?? (ex => base.IsInvalidatingError(ex));
        }

        protected override Task<TService> CreateService(CancellationToken cancellationToken) =>
            createService(cancellationToken);

        public override bool IsInvalidatingError(Exception exception)
        {
            return isInvalidatingError(exception);
        }
    }
}