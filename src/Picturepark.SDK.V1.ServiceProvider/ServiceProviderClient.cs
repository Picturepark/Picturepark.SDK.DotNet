using Picturepark.SDK.V1.ServiceProvider.Buffer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Net.Security;
using System.Security.Authentication;

namespace Picturepark.SDK.V1.ServiceProvider
{
    public class ServiceProviderClient : IDisposable
    {
        private const string ExchangeName = "pp.livestream";

        private readonly Configuration _configuration;
        private readonly ConnectionFactory _factory;
        private readonly LiveStreamBuffer _liveStreamBuffer;

        private IConnection _connection;
        private IModel _liveStreamModel;
        private IModel _requestMessageModel;

        private LiveStreamConsumer _liveStreamConsumer;

        public ServiceProviderClient(Configuration configuration)
        {
            _configuration = configuration;

            _factory = CreateConnectionFactory(configuration);

            _connection = _factory.CreateConnection();
            _liveStreamModel = _connection.CreateModel();
            _requestMessageModel = _connection.CreateModel();

            _liveStreamBuffer = new LiveStreamBuffer(new LiveStreamBufferPriorityQueue());
            _liveStreamBuffer.Start();
        }

        public void Dispose()
        {
            ////_liveStreamConsumer.Stop();
            _liveStreamModel.Close();
            _requestMessageModel.Close();
            _liveStreamBuffer.Stop();
            _connection.Close();
        }

        public IObservable<EventPattern<EventArgsLiveStreamMessage>> GetLiveStreamObserver(int bufferSize = 0, int delayMilliseconds = 0)
        {
            // buffer
            _liveStreamBuffer.BufferHoldBackTimeMilliseconds = delayMilliseconds;

            var queueName = $"{ExchangeName}.{_configuration.NodeId}";
            var isOldStyleProvider = TryDeclareExchangeQueue(queueName);
            if (isOldStyleProvider)
            {
                _connection = _factory.CreateConnection();
                _liveStreamModel = _connection.CreateModel();
                _requestMessageModel = _connection.CreateModel();
            }

            _liveStreamModel.BasicQos(0, (ushort)bufferSize, false);

            // create observable
            var result = Observable.FromEventPattern<EventArgsLiveStreamMessage>(
                ev => _liveStreamBuffer.BufferedReceive += ev,
                ev => _liveStreamBuffer.BufferedReceive -= ev
            );

            // create consumer for RabbitMQ events
            _liveStreamConsumer = new LiveStreamConsumer(_configuration, _liveStreamModel);
            _liveStreamConsumer.Received += (o, e) => { _liveStreamBuffer.Enqueue(e); };

            // consumer
            var consumer = new EventingBasicConsumer(_requestMessageModel);
            consumer.Received += (o, e) => { _liveStreamConsumer.OnReceived(o, e); };
            _liveStreamModel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            return result;
        }

        private bool TryDeclareExchangeQueue(string queueName)
        {
            try
            {
                _liveStreamModel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);

                var args = new Dictionary<string, object>
                    { { "x-max-priority", _configuration.DefaultQueuePriorityMax } };

                // queue
                var queueDeclareOk = _liveStreamModel.QueueDeclare(queueName, true, false, false, args);
                _liveStreamModel.QueueBind(queueDeclareOk, ExchangeName, string.Empty, null);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
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
                factory.Ssl = new SslOption()
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
