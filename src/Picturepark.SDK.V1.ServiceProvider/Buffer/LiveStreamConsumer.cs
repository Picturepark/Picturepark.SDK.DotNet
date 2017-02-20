using Picturepark.API.Contract.V1.LiveStream;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Picturepark.SDK.V1.ServiceProvider.Buffer
{
	public class LiveStreamConsumer
	{
		private readonly Configuration _configuration;
		private readonly IModel _model;

		public LiveStreamConsumer(Configuration configuration, IModel model)
		{
			_configuration = configuration;
			_model = model;
		}

		public event EventHandler<EventArgsLiveStreamMessage> Received;

		public void OnReceived(object sender, BasicDeliverEventArgs e)
		{
			LiveStreamMessage message = Newtonsoft.Json.JsonConvert.DeserializeObject<LiveStreamMessage>(
				Encoding.UTF8.GetString(e.Body),
				_configuration.SerializerSettings
			);

			var bufferMessage = new EventArgsLiveStreamMessage()
			{
				Created = message.Timestamp,
				Received = DateTime.Now,
				Message = message,
				Model = _model,
				Tag = e.DeliveryTag
			};

			Received?.Invoke(this, bufferMessage);
		}
	}
}
