using System;
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
		BusinessProcessWaitResult WaitForLifeCycles(string processId, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds, TimeSpan? timeout = null);

		/// <summary>Waits until the business process transitioned into one of the given lifecycles or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="lifeCycleIds">The business process lifecycle IDs to wait for.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		Task<BusinessProcessWaitResult> WaitForLifeCyclesAsync(string processId, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>Waits until the business process transitioned into one of the given states or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="states">The states to wait for.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		BusinessProcessWaitResult WaitForStates(string processId, IEnumerable<string> states, TimeSpan? timeout = null);

		/// <summary>Waits until the business process transitioned into one of the given states or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="states">The states to wait for.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		Task<BusinessProcessWaitResult> WaitForStatesAsync(string processId, IEnumerable<string> states, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>Waits until the business process is completed or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		BusinessProcessWaitResult WaitForCompletion(string processId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>Waits until the business process is completed or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		Task<BusinessProcessWaitResult> WaitForCompletionAsync(string processId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));
	}
}
