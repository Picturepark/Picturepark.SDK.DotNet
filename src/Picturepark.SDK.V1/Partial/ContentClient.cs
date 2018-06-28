using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Extensions;

namespace Picturepark.SDK.V1
{
    public partial class ContentClient
    {
        private readonly IBusinessProcessClient _businessProcessClient;

        public ContentClient(IBusinessProcessClient businessProcessClient, IPictureparkClientSettings settings, HttpClient httpClient)
            : this(settings, httpClient)
        {
            _businessProcessClient = businessProcessClient;
        }

        /// <summary>Gets a <see cref="ContentDetail"/> by ID.</summary>
        /// <param name="contentId">The content ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The content detail.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<ContentDetail> GetAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAsync(contentId, true, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Deactivates a content item by ID (i.e. marks the content item as deleted).</summary>
        /// <param name="contentId">The content ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task DeactivateAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            await DeactivateAsync(contentId, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Reactivates a content item by ID (i.e. marks the content item as not deleted).</summary>
        /// <param name="contentId">The content ID.</param>
        /// <param name="resolve">Resolves the data of referenced list items into the contents's content.</param>
        /// <param name="timeout">The timeout in milliseconds to wait for completion.</param>
        /// <param name="allowMissingDependencies">Allow reactivating contents that refer to list items or contents that don't exist in the system.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<ContentDetail> ReactivateAsync(string contentId, bool resolve = true, TimeSpan? timeout = null, bool allowMissingDependencies = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ReactivateAsync(contentId, resolve, timeout, null, allowMissingDependencies, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Downloads multiple files.</summary>
        /// <param name="contents">The files to download.</param>
        /// <param name="exportDirectory">The directory to store the downloaded files.</param>
        /// <param name="overwriteIfExists">Specifies whether to overwrite files.</param>
        /// <param name="concurrentDownloads">Specifies the number of concurrent downloads.</param>
        /// <param name="outputFormat">The output format name (e.g. 'Original').</param>
        /// <param name="outputExtension">The expected output file extension.</param>
        /// <param name="successDelegate">The success delegate/callback.</param>
        /// <param name="errorDelegate">The error delegate/callback.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        public async Task DownloadFilesAsync(
            ContentSearchResult contents,
            string exportDirectory,
            bool overwriteIfExists,
            int concurrentDownloads = 4,
            string outputFormat = "Original",
            string outputExtension = "",
            Action<ContentDetail> successDelegate = null,
            Action<Exception> errorDelegate = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            List<Task> allTasks = new List<Task>();

            // Limits Concurrent Downloads
            SemaphoreSlim throttler = new SemaphoreSlim(concurrentDownloads);

            // Create directory if it does not exist
            if (!Directory.Exists(exportDirectory))
                Directory.CreateDirectory(exportDirectory);

            foreach (var content in contents.Results)
            {
                await throttler.WaitAsync(cancellationToken).ConfigureAwait(false);
                allTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var contentDetail = await GetAsync(content.Id, cancellationToken).ConfigureAwait(false);
                        var metadata = contentDetail.GetFileMetadata();
                        string fileNameOriginal = metadata.FileName;

                        try
                        {
                            var fileName = string.IsNullOrEmpty(outputExtension) ?
                                fileNameOriginal :
                                fileNameOriginal.Replace(Path.GetExtension(fileNameOriginal), outputExtension);

                            if (string.IsNullOrEmpty(fileName))
                                throw new Exception("Filename empty: " + metadata);

                            var filePath = Path.Combine(exportDirectory, fileName);

                            if (!new FileInfo(filePath).Exists || overwriteIfExists)
                            {
                                try
                                {
                                    using (var response = await DownloadAsync(content.Id, outputFormat, cancellationToken: cancellationToken).ConfigureAwait(false))
                                    {
                                        using (var fileStream = File.Create(filePath))
                                        {
                                            response.Stream.CopyTo(fileStream);
                                        }
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

        /// <summary>Create - many</summary>
        /// <param name="createManyRequest">The content create many request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <returns>Created <see cref="ContentDetail"/>s.</returns>
        public async Task<IEnumerable<ContentDetail>> CreateManyAsync(ContentCreateManyRequest createManyRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!createManyRequest.Requests.Any())
            {
                return new List<ContentDetail>();
            }

            var businessProcess = await CreateManyCoreAsync(createManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForCompletionAndReturnItems(businessProcess, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Update metadata - many</summary>
        /// <param name="updateManyRequest">The metadata update requests.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Updated <see cref="ContentDetail"/>s.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="ContentsNotFoundException">Not all provided contents could be found</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        public async Task<IEnumerable<ContentDetail>> UpdateMetadataManyAsync(ContentMetadataUpdateManyRequest updateManyRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!updateManyRequest.Requests.Any())
            {
                return new List<ContentDetail>();
            }

            var businessProcess = await UpdateMetadataManyCoreAsync(updateManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForCompletionAndReturnItems(businessProcess, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Batch update fields - by filter</summary>
        /// <param name="updateRequest">The metadata update request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Updated <see cref="ContentDetail"/>s.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        public async Task<IEnumerable<ContentDetail>> BatchUpdateFieldsByFilterAsync(ContentFieldsFilterUpdateRequest updateRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await BatchUpdateFieldsByFilterCoreAsync(updateRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForCompletionAndReturnItems(businessProcess, cancellationToken);
        }

        /// <summary>Batch update fields - by ids</summary>
        /// <param name="updateRequest">The metadata update request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Updated <see cref="ContentDetail"/>s.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        public async Task<IEnumerable<ContentDetail>> BatchUpdateFieldsByIdsAsync(ContentFieldsUpdateRequest updateRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await BatchUpdateFieldsByIdsCoreAsync(updateRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForCompletionAndReturnItems(businessProcess, cancellationToken);
        }

        private async Task<IEnumerable<ContentDetail>> WaitForCompletionAndReturnItems(BusinessProcess businessProcess, CancellationToken cancellationToken)
        {
            var waitResult = await _businessProcessClient.WaitForCompletionAsync(businessProcess.Id, null, cancellationToken).ConfigureAwait(false);
            if (waitResult.HasLifeCycleHit)
            {
                if (waitResult.LifeCycleHit == BusinessProcessLifeCycle.Failed)
                {
                    // TODO: Use better exception classes in this method.
                    throw new Exception("The business process failed to execute.");
                }

                var details = await _businessProcessClient.GetDetailsAsync(businessProcess.Id, cancellationToken).ConfigureAwait(false);
                var bulkResult = (BusinessProcessDetailsDataBatchResponse)details.Details;

                if (waitResult.LifeCycleHit == BusinessProcessLifeCycle.SucceededWithErrors)
                {
                    if (bulkResult.Response.Rows.Any(i => !i.Succeeded))
                    {
                        // TODO: Use better exception classes in this method.
                        throw new Exception("Could not save all objects.");
                    }
                }

                return await GetManyAsync(bulkResult.Response.Rows.Select(i => i.Id), true, null, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("The business process has not been completed.");
            }
        }
    }
}
