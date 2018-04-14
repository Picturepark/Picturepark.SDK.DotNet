using Picturepark.SDK.V1.Contract;
using RabbitMQ.Client;
using System;

namespace Picturepark.SDK.V1.ServiceProvider.Buffer
{
    public class EventArgsLiveStreamMessage : EventArgs
    {
        public DateTime Created { get; set; }

        public DateTime Received { get; set; }

        public LiveStreamMessage Message { get; set; }

        public IModel Model { get; set; }

        public ulong Tag { get; set; }

        public void Ack()
        {
            Model.BasicAck(Tag, false);
        }

        public void Nack(bool requeue = false)
        {
            Model.BasicNack(Tag, false, requeue);
        }
    }
}
