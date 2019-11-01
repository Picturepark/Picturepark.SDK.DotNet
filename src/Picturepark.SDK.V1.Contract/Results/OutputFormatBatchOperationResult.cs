using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class OutputFormatBatchOperationResult : BatchOperationResult<OutputFormat>
    {
        private readonly IOutputFormatClient _outputFormatClient;

        public OutputFormatBatchOperationResult(
            IOutputFormatClient outputFormatClient,
            string businessProcessId,
            BusinessProcessLifeCycle? lifeCycle,
            IBusinessProcessClient businessProcessClient)
            : base(
                businessProcessId,
                lifeCycle,
                businessProcessClient)
        {
            _outputFormatClient = outputFormatClient;
        }

        public static OutputFormatBatchOperationResult Empty => new OutputFormatBatchOperationResult(null, null, null, null);

        public async Task<BatchOperationResultDetail<OutputFormat>> FetchDetail(CancellationToken cancellationToken = default)
        {
            return await FetchDetail(
                async ids => await _outputFormatClient.GetManyAsync(ids, cancellationToken).ConfigureAwait(false),
                cancellationToken);
        }
    }
}