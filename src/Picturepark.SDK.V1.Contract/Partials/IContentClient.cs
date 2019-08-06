using System;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract.Results;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IContentClient
    {
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
        Task DownloadFilesAsync(
            ContentSearchResult contents,
            string exportDirectory,
            bool overwriteIfExists,
            int concurrentDownloads = 4,
            string outputFormat = "Original",
            string outputExtension = "",
            Action<ContentDetail> successDelegate = null,
            Action<Exception> errorDelegate = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Create - many</summary>
        /// <param name="contentCreateManyRequest">The content create many request.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        Task<ContentBatchOperationWithRequestIdResult> CreateManyAsync(ContentCreateManyRequest contentCreateManyRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Update metadata - many</summary>
        /// <param name="updateRequest">The metadata update requests.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="ContentNotFoundException">Not all provided contents could be found</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<ContentBatchOperationResult> UpdateMetadataManyAsync(ContentMetadataUpdateManyRequest updateRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Update permissions - many</summary>
        /// <param name="updateManyRequest">The permissions update many request.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<ContentBatchOperationResult> UpdatePermissionsManyAsync(ContentPermissionsUpdateManyRequest updateManyRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Transfer ownership - many</summary>
        /// <param name="contentOwnershipTransferManyRequest">The content ownership transfer many request.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<ContentBatchOperationResult> TransferOwnershipManyAsync(ContentOwnershipTransferManyRequest contentOwnershipTransferManyRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Batch update fields - by ids</summary>
        /// <param name="updateRequest">The metadata update request.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<ContentBatchOperationResult> BatchUpdateFieldsByIdsAsync(ContentFieldsBatchUpdateRequest updateRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Batch update fields - by filter</summary>
        /// <param name="updateRequest">The metadata update request.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<ContentBatchOperationResult> BatchUpdateFieldsByFilterAsync(ContentFieldsBatchUpdateFilterRequest updateRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Waits for a business process and returns a <see cref="ContentBatchOperationResult"/>.
        /// </summary>
        /// <param name="businessProcessId">The business process id.</param>
        /// <param name="timeout">The timeout to wait on the business process.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        Task<ContentBatchOperationResult> WaitForBusinessProcessAndReturnResult(string businessProcessId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Waits for a business process and returns a <see cref="ContentBatchOperationWithRequestIdResult"/>.
        /// </summary>
        /// <param name="businessProcessId">The business process id.</param>
        /// <param name="timeout">The timeout to wait on the business process.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="ContentBatchOperationWithRequestIdResult"/>.</returns>
        Task<ContentBatchOperationWithRequestIdResult> WaitForBusinessProcessAndReturnResultWithRequestId(string businessProcessId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}