using System;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class BusinessProcessViewItemExtensions
    {
        public static async Task<BusinessProcessWaitResult> Wait4StateAsync(this BusinessProcessViewItem process, string states, IBusinessProcessesClient businessProcessesClient)
        {
            return await businessProcessesClient.WaitForStatesAsync(process.Id, states, 60 * 1000);
        }

        public static async Task<BusinessProcessWaitResult> Wait4MetadataAsync(this BusinessProcessViewItem process, IMetadataObjectsClient metadataObjectClient)
        {
            var waitResult = await metadataObjectClient.WaitForStatesAsync(process.Id, 60 * 1000, new[] { "Completed" });
            if (waitResult.HasStateHit == false)
            {
                var exception = waitResult.BusinessProcess.StateHistory.SingleOrDefault(i => i.Error != null);
                throw new Exception(exception.Error.Exception);
            }

            return waitResult;
        }
    }
}