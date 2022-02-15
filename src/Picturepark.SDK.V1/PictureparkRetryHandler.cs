using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1
{
    /// <summary>
    /// Decorating <see cref="HttpClientHandler"/> which retries calls when HTTP 429 (Too many requests) is returned
    /// </summary>
    public class PictureparkRetryHandler : DelegatingHandler
    {
        private readonly int _maxRetries;

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkRetryHandler"/> class to handle throttled requests (HTTP 429 Too many requests).
        /// Uses the default <see cref="HttpClientHandler"/> inner handler.
        /// </summary>
        /// <param name="maxRetries">Maximum number of retries before a request is failing.</param>
        public PictureparkRetryHandler(int maxRetries = 3) : this(new HttpClientHandler(), maxRetries)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkRetryHandler"/> class to handle throttled requests (HTTP 429 Too many requests).
        /// Adds an inner handler for further processing.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="maxRetries">Maximum number of retries before a request is failing.</param>
        public PictureparkRetryHandler(HttpMessageHandler innerHandler, int maxRetries = 3)
            : base(innerHandler)
        {
            if (maxRetries < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRetries), maxRetries, "MaxRetries should be 0 or a positive integer");

            _maxRetries = maxRetries;
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// Retries the request up to the specified maximum retries when encountering a response with the status code 429.
        /// </summary>
        /// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.</returns>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request" /> was null.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage msg = null;
            for (var i = 0; i < _maxRetries + 1; i++)
            {
                msg = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (msg.StatusCode != (HttpStatusCode)429)
                    return msg;

                var delay = TimeSpan.FromSeconds(Math.Pow(2, i));
                if (msg.Headers.RetryAfter?.Delta != null)
                    delay = delay.Add(msg.Headers.RetryAfter.Delta.Value);
                else if (msg.Headers.RetryAfter?.Date != null)
                    delay = msg.Headers.RetryAfter.Date.Value - DateTime.Now;

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            // return the message w/statuscode 429 here, will be converted to an ApiException by the generated clients
            return msg;
        }
    }
}