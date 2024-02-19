using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    internal class ConnectionIssuesHandler : DelegatingHandler
    {
        private readonly ConcurrentDictionary<Guid, DateTime> _failedRequests = new ConcurrentDictionary<Guid, DateTime>();

        private readonly int maxFailuresPerWindow = 5;
        private readonly TimeSpan _windowLength = TimeSpan.FromSeconds(60);
        private readonly CancellationTokenSource _requestMonitorCts;

        private volatile bool _failAll;

        public ConnectionIssuesHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _requestMonitorCts = new CancellationTokenSource();

            Task.Run(
                async () =>
                {
                    while (!_requestMonitorCts.IsCancellationRequested)
                    {
                        await Task.Delay(100);

                        // prune request dictionary
                        foreach (var request in _failedRequests.ToArray())
                        {
                            if (request.Value.Add(_windowLength) < DateTime.UtcNow)
                            {
                                _failedRequests.TryRemove(request.Key, out _);
                            }
                        }

                        if (_failedRequests.Count > maxFailuresPerWindow)
                            _failAll = true;
                    }
                });
        }

        protected override void Dispose(bool disposing)
        {
            _requestMonitorCts.Cancel();
            base.Dispose(disposing);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpRequestException exception;

            do
            {
                try
                {
                    return await base.SendAsync(request, cancellationToken);
                }
                catch (HttpRequestException ex)
                {
                    if (_failAll)
                        throw;

                    exception = ex;
                    _failedRequests.AddOrUpdate(Guid.NewGuid(), DateTime.UtcNow, (id, ts) => DateTime.UtcNow);
                }

                // back off a bit
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            while (!_failAll);

            throw exception;
        }
    }
}