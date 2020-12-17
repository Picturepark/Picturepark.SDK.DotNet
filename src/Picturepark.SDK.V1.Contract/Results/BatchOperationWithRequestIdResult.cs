using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public abstract class BatchOperationWithRequestIdResult<T> : BatchOperationResultBase
    {
        private readonly IBusinessProcessClient _businessProcessClient;

        protected BatchOperationWithRequestIdResult(string businessProcessId, BusinessProcessLifeCycle? lifeCycle, IBusinessProcessClient businessProcessClient)
            : base(businessProcessClient, businessProcessId)
        {
            _businessProcessClient = businessProcessClient;
            LifeCycle = lifeCycle;
        }

        public BusinessProcessLifeCycle? LifeCycle { get; }

        protected async Task<BatchOperationWithRequestIdResultDetail<T>> FetchDetail(Func<string[], Task<IEnumerable<T>>> fetchEntities, Func<T, string> idAccessor, CancellationToken cancellationToken = default(CancellationToken))
        {
            var (successfulRows, failedRows) = await FetchItems(cancellationToken).ConfigureAwait(false);
            return new BatchOperationWithRequestIdResultDetail<T>(successfulRows, failedRows, fetchEntities, idAccessor);
        }
    }
}