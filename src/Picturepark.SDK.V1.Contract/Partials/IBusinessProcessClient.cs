﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IBusinessProcessClient
    {
        /// <summary>Waits until the business process transitioned into one of the given lifecycles or the timeout is reached.</summary>
        /// <param name="processId">The process ID.</param>
        /// <param name="lifeCycleIds">The business process lifecycle IDs to wait for.</param>
        /// <param name="timeout">The timeout in ms to wait for completion.</param>
        /// <returns>The wait result.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        Task<BusinessProcessWaitForLifeCycleResult> WaitForLifeCyclesAsync(string processId, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Waits until the business process transitioned into one of the given states or the timeout is reached.</summary>
        /// <param name="processId">The process ID.</param>
        /// <param name="states">The states to wait for.</param>
        /// <param name="timeout">The timeout in ms to wait for completion.</param>
        /// <returns>The wait result.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        Task<BusinessProcessWaitForStateResult> WaitForStatesAsync(string processId, IEnumerable<string> states, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Waits until the business process is completed or the timeout is reached.</summary>
        /// <param name="processId">The process ID.</param>
        /// <param name="timeout">The timeout in ms to wait for completion.</param>
        /// <returns>The wait result.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <param name="waitForContinuationCompletion">Waits for the completion of the continuation business process (if existing, recursively). Default to true.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        Task<BusinessProcessWaitForLifeCycleResult> WaitForCompletionAsync(string processId, TimeSpan? timeout = null, bool waitForContinuationCompletion = true, CancellationToken cancellationToken = default(CancellationToken));
    }
}
