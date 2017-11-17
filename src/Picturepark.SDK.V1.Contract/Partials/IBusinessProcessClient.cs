using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IBusinessProcessClient
    {
		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <param name="states">The states to wait for</param>
		/// <param name="lifeCycleIds">Business process lifeCycle to wait for</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		BusinessProcessWaitResult Wait(string processId, IEnumerable<string> states = null, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds = null, int? timeout = null);

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <param name="states">The states to wait for</param>
		/// <param name="lifeCycleIds">Business process lifeCycle to wait for</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		Task<BusinessProcessWaitResult> WaitAsync(string processId, IEnumerable<string> states = null, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds = null, int? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		BusinessProcessWaitResult WaitForCompletion(string processId);

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		Task<BusinessProcessWaitResult> WaitForCompletionAsync(string processId, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		BusinessProcessWaitResult WaitForCompletion(string processId, int timeout);

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		Task<BusinessProcessWaitResult> WaitForCompletionAsync(string processId, int timeout, CancellationToken cancellationToken = default(CancellationToken));
	}
}
