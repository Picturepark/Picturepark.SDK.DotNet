using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Picturepark.SDK.V1
{
    public partial class BusinessProcessClient
    {
        /// <summary>Waits until the business process transitioned into one of the given lifecycles or the timeout is reached.</summary>
        /// <param name="processId">The process ID.</param>
        /// <param name="lifeCycleIds">The business process lifecycle IDs to wait for.</param>
        /// <param name="timeout">The timeout in ms to wait for completion.</param>
        /// <returns>The wait result.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<BusinessProcessWaitResult> WaitForLifeCyclesAsync(string processId, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _httpClient.Poll(timeout, cancellationToken, async () =>
            {
                var waitResult = await WaitCoreAsync(processId, null, lifeCycleIds, timeout, cancellationToken).ConfigureAwait(false);

                var errors = waitResult.BusinessProcess.StateHistory?
                    .Where(i => i.Error != null)
                    .Select(i => i.Error)
                    .ToList();

                if (errors != null && errors.Any())
                {
                    if (errors.Count == 1)
                    {
                        throw JsonConvert.DeserializeObject<PictureparkException>(errors.First().Exception, JsonSerializerSettings);
                    }

                    var exceptions = errors.Select(error => JsonConvert.DeserializeObject<PictureparkException>(error.Exception, JsonSerializerSettings));
                    throw new AggregateException(exceptions);
                }

                return waitResult;
            });
        }

        /// <summary>Waits until the business process transitioned into one of the given states or the timeout is reached.</summary>
        /// <param name="processId">The process ID.</param>
        /// <param name="states">The states to wait for.</param>
        /// <param name="timeout">The timeout in ms to wait for completion.</param>
        /// <returns>The wait result.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<BusinessProcessWaitResult> WaitForStatesAsync(string processId, IEnumerable<string> states, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _httpClient.Poll(timeout, cancellationToken, async () =>
            {
                var waitResult = await WaitCoreAsync(processId, states, null, timeout, cancellationToken).ConfigureAwait(false);

                var errors = waitResult.BusinessProcess.StateHistory?
                    .Where(i => i.Error != null)
                    .Select(i => i.Error)
                    .ToList();

                if (errors != null && errors.Any())
                {
                    if (errors.Count == 1)
                    {
                        throw JsonConvert.DeserializeObject<PictureparkException>(errors.First().Exception, JsonSerializerSettings);
                    }

                    var exceptions = errors.Select(error => JsonConvert.DeserializeObject<PictureparkException>(error.Exception, JsonSerializerSettings));
                    throw new AggregateException(exceptions);
                }

                return waitResult;
            });
        }

        /// <summary>Waits until the business process is completed or the timeout is reached.</summary>
        /// <param name="processId">The process ID.</param>
        /// <param name="timeout">The timeout wait for completion.</param>
        /// <returns>The wait result.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<BusinessProcessWaitResult> WaitForCompletionAsync(string processId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _httpClient.Poll(timeout, cancellationToken, async () =>
            {
                var waitResult = await WaitForCompletionCoreAsync(processId, timeout, cancellationToken).ConfigureAwait(false);
                if (waitResult.HasLifeCycleHit && (waitResult.BusinessProcess.LifeCycle == BusinessProcessLifeCycle.Succeeded ||
                                                   waitResult.BusinessProcess.LifeCycle == BusinessProcessLifeCycle.SucceededWithErrors))
                    return waitResult;

                var errors = waitResult.BusinessProcess.StateHistory?
                    .Where(i => i.Error != null)
                    .Select(i => i.Error)
                    .ToList();

                if (errors != null && errors.Any())
                {
                    if (errors.Count == 1)
                    {
                        throw JsonConvert.DeserializeObject<PictureparkException>(errors.First().Exception, JsonSerializerSettings);
                    }

                    var exceptions = errors.Select(error => JsonConvert.DeserializeObject<PictureparkException>(error.Exception, JsonSerializerSettings));
                    throw new AggregateException(exceptions);
                }

                throw new TimeoutException($"Wait for business process on completion timed out after {timeout?.TotalSeconds} seconds");
            });
        }
    }
}
