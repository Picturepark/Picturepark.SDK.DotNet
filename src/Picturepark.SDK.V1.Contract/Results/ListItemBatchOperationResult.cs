using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class ListItemBatchOperationResult : BatchOperationResult<ListItemDetail>
    {
        private readonly IListItemClient _listItemClient;

        public ListItemBatchOperationResult(
            IListItemClient listItemClient,
            string businessProcessId,
            BusinessProcessLifeCycle? lifeCycle,
            IBusinessProcessClient businessProcessClient)
            : base(
                businessProcessId,
                lifeCycle,
                businessProcessClient)
        {
            _listItemClient = listItemClient;
        }

        public static ListItemBatchOperationResult Empty => new ListItemBatchOperationResult(null, null, null, null);

        public async Task<BatchOperationResultDetail<ListItemDetail>> FetchDetail(IEnumerable<ListItemResolveBehavior> resolveBehaviors = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await FetchDetail(
                async ids => await _listItemClient.GetManyAsync(ids, resolveBehaviors, cancellationToken).ConfigureAwait(false),
                cancellationToken);
        }
    }
}