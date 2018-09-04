using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1
{
    public class PictureparkRetryHandler : DelegatingHandler
    {
        private readonly int _maxRetries;

        public PictureparkRetryHandler(HttpMessageHandler innerHandler, int maxRetries = 3)
            : base(innerHandler)
        {
            _maxRetries = maxRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage msg = null;
            for (int i = 0; i < _maxRetries; i++)
            {
                msg = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (msg.StatusCode != (HttpStatusCode)429)
                {
                    return msg;
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, i));
                if (int.TryParse(msg.ReasonPhrase, out int waitSeconds))
                {
                    delay = delay.Add(TimeSpan.FromSeconds(waitSeconds));
                }

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            // return the message w/statuscode 429 here, will be converted to an ApiException by the generated clients
            return msg;
        }
    }
}