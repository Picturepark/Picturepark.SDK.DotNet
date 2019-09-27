using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Results;

namespace Picturepark.SDK.V1
{
    public partial class ContentClient
    {
        private readonly IBusinessProcessClient _businessProcessClient;

        public ContentClient(IBusinessProcessClient businessProcessClient, IPictureparkServiceSettings settings, HttpClient httpClient)
            : this(settings, httpClient)
        {
            _businessProcessClient = businessProcessClient;
        }

        /// <inheritdoc />
        public async Task DownloadFilesAsync(
            ContentSearchResult contents,
            string exportDirectory,
            bool overwriteIfExists,
            int concurrentDownloads = 4,
            string outputFormat = "Original",
            string outputExtension = "",
            bool contentIdAsFilename = false,
            Action<ContentDetail> successDelegate = null,
            Action<Exception> errorDelegate = null,
            CancellationToken cancellationToken = default)
        {
            var allTasks = new List<Task>();
            var fileNameRegistry = new ConcurrentDictionary<string, object>();

            // Create directory if it does not exist
            if (!Directory.Exists(exportDirectory))
                Directory.CreateDirectory(exportDirectory);

            using (var throttler = new SemaphoreSlim(concurrentDownloads)) // Limits Concurrent Downloads
            {
                foreach (var content in contents.Results)
                {
                    await throttler.WaitAsync(cancellationToken).ConfigureAwait(false);
                    allTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var contentDetail = await GetAsync(content.Id, new[] { ContentResolveBehavior.Content }, cancellationToken).ConfigureAwait(false);
                            var metadata = contentDetail.GetFileMetadata();
                            var fileNameOriginal = metadata.FileName;

                            try
                            {
                                if (string.IsNullOrEmpty(fileNameOriginal))
                                    throw new Exception("Filename empty: " + metadata);

                                var fileName = string.IsNullOrEmpty(outputExtension)
                                    ? fileNameOriginal
                                    : fileNameOriginal.Replace(Path.GetExtension(fileNameOriginal), outputExtension);

                                var extension = Path.GetExtension(fileName);
                                var nameNoExtension = Path.GetFileNameWithoutExtension(fileName);

                                if (contentIdAsFilename)
                                {
                                    fileName = content.Id + extension;
                                }
                                else
                                {
                                    var fileNameDiscriminator = 1;
                                    do
                                    {
                                        var discriminatorString = fileNameDiscriminator == 1
                                            ? string.Empty
                                            : $" ({fileNameDiscriminator})";

                                        fileName = nameNoExtension + discriminatorString + extension;
                                        fileNameDiscriminator++;
                                    }
                                    while (!fileNameRegistry.TryAdd(fileName, null));
                                }

                                var filePath = Path.Combine(exportDirectory, fileName);

                                if (!new FileInfo(filePath).Exists || overwriteIfExists)
                                {
                                    try
                                    {
                                        using (var response = await DownloadAsync(content.Id, outputFormat, cancellationToken: cancellationToken).ConfigureAwait(false))
                                        using (var fileStream = File.Create(filePath))
                                        {
                                            await response.Stream.CopyToAsync(fileStream).ConfigureAwait(false);
                                        }

                                        successDelegate?.Invoke(contentDetail);
                                    }
                                    catch (Exception ex)
                                    {
                                        errorDelegate?.Invoke(ex);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                errorDelegate?.Invoke(ex);
                            }
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(allTasks).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task<ContentBatchOperationWithRequestIdResult> CreateManyAsync(ContentCreateManyRequest contentCreateManyRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await CreateManyCoreAsync(contentCreateManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResultWithRequestId(businessProcess.Id, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ContentBatchOperationResult> UpdateMetadataManyAsync(ContentMetadataUpdateManyRequest updateRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await UpdateMetadataManyCoreAsync(updateRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ContentBatchOperationResult> UpdatePermissionsManyAsync(ContentPermissionsUpdateManyRequest updateManyRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await UpdatePermissionsManyCoreAsync(updateManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ContentBatchOperationResult> TransferOwnershipManyAsync(ContentOwnershipTransferManyRequest contentOwnershipTransferManyRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await TransferOwnershipManyCoreAsync(contentOwnershipTransferManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ContentBatchOperationResult> BatchUpdateFieldsByIdsAsync(ContentFieldsBatchUpdateRequest updateRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await BatchUpdateFieldsByIdsCoreAsync(updateRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ContentBatchOperationResult> BatchUpdateFieldsByFilterAsync(ContentFieldsBatchUpdateFilterRequest updateRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await BatchUpdateFieldsByFilterCoreAsync(updateRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ContentBatchOperationResult> WaitForBusinessProcessAndReturnResult(string businessProcessId, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _businessProcessClient.WaitForCompletionAsync(businessProcessId, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);

            return new ContentBatchOperationResult(this, businessProcessId, result.LifeCycleHit, _businessProcessClient);
        }

        /// <inheritdoc />
        public async Task<ContentBatchOperationWithRequestIdResult> WaitForBusinessProcessAndReturnResultWithRequestId(string businessProcessId, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _businessProcessClient.WaitForCompletionAsync(businessProcessId, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);

            return new ContentBatchOperationWithRequestIdResult(this, businessProcessId, result.LifeCycleHit, _businessProcessClient);
        }
    }
}
