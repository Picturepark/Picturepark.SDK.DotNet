using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface ITransferClient
    {
        /// <summary>Searches files of a given transfer ID.</summary>
        /// <param name="transferId">The transfer ID.</param>
        /// <param name="limit">The maximum number of search results.</param>
        /// <returns>The result.</returns>
        Task<IReadOnlyCollection<FileTransfer>> SearchFilesByTransferIdAsync(string transferId, int? limit = null);

        /// <summary>Uploads multiple files from the filesystem.</summary>
        /// <param name="transferName">The name of the created transfer.</param>
        /// <param name="files">The file paths on the filesystem with optional overrides.</param>
        /// <param name="uploadOptions">The file upload options.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created transfer object.</returns>
        Task<CreateTransferResult> UploadFilesAsync(string transferName, IEnumerable<FileLocations> files, UploadOptions uploadOptions, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Uploads multiple files from the filesystem.</summary>
        /// <param name="transfer">The existing transfer object.</param>
        /// <param name="files">The file paths on the filesystem with optional overrides.</param>
        /// <param name="uploadOptions">The file upload options.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created transfer object.</returns>
        Task UploadFilesAsync(Transfer transfer, IEnumerable<FileLocations> files, UploadOptions uploadOptions, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Transfers the uploaded files and waits for its completions.</summary>
        /// <param name="transfer">The transfer.</param>
        /// <param name="createRequest">The create request.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellcation token.</param>
        /// <returns>The task.</returns>
        Task ImportAndWaitForCompletionAsync(Transfer transfer, ImportTransferRequest createRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Creates a transfer and waits for its completion.</summary>
        /// <param name="request">The create request.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The transfer.</returns>
        Task<CreateTransferResult> CreateAndWaitForCompletionAsync(CreateTransferRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Creates a transfer and waits for its completion.</summary>
        /// <param name="transferName">The name of the transfer.</param>
        /// <param name="files">The file names of the transfer.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="uploadOptions">The upload options.</param>
        /// <returns>The transfer.</returns>
        Task<CreateTransferResult> CreateAndWaitForCompletionAsync(
            string transferName,
            IEnumerable<FileLocations> files,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            UploadOptions uploadOptions = null);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Cancel transfer</summary>
        /// <param name="id">ID of transfer.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        [Obsolete("This method will be removed in future versions. Please use CancelAsync method instead.")]
        Task CancelTransferAsync(string id, CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Import transfer</summary>
        /// <param name="id">ID of transfer.</param>
        /// <param name="request">The ImportTransfer request.</param>
        /// <returns>Transfer</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        [Obsolete("This method will be removed in future versions. Please use ImportAsync method instead.")]
        Task<Transfer> ImportTransferAsync(string id, ImportTransferRequest request, CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Upload file</summary>
        /// <param name="formFile">Information about chunk.</param>
        /// <param name="chunkNumber">Information about chunk.</param>
        /// <param name="currentChunkSize">Information about chunk.</param>
        /// <param name="totalSize">Information about chunk.</param>
        /// <param name="totalChunks">Information about chunk.</param>
        /// <param name="transferId">ID of transfer.</param>
        /// <param name="requestId">Identifier of file.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        [Obsolete("This overload is going to be removed in future versions. Please use the (string, string, Stream, long, long, long, long, CancellationToken) one")]
        Task UploadFileAsync(
            long chunkNumber,
            long currentChunkSize,
            long totalSize,
            long totalChunks,
            string transferId,
            string requestId,
            FileParameter formFile = null,
            CancellationToken cancellationToken = default);

        /// <summary>Upload file</summary>
        /// <param name="transferId">ID of transfer.</param>
        /// <param name="requestId">Identifier of file.</param>
        /// <param name="chunkNumber">Information about chunk.</param>
        /// <param name="currentChunkSize">Information about chunk.</param>
        /// <param name="totalSize">Information about chunk.</param>
        /// <param name="totalChunks">Information about chunk.</param>
        /// <param name="formFile">Information about chunk.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        [Obsolete("This overload is going to be removed in future versions. Please use the (string, string, Stream, long, long, long, long, CancellationToken) one")]
        Task UploadFileAsync(
            FileParameter formFile,
            long chunkNumber,
            long currentChunkSize,
            long totalSize,
            long totalChunks,
            string transferId,
            string requestId,
            System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
    }
}