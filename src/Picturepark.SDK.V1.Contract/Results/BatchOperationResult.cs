using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class BatchOperationResult<T>
    {
        private readonly Func<string[], Task<IEnumerable<T>>> _fetchEntities;
        private readonly IBusinessProcessClient _businessProcessClient;

        public BatchOperationResult(string businessProcessId, BusinessProcessLifeCycle? lifeCycle, Func<string[], Task<IEnumerable<T>>> fetchEntities, IBusinessProcessClient businessProcessClient)
        {
            BusinessProcessId = businessProcessId;
            _fetchEntities = fetchEntities;
            _businessProcessClient = businessProcessClient;
            LifeCycle = lifeCycle;
        }

        public static BatchOperationResult<T> Empty => new BatchOperationResult<T>(null, null, null, null);

        public BusinessProcessLifeCycle? LifeCycle { get; }

        public string BusinessProcessId { get; }

        public async Task<BatchOperationResultDetail<T>> FetchDetail(CancellationToken cancellationToken = default(CancellationToken))
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

            return new BatchOperationResultDetail<T>(batchResult, _fetchEntities);
        }
    }
}
