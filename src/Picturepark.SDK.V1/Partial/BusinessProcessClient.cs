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
		/// <summary>Waits until the business process transitioned into one of the given lifecycles or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="lifeCycleIds">The business process lifecycle IDs to wait for.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		public BusinessProcessWaitResult WaitForLifeCycles(string processId, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds, int? timeout = null)
		{
			return Task.Run(async () => await WaitForLifeCyclesAsync(processId, lifeCycleIds, timeout, CancellationToken.None)).GetAwaiter().GetResult();
		}

		/// <summary>Waits until the business process transitioned into one of the given lifecycles or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="lifeCycleIds">The business process lifecycle IDs to wait for.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		public async Task<BusinessProcessWaitResult> WaitForLifeCyclesAsync(string processId, IEnumerable<BusinessProcessLifeCycle> lifeCycleIds, int? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var waitResult = await WaitCoreAsync(processId, null, lifeCycleIds, timeout, cancellationToken);

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

		/// <summary>Waits until the business process transitioned into one of the given states or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="states">The states to wait for.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		public BusinessProcessWaitResult WaitForStates(string processId, IEnumerable<string> states, int? timeout = null)
		{
			return Task.Run(async () => await WaitForStatesAsync(processId, states, timeout, CancellationToken.None)).GetAwaiter().GetResult();
		}

		/// <summary>Waits until the business process transitioned into one of the given states or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="states">The states to wait for.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		public async Task<BusinessProcessWaitResult> WaitForStatesAsync(string processId, IEnumerable<string> states, int? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var waitResult = await WaitCoreAsync(processId, states, null, timeout, cancellationToken);

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

		/// <summary>Waits until the business process is completed.</summary>
		/// <param name="processId">The process ID.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		public BusinessProcessWaitResult WaitForCompletion(string processId)
		{
			return Task.Run(async () => await WaitForCompletionCoreAsync(processId, 60 * 1000, CancellationToken.None)).GetAwaiter().GetResult();
		}

		/// <summary>Waits until the business process is completed or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		public BusinessProcessWaitResult WaitForCompletion(string processId, int timeout)
		{
			return Task.Run(async () => await WaitForCompletionCoreAsync(processId, timeout, CancellationToken.None)).GetAwaiter().GetResult();
		}

		/// <summary>Waits until the business process is completed or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <returns>The wait result.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">Internal server error</exception>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		public async Task<BusinessProcessWaitResult> WaitForCompletionAsync(string processId, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await WaitForCompletionAsync(processId, 60 * 1000, cancellationToken);
		}

		/// <summary>Waits until the business process is completed or the timeout is reached.</summary>
		/// <param name="processId">The process ID.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <returns>The wait result.</returns>
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
