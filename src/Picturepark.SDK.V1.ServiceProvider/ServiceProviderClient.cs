using Picturepark.SDK.V1.ServiceProvider.Buffer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.ServiceProvider.Contract;

namespace Picturepark.SDK.V1.ServiceProvider
{
	public class ServiceProviderClient : IDisposable
	{
		private readonly Configuration _configuration;

		private readonly IConnection _connection;
		private readonly IModel _liveStreamModel;
		private readonly IModel _requestMessageModel;
		private readonly LiveStreamBuffer _liveStreamBuffer;

		private readonly Dictionary<string, ServiceProviderRestClient> _serviceProviderCache;

		private LiveStreamConsumer _liveStreamConsumer;

		public ServiceProviderClient(Configuration configuration)
		{
			_configuration = configuration;
			_serviceProviderCache = new Dictionary<string, ServiceProviderRestClient>();

			ConnectionFactory factory = new ConnectionFactory();

			factory.Uri = $"amqp://{configuration.User}:{configuration.Password}@{configuration.Host}:{configuration.Port}";
			factory.AutomaticRecoveryEnabled = true;
			factory.VirtualHost = configuration.ServiceProviderId;
			factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);

			_connection = factory.CreateConnection();
			_liveStreamModel = _connection.CreateModel();
			_requestMessageModel = _connection.CreateModel();

			_liveStreamBuffer = new LiveStreamBuffer(new LiveStreamBufferPriorityQueue());
			_liveStreamBuffer.Start();
		}

		public event EventHandler<ServiceProviderRequestEventArgs> ServiceProviderRequestEvent;

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

			// exchange
			var exchangeName = "pp.livestream";
			_liveStreamModel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);

			var args = new Dictionary<string, object>();
			args.Add("x-max-priority", _configuration.DefaultQueuePriorityMax);

			// queue
			var queueName = _liveStreamModel.QueueDeclare($"{exchangeName}.{_configuration.NodeId}", true, false, false, args);
			_liveStreamModel.QueueBind(queueName, exchangeName, string.Empty, null);
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
			_liveStreamModel.BasicConsume(queue: queueName, noAck: false, consumer: consumer);

			return result;
		}

		public IObservable<EventPattern<ServiceProviderRequestEventArgs>> GetRequestObserver()
		{
			// exchange
			var exchangeName = "pp.request";
			_requestMessageModel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

			var args = new Dictionary<string, object>();
			args.Add("x-max-priority", _configuration.DefaultQueuePriorityMax);

			// queue
			var queueName = _requestMessageModel.QueueDeclare($"{exchangeName}", true, false, false, args);
			_requestMessageModel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: null);

			// consumer
			var consumer = new EventingBasicConsumer(_requestMessageModel);
			consumer.Received += Request_Received;
			_requestMessageModel.BasicConsume(queue: queueName, noAck: false, consumer: consumer);

			// create observable
			return Observable.FromEventPattern<ServiceProviderRequestEventArgs>(
				ev => ServiceProviderRequestEvent += ev,
				ev => ServiceProviderRequestEvent -= ev
			);
		}

		public ServiceProviderRestClient GetConfigurationClient(string baseUrl, string accessToken, string customerAlias)
		{
			// TODO BRO: Lock
			if (_serviceProviderCache.ContainsKey(baseUrl))
			{
				return _serviceProviderCache[baseUrl];
			}
			else
			{
				var client = new ServiceProviderRestClient(new PictureparkClientSettings(new AccessTokenAuthClient(baseUrl, accessToken, customerAlias)));
				client.BaseUrl = baseUrl;

				_serviceProviderCache[baseUrl] = client;
				return client;
			}
		}

		private void Request_Received(object sender, BasicDeliverEventArgs e)
		{
			ServiceProviderMessage message = (ServiceProviderMessage)Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceProviderMessage>(
				Encoding.UTF8.GetString(e.Body),
				_configuration.SerializerSettings
			);

			// process event
			ServiceProviderRequestEvent(this, new ServiceProviderRequestEventArgs(message));

			((EventingBasicConsumer)sender).Model.BasicAck(e.DeliveryTag, false);
		}
	}
}
