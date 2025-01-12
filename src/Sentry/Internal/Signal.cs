using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.Internal
{
    internal class Signal : IDisposable
    {
        private readonly object _lock = new();
        private readonly SemaphoreSlim _semaphore = new(0, 1);

        public Signal(bool isReleasedInitially = false)
        {
            if (isReleasedInitially)
            {
                Release();
            }
        }

        public void Release()
        {
            // Make sure the semaphore does not go above 1
            lock (_lock)
            {
                if (_semaphore.CurrentCount >= 1)
                {
                    return;
                }

                _semaphore.Release();
            }
        }

        // It's synchronized only on Release, not to go above 1. The type itself is thread-safe.
        // ReSharper disable once InconsistentlySynchronizedField
        public Task WaitAsync(CancellationToken cancellationToken = default) =>
            _semaphore.WaitAsync(cancellationToken);

        public void Dispose() => _semaphore.Dispose();
    }
}
