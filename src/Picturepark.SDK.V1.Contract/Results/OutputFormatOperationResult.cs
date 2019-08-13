using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class OutputFormatOperationResult : OutputFormatOperationResultWithoutFetch
    {
        private readonly IOutputFormatClient _outputFormatClient;

        public OutputFormatOperationResult(string outputFormatId, string businessProcessId, BusinessProcessLifeCycle? lifeCycle, IOutputFormatClient outputFormatClient) : base(outputFormatId, businessProcessId, lifeCycle)
        {
            _outputFormatClient = outputFormatClient;
        }

        public virtual async Task<OutputFormatDetail> FetchResult(CancellationToken cancellationToken = default)
        {
            return await _outputFormatClient.GetAsync(OutputFormatId, cancellationToken).ConfigureAwait(false);
        }
    }
}