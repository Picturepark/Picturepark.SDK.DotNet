using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public abstract class BatchOperationResultBase
    {
        private readonly IBusinessProcessClient _businessProcessClient;

        protected BatchOperationResultBase(IBusinessProcessClient businessProcessClient, string businessProcessId)
        {
            BusinessProcessId = businessProcessId;
            _businessProcessClient = businessProcessClient;
        }

        public string BusinessProcessId { get; }

        protected async Task<(BatchResponseRow[] successfulItems, BatchResponseRow[] failedItems)> FetchItems(CancellationToken ct)
        {
            var summary = !string.IsNullOrEmpty(BusinessProcessId)
                ? await _businessProcessClient.GetSummaryAsync(BusinessProcessId, ct).ConfigureAwait(false) as BusinessProcessSummaryBatchBased
                : new BusinessProcessSummaryBatchBased();

            if (summary == null)
                throw new InvalidOperationException("BusinessProcess did not return a BatchResponse");

            async Task<IReadOnlyList<BatchResponseRow>> PageOverAllItems(Func<string, int, string, CancellationToken, Task<BusinessProcessBatch>> fetchPage)
            {
                var items = new List<BatchResponseRow>();

                string pageToken = null;
                do
                {
                    var page = await fetchPage(BusinessProcessId, 200, pageToken, ct).ConfigureAwait(false);
                    pageToken = page.PageToken;

                    if (page.Data is BusinessProcessBatchItemBatchResponse batch)
                        items.AddRange(batch.Items);
                }
                while (pageToken != null);

                return items;
            }

            var successfulRows = new List<BatchResponseRow>();
            var failedRows = new List<BatchResponseRow>();

            if (summary.SucceededItemCount > 0)
                successfulRows.AddRange(await PageOverAllItems(_businessProcessClient.GetSuccessfulItemsAsync));

            if (summary.FailedItemCount > 0)
                failedRows.AddRange(await PageOverAllItems(_businessProcessClient.GetFailedItemsAsync));

            return (successfulRows.ToArray(), failedRows.ToArray());
        }
    }
}