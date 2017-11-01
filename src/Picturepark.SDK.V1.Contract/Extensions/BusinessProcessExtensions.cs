using System;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class BusinessProcessExtensions
	{
		public static async Task<BusinessProcessWaitResult> Wait4StateAsync(this BusinessProcess process, string states, IBusinessProcessClient businessProcessesClient)
		{
			return await businessProcessesClient.WaitForStatesAsync(process.Id, states, 60 * 1000);
		}

		public static async Task<BusinessProcessWaitResult> Wait4MetadataAsync(this BusinessProcess process, IBusinessProcessClient businessProcessClient)
		{
			var waitResult = await businessProcessClient.WaitForStatesAsync(process.Id, "Completed", 60 * 1000);
			if (waitResult.HasStateHit == false)
			{
				// TODO: Deserialize exception
				var exception = waitResult.BusinessProcess.StateHistory.SingleOrDefault(i => i.Error != null);
				throw new Exception(exception.Error.Exception);
			}

			return waitResult;
		}
	}
}