using Picturepark.SDK.V1.ServiceProvider.Buffer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client.Exceptions;

namespace Picturepark.SDK.V1.ServiceProvider
{
    public class ServiceProviderClient : IAsyncDisposable
    {
        private const string DefaultExchangeName = "pp.livestream";
        private const string DefaultQueueName = "pp.livestream.messages";

        private readonly Configuration _configuration;
        private readonly ConnectionFactory _factory;
        private readonly LiveStreamBuffer _liveStreamBuffer;

        private IConnection _connection;
        private IChannel _liveStreamChannel;
        private IChannel _requestMessageChannel;

        private LiveStreamConsumer _liveStreamConsumer;

        public ServiceProviderClient(Configuration configuration)
        {
            _configuration = configuration;

            _factory = CreateConnectionFactory(configuration);

            _liveStreamBuffer = new LiveStreamBuffer(new LiveStreamBufferPriorityQueue());
            _liveStreamBuffer.Start();
        }

        public async ValueTask DisposeAsync()
        {
            await _liveStreamChannel.CloseAsync();
            await _requestMessageChannel.CloseAsync();
            _liveStreamBuffer.Stop();
            await _connection.CloseAsync();
        }

        public async Task<IObservable<EventPattern<EventArgsLiveStreamMessage>>> GetLiveStreamObserver(
            int bufferSize = 0, int delayMilliseconds = 0, CancellationToken cancellationToken = default)
        {
            // buffer
            _liveStreamBuffer.BufferHoldBackTimeMilliseconds = delayMilliseconds;

            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            _liveStreamChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            _requestMessageChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _liveStreamChannel.BasicQosAsync(0, (ushort)bufferSize, false, cancellationToken);

            // create observable
            var result = Observable.FromEventPattern<EventArgsLiveStreamMessage>(
                ev => _liveStreamBuffer.BufferedReceive += ev,
                ev => _liveStreamBuffer.BufferedReceive -= ev
            );

            // create consumer for RabbitMQ events
            _liveStreamConsumer = new LiveStreamConsumer(_configuration, _liveStreamChannel);
            _liveStreamConsumer.Received += (_, e) => { _liveStreamBuffer.Enqueue(e); };

            // consumer
            var consumer = new AsyncEventingBasicConsumer(_requestMessageChannel);
            consumer.ReceivedAsync += (o, e) =>
            {
                _liveStreamConsumer.OnReceived(o, e);
                return Task.CompletedTask;
            };
            await _liveStreamChannel.BasicConsumeAsync(queue: DefaultQueueName, autoAck: false, consumer: consumer, cancellationToken);

            return result;
        }

        private ConnectionFactory CreateConnectionFactory(Configuration configuration)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration.Host,
                Port = int.Parse(configuration.Port),
                UserName = configuration.User,
                Password = configuration.Password,
                AutomaticRecoveryEnabled = true,
                VirtualHost = configuration.ServiceProviderId,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            if (_configuration.UseSsl)
            {
                factory.Ssl = new SslOption
                {
                    Version = SslProtocols.Tls12,
                    Enabled = true,
                    ServerName = factory.HostName,
                    AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
                                             SslPolicyErrors.RemoteCertificateChainErrors
                };
            }

            return factory;
        }
    }
}