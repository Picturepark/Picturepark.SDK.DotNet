using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class BusinessProcessExtensions
	{
		public static async Task<BusinessProcessWaitResult> WaitForCompletionAsync(this BusinessProcess process, IBusinessProcessClient businessProcessesClient, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await WaitForCompletionAsync(process, businessProcessesClient, 60 * 1000, cancellationToken);
		}

		public static async Task<BusinessProcessWaitResult> WaitForCompletionAsync(this BusinessProcess process, IBusinessProcessClient businessProcessesClient, int timeout, CancellationToken cancellationToken = default(CancellationToken))
		{
			var waitResult = await businessProcessesClient.WaitForCompletionAsync(process.Id, timeout, cancellationToken);

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