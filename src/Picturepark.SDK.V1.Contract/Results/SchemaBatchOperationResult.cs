using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class SchemaBatchOperationResult : BatchOperationResult<SchemaDetail>
    {
        private readonly ISchemaClient _schemaClient;

        public SchemaBatchOperationResult(
            ISchemaClient schemaClient,
            string businessProcessId,
            BusinessProcessLifeCycle? lifeCycle,
            IBusinessProcessClient businessProcessClient)
            : base(businessProcessId, lifeCycle, businessProcessClient)
        {
            _schemaClient = schemaClient;
        }

        public static SchemaBatchOperationResult Empty => new SchemaBatchOperationResult(null, null, null, null);

        public async Task<BatchOperationResultDetail<SchemaDetail>> FetchDetail(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await FetchDetail(async ids => await _schemaClient.GetManyAsync(ids, cancellationToken).ConfigureAwait(false), c => c.Id, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}