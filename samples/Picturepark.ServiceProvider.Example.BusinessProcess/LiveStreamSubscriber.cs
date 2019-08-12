using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Picturepark.SDK.V1.ServiceProvider;
using Picturepark.SDK.V1.ServiceProvider.Buffer;

namespace Picturepark.ServiceProvider.Example.BusinessProcess
{
    public class LiveStreamSubscriber : IHostedService, IDisposable
    {
        private readonly ILogger<LiveStreamSubscriber> _logger;
        private readonly IApplicationEventHandlerFactory _eventHandlerFactory;
        private readonly Configuration _serviceProviderConfiguration;

        private ServiceProviderClient _client;
        private IDisposable _subscription;

        public LiveStreamSubscriber(ILogger<LiveStreamSubscriber> logger, IOptions<SampleConfiguration> config, IApplicationEventHandlerFactory eventHandlerFactory)
        {
            _logger = logger;
            _eventHandlerFactory = eventHandlerFactory;

            _serviceProviderConfiguration = new Picturepark.SDK.V1.ServiceProvider.Configuration
            {
                Host = config.Value.IntegrationHost,
                Port = config.Value.IntegrationPort.ToString(),
                NodeId = Environment.MachineName,
                ServiceProviderId = config.Value.ServiceProviderId,
                User = config.Value.ServiceProviderId,
                Password = config.Value.Secret,
                UseSsl = config.Value.UseSsl
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Subscribing to live stream");
            _client = new ServiceProviderClient(_serviceProviderConfiguration);
            _subscription = _client.GetLiveStreamObserver().Subscribe(OnLiveStreamEvent);

            return Task.CompletedTask;
        }

        private void OnLiveStreamEvent(EventPattern<EventArgsLiveStreamMessage> e)
        {
            var applicationEvent = e.EventArgs.Message.ApplicationEvent;

            if (applicationEvent != null)
            {
                var handler = _eventHandlerFactory.Get(applicationEvent);
                handler?.Handle(applicationEvent);

                e.EventArgs.Ack();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _client?.Dispose();
            _subscription?.Dispose();
        }
    }
}