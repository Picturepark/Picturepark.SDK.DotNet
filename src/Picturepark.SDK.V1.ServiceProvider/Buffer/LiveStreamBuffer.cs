using System;
using System.Linq;
using System.Threading;

namespace Picturepark.SDK.V1.ServiceProvider.Buffer
{
    public class LiveStreamBuffer
    {
        private readonly AutoResetEvent _hasNewMessage = new AutoResetEvent(false);
        private Thread _thread;
        private volatile bool _keepRunning = true;

        public LiveStreamBuffer(LiveStreamBufferPriorityQueue queue)
        {
            Queue = queue;
        }

        public event EventHandler<EventArgsLiveStreamMessage> BufferedReceive;

        public LiveStreamBufferPriorityQueue Queue { get; set; }

        public int BufferHoldBackTimeMilliseconds { get; set; } = 3000;

        public bool Enqueue(EventArgsLiveStreamMessage message)
        {
            Queue.Enqueue(message, message.Created);

            SignalNewMessage();

            return true;
        }

        public Thread Start()
        {
            _thread = new Thread(ConsumeEntries);

            _thread.Start();

            return _thread;
        }

        public void ConsumeEntries()
        {
            while (_keepRunning)
            {
                // wait for a new message, give air to re-checking the keep running flag
                _hasNewMessage.WaitOne(5000);

                if (Queue.Count <= 0)
                {
                    continue;
                }

                ProcessMessage();

                // just in case we missed on, I don't think this one is really needed
                if (Queue.Count >= 0)
                {
                    _hasNewMessage.Set();
                }
            }
        }

        public void Stop()
        {
            _keepRunning = false;
            _hasNewMessage.Set();
            _thread.Join();
        }

        private void SignalNewMessage()
        {
            _hasNewMessage.Set();
        }

        private void ProcessMessage()
        {
            var first = Queue.First();

            var now = DateTime.Now;

            var delayMilliseconds = (int)(now - first.Received).TotalMilliseconds;

            if (delayMilliseconds < BufferHoldBackTimeMilliseconds)
            {
                Thread.Sleep(BufferHoldBackTimeMilliseconds - delayMilliseconds);
            }

            var bufferMessage = Queue.Dequeue();

            BufferedReceive?.Invoke(this, bufferMessage);

            while (_keepRunning && Queue.Where(s => (DateTime.Now - s.Received).TotalMilliseconds > BufferHoldBackTimeMilliseconds).Count() > 0)
            {
                bufferMessage = Queue.Dequeue();
                BufferedReceive?.Invoke(this, bufferMessage);
            }
        }
    }
}
