using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class ContentBatchOperationResult : BatchOperationResult<ContentDetail>
    {
        private readonly IContentClient _contentClient;

        public ContentBatchOperationResult(
            IContentClient contentClient,
            string businessProcessId,
            BusinessProcessLifeCycle? lifeCycle,
            IBusinessProcessClient businessProcessClient)
            : base(
                businessProcessId,
                lifeCycle,
                businessProcessClient)
        {
            _contentClient = contentClient;
        }

        public static ContentBatchOperationResult Empty => new ContentBatchOperationResult(null, null, null, null);

        public async Task<BatchOperationResultDetail<ContentDetail>> FetchDetail(IEnumerable<ContentResolveBehavior> resolveBehaviors = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await FetchDetail(
                async ids => await _contentClient.GetManyAsync(ids, resolveBehaviors, cancellationToken).ConfigureAwait(false),
                c => c.Id,
                cancellationToken);
        }
    }
}