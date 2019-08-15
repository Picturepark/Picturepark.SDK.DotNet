using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Picturepark.SDK.V1.Contract;
using Picturepark.ServiceProvider.Example.BusinessProcess.BusinessProcess;
using Picturepark.ServiceProvider.Example.BusinessProcess.Config;
using Picturepark.ServiceProvider.Example.BusinessProcess.Util;

namespace Picturepark.ServiceProvider.Example.BusinessProcess
{
    internal class ContentBatchDownloadService : IHostedService, IDisposable
    {
        private readonly ILogger<ContentBatchDownloadService> _logger;
        private readonly ContentIdQueue _queue;
        private readonly IOptions<SampleConfiguration> _config;
        private readonly Func<IPictureparkService> _clientFactory;
        private readonly IBusinessProcessCancellationManager _cancellationManager;
        private readonly CancellationTokenSource _taskCancellationTokenSource;
        private readonly BlockingCollection<string[]> _batches = new BlockingCollection<string[]>();

        private Task _batchingTask;
        private Task _downloadTask;

        public ContentBatchDownloadService(
            ILogger<ContentBatchDownloadService> logger,
            ContentIdQueue queue,
            IOptions<SampleConfiguration> config,
            Func<IPictureparkService> clientFactory,
            IBusinessProcessCancellationManager cancellationManager)
        {
            _logger = logger;
            _queue = queue;
            _config = config;
            _clientFactory = clientFactory;
            _cancellationManager = cancellationManager;
            _taskCancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _batchingTask = Task.Run(GatherBatches, CancellationToken.None).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                        throw t.Exception;
                },
                CancellationToken.None);

            _downloadTask = Task.Run(DownloadBatches, CancellationToken.None).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                        throw t.Exception;
                },
                CancellationToken.None);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _taskCancellationTokenSource.Cancel();

            var batchingTask = _batchingTask ?? Task.CompletedTask;
            var downloadTask = _downloadTask ?? Task.CompletedTask;

            await Task.WhenAll(batchingTask, downloadTask).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _queue?.Dispose();
            _batchingTask?.Dispose();
            _taskCancellationTokenSource?.Dispose();
        }

        private void GatherBatches()
        {
            while (!_taskCancellationTokenSource.IsCancellationRequested)
            {
                _logger.LogInformation("Waiting for work...");

                using (var ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(_taskCancellationTokenSource.Token))
                {
                    ctsTimeout.CancelAfter(_config.Value.InactivityTimeout);

                    var batch = new List<string>();
                    while (batch.Count < _config.Value.BatchSize && !ctsTimeout.IsCancellationRequested)
                    {
                        if (_queue.TryTake(out string contentId, _config.Value.InactivityTimeout))
                            batch.Add(contentId);
                    }

                    if (ctsTimeout.IsCancellationRequested)
                        _logger.LogInformation("Inactivity period elapsed");

                    if (batch.Count > 0)
                    {
                        _logger.LogInformation($"Got {batch.Count} content ids to download original");
                        _batches.Add(batch.ToArray(), CancellationToken.None);
                    }
                }
            }
        }

        private async Task DownloadBatches()
        {
            try
            {
                while (_batches.TryTake(out var batch, -1, _taskCancellationTokenSource.Token))
                {
                    _logger.LogInformation($"Downloading batch consisting of {batch.Length} items");

                    TranslatedStringDictionary GetTitle(string state) => new TranslatedStringDictionary { { "en", $"Content carbon copy {state}" } };
                    TranslatedStringDictionary GetProgress(int n) => new TranslatedStringDictionary { { "en", $"Downloaded {n}/{batch.Length} contents" } };

                    var client = _clientFactory();

                    var businessProcess = await client.BusinessProcess.CreateAsync(
                        new BusinessProcessCreateRequest
                        {
                            SupportsCancellation = true,
                            InitialState = "InProgress",
                            Notification = new BusinessProcessNotificationUpdate
                            {
                                EventType = NotificationEventType.InProgress,
                                Title = GetTitle("in progress"),
                                Message = GetProgress(0)
                            }
                        }).ConfigureAwait(false);

                    if (!Directory.Exists(_config.Value.OutputDownloadDirectory))
                        Directory.CreateDirectory(_config.Value.OutputDownloadDirectory);

                    var done = 0;
                    var cancelled = false;

                    foreach (var contentId in batch)
                    {
                        if (_cancellationManager.IsCancelled(businessProcess.Id))
                        {
                            cancelled = true;
                            break;
                        }

                        var response = await client.Content.DownloadAsync(contentId, "Original").ConfigureAwait(false);
                        using (var fs = new FileStream(Path.Combine(_config.Value.OutputDownloadDirectory, contentId + ".jpg"), FileMode.CreateNew))
                        {
                            await response.Stream.CopyToAsync(fs).ConfigureAwait(false);
                        }

                        await client.BusinessProcess.UpdateNotificationAsync(
                            businessProcess.Id,
                            new BusinessProcessNotificationUpdateRequest
                            {
                                EventType = NotificationEventType.InProgress,
                                Title = GetTitle("in progress"),
                                Message = GetProgress(++done)
                            }).ConfigureAwait(false);

                        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false); // make task artificially slower to demonstrate cancellation in the UI
                    }

                    await client.BusinessProcess.ChangeStateAsync(
                        businessProcess.Id,
                        new BusinessProcessStateChangeRequest
                        {
                            LifeCycle = cancelled ? BusinessProcessLifeCycle.Cancelled : BusinessProcessLifeCycle.Succeeded,
                            State = cancelled ? "Cancelled" : "Finished",
                            Notification = new BusinessProcessNotificationUpdate
                            {
                                EventType = cancelled ? NotificationEventType.Warning : NotificationEventType.Success,
                                Title = GetTitle(cancelled ? "cancelled" : "finished"),
                                Message = GetProgress(done)
                            }
                        }).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }
    }
}