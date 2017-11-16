using System;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class BusinessProcessExtensions
	{
		public static async Task<BusinessProcessWaitResult> WaitForCompletionAsync(this BusinessProcess process, IBusinessProcessClient businessProcessesClient)
		{
			var waitResult = await businessProcessesClient.WaitForCompletionAsync(process.Id, 60 * 1000);

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

			throw new InvalidOperationException("The state has not hit but no error could be found."); // TODO: Is this a timeout exception?
		}
	}
}