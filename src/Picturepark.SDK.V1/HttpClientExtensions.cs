using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    internal static class HttpClientExtensions
    {
        public static async Task<T> Poll<T>(this HttpClient httpClient, TimeSpan? timeout, CancellationToken cancellationToken, Func<Task<T>> execute)
        {
            while (true)
            {
                try
                {
                    return await execute();
                }
                catch (ApiException ex)
                {
                    if (ex.StatusCode != (int)HttpStatusCode.GatewayTimeout)
                        throw;
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    var finished = false;

                    if (timeout != null)
                    {
                        var newTimeout = timeout.Value.Subtract(httpClient.Timeout);
                        if (newTimeout.TotalMilliseconds <= 0)
                        {
                            finished = true;
                        }
                    }
                    else
                    {
                        finished = true;
                    }

                    if (finished)
                        throw;
                }
            }
        }
    }
}
