using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1
{
	public partial class BusinessProcessClient
	{
		// TODO: Complete XML docs here and in IBusinessProcessClient

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <param name="states">The states to wait for</param>
		/// <param name="lifeCycleIds">Business process lifeCycle to wait for</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		public BusinessProcessWaitResult Wait(string processId, IEnumerable<string> states = null, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds = null, int? timeout = null)
		{
			return Task.Run(async () => await WaitAsync(processId, states, lifeCycleIds, timeout, CancellationToken.None)).GetAwaiter().GetResult();
		}

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <param name="states">The states to wait for</param>
		/// <param name="lifeCycleIds">Business process lifeCycle to wait for</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		public async Task<BusinessProcessWaitResult> WaitAsync(string processId, IEnumerable<string> states = null, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds = null, int? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var waitResult = await WaitCoreAsync(processId, states, lifeCycleIds, timeout, cancellationToken);

			var errors = waitResult.BusinessProcess.StateHistory?
				.Where(i => i.Error != null)
				.Select(i => i.Error)
				.ToList();

			if (errors != null && errors.Any())
			{
				if (errors.Count == 1)
				{
					throw PictureparkException.FromJson(errors.First().Exception);
				}

				var exceptions = errors.Select(error => PictureparkException.FromJson(error.Exception));
				throw new AggregateException(exceptions);
			}

			return waitResult;
		}

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		public BusinessProcessWaitResult WaitForCompletion(string processId)
		{
			return Task.Run(async () => await WaitForCompletionCoreAsync(processId, 60 * 1000, CancellationToken.None)).GetAwaiter().GetResult();
		}

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		public BusinessProcessWaitResult WaitForCompletion(string processId, int timeout)
		{
			return Task.Run(async () => await WaitForCompletionCoreAsync(processId, timeout, CancellationToken.None)).GetAwaiter().GetResult();
		}

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		public async Task<BusinessProcessWaitResult> WaitForCompletionAsync(string processId, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await WaitForCompletionAsync(processId, 60 * 1000, cancellationToken);
		}

		/// <summary>Wait</summary>
		/// <param name="processId">The process id</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>BusinessProcessWaitResult</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		public async Task<BusinessProcessWaitResult> WaitForCompletionAsync(string processId, int timeout, CancellationToken cancellationToken = default(CancellationToken))
		{
			var waitResult = await WaitForCompletionCoreAsync(processId, timeout, cancellationToken);

			if (waitResult.HasLifeCycleHit)
				return waitResult;

			var errors = waitResult.BusinessProcess.StateHistory?
				.Where(i => i.Error != null)
				.Select(i => i.Error)
				.ToList();

			if (errors != null && errors.Any())
			{
				if (errors.Count == 1)
				{
					throw PictureparkException.FromJson(errors.First().Exception);
				}

				var exceptions = errors.Select(error => PictureparkException.FromJson(error.Exception));
				throw new AggregateException(exceptions);
			}

			throw new TimeoutException($"Wait for business process on completion timed out after {timeout / 1000} seconds");
		}
	}
}
