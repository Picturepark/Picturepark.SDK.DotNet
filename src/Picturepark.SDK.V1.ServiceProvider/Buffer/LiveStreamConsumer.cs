using Picturepark.SDK.V1.Contract;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Picturepark.SDK.V1.ServiceProvider.Buffer
{
    public class LiveStreamConsumer
    {
        private readonly Configuration _configuration;
        private readonly IChannel _channel;

        public LiveStreamConsumer(Configuration configuration, IChannel channel)
        {
            _configuration = configuration;
            _channel = channel;
        }

        public event EventHandler<EventArgsLiveStreamMessage> Received;

        public void OnReceived(object sender, BasicDeliverEventArgs e)
        {
            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<LiveStreamMessage>(
                Encoding.UTF8.GetString(e.Body.ToArray()),
                _configuration.SerializerSettings
            );

            var bufferMessage = new EventArgsLiveStreamMessage
            {
                Created = message.Timestamp,
                Received = DateTime.Now,
                Message = message,
                Channel = _channel,
                Tag = e.DeliveryTag
            };

            Received?.Invoke(this, bufferMessage);
        }
    }
}
