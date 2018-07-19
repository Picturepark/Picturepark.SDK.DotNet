using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class BatchOperationResultDetail<T>
    {
        internal BatchOperationResultDetail(BusinessProcessDetailsDataBatchResponse details, Func<string[], Task<IEnumerable<T>>> fetchEntities)
        {
            FailedItems = details.Response.Rows.Where(r => !r.Succeeded).ToArray();
            FailedIds = FailedItems.Select(i => i.Id).ToArray();

            SucceededIds = details.Response.Rows.Where(r => r.Succeeded).Select(i => i.Id).ToArray();
            SucceededItems = new BatchOperationResultCollection<T>(details, fetchEntities);
        }

        public IReadOnlyCollection<T> SucceededItems { get; }

        public IReadOnlyList<BatchResponseRow> FailedItems { get; }

        public IReadOnlyList<string> SucceededIds { get; }

        public IReadOnlyList<string> FailedIds { get; }
    }
}