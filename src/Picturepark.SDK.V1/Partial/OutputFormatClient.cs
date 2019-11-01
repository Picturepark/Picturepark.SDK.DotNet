using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Results;

namespace Picturepark.SDK.V1
{
    public partial class OutputFormatClient
    {
        private readonly IBusinessProcessClient _businessProcessClient;

        public OutputFormatClient(IBusinessProcessClient businessProcessClient, IPictureparkServiceSettings settings, HttpClient httpClient)
            : this(settings, httpClient)
        {
            _businessProcessClient = businessProcessClient;
        }

        public async Task<ICollection<OutputFormatDetail>> GetAllAsync(CancellationToken cancellationToken = default)
            => await GetManyAsync(cancellationToken: cancellationToken);

        /// <inheritdoc />
        public async Task<OutputFormatOperationResult> CreateAsync(OutputFormat request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            => await WaitForBusinessProcessAndReturnResult(() => CreateCoreAsync(request, cancellationToken), request.Id, timeout, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<OutputFormatOperationResult> UpdateAsync(string id, OutputFormatEditable request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            => await WaitForBusinessProcessAndReturnResult(() => UpdateCoreAsync(id, request, cancellationToken), id, timeout, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<OutputFormatDeleteResult> DeleteAsync(string id, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var businessProcessId = (await DeleteCoreAsync(id, cancellationToken).ConfigureAwait(false)).Id;

            var result = await _businessProcessClient.WaitForCompletionAsync(businessProcessId, timeout, cancellationToken: cancellationToken).ConfigureAwait(false);

            return new OutputFormatDeleteResult(id, businessProcessId, result.LifeCycleHit);
        }

        /// <inheritdoc />
        public async Task<OutputFormatBatchOperationResult> CreateManyAsync(OutputFormatCreateManyRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            => await WaitForBusinessProcessAndReturnResultMany(() => CreateManyCoreAsync(request, cancellationToken), timeout, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<OutputFormatBatchOperationResult> UpdateManyAsync(OutputFormatUpdateManyRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            => await WaitForBusinessProcessAndReturnResultMany(() => UpdateManyCoreAsync(request, cancellationToken), timeout, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<OutputFormatBatchOperationResult> DeleteManyAsync(OutputFormatDeleteManyRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            => await WaitForBusinessProcessAndReturnResultMany(() => DeleteManyCoreAsync(request, cancellationToken), timeout, cancellationToken).ConfigureAwait(false);

        public async Task<OutputFormatOperationResult> WaitForBusinessProcessAndReturnResult(Func<Task<BusinessProcess>> call, string id, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcessId = (await call().ConfigureAwait(false)).Id;

            var result = await _businessProcessClient.WaitForCompletionAsync(businessProcessId, timeout, cancellationToken: cancellationToken).ConfigureAwait(false);

            return new OutputFormatOperationResult(id, businessProcessId, result.LifeCycleHit, this);
        }

        public async Task<OutputFormatBatchOperationResult> WaitForBusinessProcessAndReturnResultMany(Func<Task<BusinessProcess>> call, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcessId = (await call().ConfigureAwait(false)).Id;

            var result = await _businessProcessClient.WaitForCompletionAsync(businessProcessId, timeout, cancellationToken: cancellationToken).ConfigureAwait(false);

            return new OutputFormatBatchOperationResult(this, businessProcessId, result.LifeCycleHit, _businessProcessClient);
        }
    }
}