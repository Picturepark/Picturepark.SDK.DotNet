using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public abstract class BatchOperationResult<T>
    {
        private readonly IBusinessProcessClient _businessProcessClient;

        protected BatchOperationResult(string businessProcessId, BusinessProcessLifeCycle? lifeCycle, IBusinessProcessClient businessProcessClient)
        {
            BusinessProcessId = businessProcessId;
            _businessProcessClient = businessProcessClient;
            LifeCycle = lifeCycle;
        }

        public BusinessProcessLifeCycle? LifeCycle { get; }

        public string BusinessProcessId { get; }

        protected async Task<BatchOperationResultDetail<T>> FetchDetail(Func<string[], Task<IEnumerable<T>>> fetchEntities, CancellationToken cancellationToken = default(CancellationToken))
        {
            BusinessProcessDetailsDataBatchResponse batchResult;

            if (!string.IsNullOrEmpty(BusinessProcessId))
            {
                var details = await _businessProcessClient.GetDetailsAsync(BusinessProcessId, cancellationToken).ConfigureAwait(false);
                batchResult = details.Details as BusinessProcessDetailsDataBatchResponse;
            }
            else
            {
                batchResult = new BusinessProcessDetailsDataBatchResponse()
                {
                    Response = new BatchResponse
                    {
                        Rows = new List<BatchResponseRow>()
                    }
                };
            }

            if (batchResult == null)
                throw new InvalidOperationException("BusinessProcess did not return a BatchResponse");

            return new BatchOperationResultDetail<T>(batchResult, fetchEntities);
        }
    }
}
