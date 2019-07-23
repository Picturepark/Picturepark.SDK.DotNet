using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class ListItemBatchOperationWithRequestIdResult : BatchOperationWithRequestIdResult<ListItemDetail>
    {
        private readonly IListItemClient _listItemClient;

        public ListItemBatchOperationWithRequestIdResult(
            IListItemClient listItemClient,
            string businessProcessId,
            BusinessProcessLifeCycle? lifeCycle,
            IBusinessProcessClient businessProcessClient) : base(businessProcessId, lifeCycle, businessProcessClient)
        {
            _listItemClient = listItemClient;
        }

        public static ListItemBatchOperationWithRequestIdResult Empty => new ListItemBatchOperationWithRequestIdResult(null, null, null, null);

        public async Task<BatchOperationWithRequestIdResultDetail<ListItemDetail>> FetchDetail(
            IEnumerable<ListItemResolveBehavior> resolveBehaviors = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await FetchDetail(
                async ids => await _listItemClient.GetManyAsync(ids, resolveBehaviors, cancellationToken).ConfigureAwait(false),
                c => c.Id,
                cancellationToken);
        }
    }
}