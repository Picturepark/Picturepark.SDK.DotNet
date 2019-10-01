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
        /// <inheritdoc />
        public async Task<BusinessProcessWaitForLifeCycleResult> WaitForLifeCyclesAsync(string processId, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _httpClient.Poll(timeout, cancellationToken, async () =>
            {
                var waitResult = await WaitForLifeCyclesCoreAsync(processId, lifeCycleIds, timeout, cancellationToken).ConfigureAwait(false);

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
            }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<BusinessProcessWaitForStateResult> WaitForStatesAsync(string processId, IEnumerable<string> states, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _httpClient.Poll(timeout, cancellationToken, async () =>
            {
                var waitResult = await WaitForStatesCoreAsync(processId, states, timeout, cancellationToken).ConfigureAwait(false);

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
            }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<BusinessProcessWaitForLifeCycleResult> WaitForCompletionAsync(string processId, TimeSpan? timeout = null, bool waitForContinuationCompletion = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _httpClient.Poll(timeout, cancellationToken, async () =>
            {
                var waitResult = await WaitForCompletionCoreAsync(processId, timeout, waitForContinuationCompletion, cancellationToken).ConfigureAwait(false);
                if (waitResult.BusinessProcess.LifeCycle == BusinessProcessLifeCycle.Succeeded ||
                    waitResult.BusinessProcess.LifeCycle == BusinessProcessLifeCycle.SucceededWithErrors)
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

                return waitResult;
            }).ConfigureAwait(false);
        }
    }
}
