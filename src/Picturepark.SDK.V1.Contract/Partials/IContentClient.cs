using System;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract.Results;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IContentClient
    {
        /// <summary>Downloads multiple files</summary>
        /// <param name="contents">The files to download.</param>
        /// <param name="exportDirectory">The directory to store the downloaded files.</param>
        /// <param name="overwriteIfExists">Specifies whether to overwrite files.</param>
        /// <param name="concurrentDownloads">Specifies the number of concurrent downloads.</param>
        /// <param name="outputFormat">The output format name (e.g. 'Original').</param>
        /// <param name="outputExtension">The expected output file extension.</param>
        /// <param name="contentIdAsFilename">Specifies whether to use the content id as filename. If false, the original filename is used and a counter added if needed.</param>
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
            bool contentIdAsFilename = false,
            Action<ContentDetail> successDelegate = null,
            Action<Exception> errorDelegate = null,
            CancellationToken cancellationToken = default);

        /// <summary>Create - many</summary>
        /// <param name="contentCreateManyRequest">The content create many request.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="waitSearchDocCreation">Wait for the creation of the search document and the rendered display values.
        /// By default the method waits for the search documents creation. Passing false, the method will return when the main entities have been created and the creation of the search documents has been enqueued but not yet performed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        Task<ContentBatchOperationWithRequestIdResult> CreateManyAsync(ContentCreateManyRequest contentCreateManyRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Update metadata - many</summary>
        /// <param name="updateRequest">The metadata update requests.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="waitSearchDocCreation">Wait for the creation of the search document and the rendered display values.
        /// By default the method waits for the search documents creation. Passing false, the method will return when the main entities have been updated and the creation of the search documents has been enqueued but not yet performed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="ContentNotFoundException">Not all provided contents could be found</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<ContentBatchOperationResult> UpdateMetadataManyAsync(ContentMetadataUpdateManyRequest updateRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Update permissions - many</summary>
        /// <param name="updateManyRequest">The permissions update many request.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="waitSearchDocCreation">Wait for the creation of the search document and the rendered display values.
        /// By default the method waits for the search documents creation. Passing false, the method will return when the main entities have been created and the creation of the search documents has been enqueued but not yet performed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<ContentBatchOperationResult> UpdatePermissionsManyAsync(ContentPermissionsUpdateManyRequest updateManyRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Transfer ownership - many</summary>
        /// <param name="contentOwnershipTransferManyRequest">The content ownership transfer many request.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="waitSearchDocCreation">Wait for the creation of the search document and the rendered display values.
        /// By default the method waits for the search documents creation. Passing false, the method will return when the main entities have been updated and the creation of the search documents has been enqueued but not yet performed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<ContentBatchOperationResult> TransferOwnershipManyAsync(ContentOwnershipTransferManyRequest contentOwnershipTransferManyRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Batch update fields - by ids</summary>
        /// <param name="updateRequest">The metadata update request.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="waitSearchDocCreation">Wait for the creation of the search document and the rendered display values.
        /// By default the method waits for the search documents creation. Passing false, the method will return when the main entities have been updated and the creation of the search documents has been enqueued but not yet performed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<ContentBatchOperationResult> BatchUpdateFieldsByIdsAsync(ContentFieldsBatchUpdateRequest updateRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Batch update fields - by filter</summary>
        /// <param name="updateRequest">The metadata update request.</param>
        /// <param name="timeout">Timeout for waiting on the business process.</param>
        /// <param name="waitSearchDocCreation">Wait for the creation of the search document and the rendered display values.
        /// By default the method waits for the search documents creation. Passing false, the method will return when the main entities have been updated and the creation of the search documents has been enqueued but not yet performed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<ContentBatchOperationResult> BatchUpdateFieldsByFilterAsync(ContentFieldsBatchUpdateFilterRequest updateRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Waits for a business process and returns a <see cref="ContentBatchOperationResult"/>.
        /// </summary>
        /// <param name="businessProcessId">The business process id.</param>
        /// <param name="timeout">The timeout to wait on the business process.</param>
        /// <param name="waitSearchDocCreation">Wait for the creation of the search documents and the rendered display values</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="ContentBatchOperationResult"/>.</returns>
        Task<ContentBatchOperationResult> WaitForBusinessProcessAndReturnResult(string businessProcessId, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Waits for a business process and returns a <see cref="ContentBatchOperationWithRequestIdResult"/>.
        /// </summary>
        /// <param name="businessProcessId">The business process id.</param>
        /// <param name="timeout">The timeout to wait on the business process.</param>
        /// <param name="waitSearchDocCreation">Wait for the creation of the search documents and the rendered display values</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="ContentBatchOperationWithRequestIdResult"/>.</returns>
        Task<ContentBatchOperationWithRequestIdResult> WaitForBusinessProcessAndReturnResultWithRequestId(string businessProcessId, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a download link and awaits potential dynamic output format rendering
        /// </summary>
        /// <param name="request">Content download link request</param>
        /// <param name="timeout">Timeout for operation</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Download link</returns>
        Task<DownloadLink> CreateAndAwaitDownloadLinkAsync(ContentDownloadLinkCreateRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a download link for a specific content historic version
        /// </summary>
        /// <param name="contentId">Content ID</param>
        /// <param name="version">Version number</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Download link</returns>
        Task<DownloadLink> GetVersionDownloadLinkAsync(string contentId, int version, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a comment
        /// </summary>
        /// <param name="id">Comment ID</param>
        /// <returns>Comment</returns>
        Task<Comment> GetCommentAsync(string id);

        /// <summary>
        /// Update a comment
        /// </summary>
        /// <param name="id">Comment ID</param>
        /// <param name="request">What should be updated</param>
        /// <returns>Updated comment</returns>
        Task<Comment> UpdateCommentAsync(string id, CommentEditable request);

        /// <summary>
        /// Delete a comment
        /// </summary>
        /// <param name="id">Comment ID</param>
        /// <returns>OK</returns>
        Task DeleteCommentAsync(string id);
    }
}