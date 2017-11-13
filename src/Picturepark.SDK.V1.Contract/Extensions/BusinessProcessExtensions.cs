using System;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class BusinessProcessExtensions
	{
		public static async Task<BusinessProcessWaitResult> WaitForStateAsync(this BusinessProcess process, string state, IBusinessProcessClient businessProcessesClient)
		{
			return await businessProcessesClient.WaitForStatesAsync(process.Id, state, 60 * 1000);
		}

		public static async Task<BusinessProcessWaitResult> WaitForMetadataAsync(this BusinessProcess process, IBusinessProcessClient businessProcessClient)
		{
			var waitResult = await businessProcessClient.WaitForStatesAsync(process.Id, "Completed", 60 * 1000);
			if (waitResult.HasStateHit == false)
			{
				var exception = waitResult.BusinessProcess.StateHistory.SingleOrDefault(i => i.Error != null);
				if (exception != null)
				{
					throw PictureparkException.FromJson(exception.Error.Exception);
				}
				else
				{
					throw new InvalidOperationException("The state has not hit but no error could be found.");
				}
			}

			return waitResult;
		}
	}
}