using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1
{
    internal static class HttpClientExtensions
    {
        public static async Task<T> Poll<T>(this HttpClient httpClient, TimeSpan? timeout, CancellationToken cancellationToken, Func<Task<T>> execute)
        {
            try
            {
                return await execute();
            }
            catch (TaskCanceledException)
            {
                if (timeout != null)
                {
                    var newTimeout = timeout.Value.Subtract(httpClient.Timeout);
                    if (newTimeout.TotalMilliseconds > 0)
                    {
                        return await Poll(httpClient, newTimeout, cancellationToken, execute);
                    }
                }

                throw;
            }
        }
    }
}
