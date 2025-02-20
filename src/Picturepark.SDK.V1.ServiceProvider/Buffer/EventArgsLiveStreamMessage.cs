using Picturepark.SDK.V1.Contract;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.ServiceProvider.Buffer
{
    public class EventArgsLiveStreamMessage : EventArgs
    {
        public DateTime Created { get; set; }

        public DateTime Received { get; set; }

        public LiveStreamMessage Message { get; set; }

        public IChannel Channel { get; set; }

        public ulong Tag { get; set; }

        public async Task Ack()
        {
            await Channel.BasicAckAsync(Tag, false);
        }

        public async Task Nack(bool requeue = false)
        {
            await Channel.BasicNackAsync(Tag, false, requeue);
        }
    }
}
