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
        private readonly Task _initializeTask;

        private IConnection _connection;
        private IChannel _liveStreamChannel;
        private IChannel _requestMessageChannel;

        private LiveStreamConsumer _liveStreamConsumer;

        public ServiceProviderClient(Configuration configuration)
        {
            _configuration = configuration;

            _factory = CreateConnectionFactory(configuration);

            _initializeTask = ConnectAsync(CancellationToken.None);
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

        public async Task<IObservable<EventPattern<EventArgsLiveStreamMessage>>> GetLiveStreamObserver(int bufferSize = 0, int delayMilliseconds = 0, CancellationToken cancellationToken = default)
        {
            await _initializeTask; // maybe better to start the task here instead, and pass the cancellation token

            // buffer
            _liveStreamBuffer.BufferHoldBackTimeMilliseconds = delayMilliseconds;

#pragma warning disable CS0618 // Type or member is obsolete
            var queueName = $"{DefaultExchangeName}.{_configuration.NodeId}";
#pragma warning restore CS0618 // Type or member is obsolete
            var isUnprotectedProvider = await TryDeclareExchangeAndBindQueue(queueName, cancellationToken);
            if (!isUnprotectedProvider)
            {
                await ConnectAsync(cancellationToken);
                queueName = DefaultQueueName;
            }

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
            await _liveStreamChannel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken);

            return result;
        }

        private async Task<bool> TryDeclareExchangeAndBindQueue(string queueName, CancellationToken cancellationToken)
        {
            try
            {
                await _liveStreamChannel.ExchangeDeclareAsync(DefaultExchangeName, ExchangeType.Fanout, cancellationToken: cancellationToken);

                var args = new Dictionary<string, object> { { "x-max-priority", _configuration.DefaultQueuePriorityMax } };

                // queue
                var queueDeclareOk = await _liveStreamChannel.QueueDeclareAsync(queueName, true, false, false, args, cancellationToken: cancellationToken);
                await _liveStreamChannel.QueueBindAsync(queueDeclareOk, DefaultExchangeName, string.Empty, null, cancellationToken: cancellationToken);
                return true;
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 403)
            {
                return false;
            }
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            _liveStreamChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            _requestMessageChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
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
                    AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors
                };
            }

            return factory;
        }
    }
}
