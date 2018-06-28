using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IContentClient
    {
        /// <summary>Gets a <see cref="ContentDetail"/> by ID.</summary>
        /// <param name="contentId">The content ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The content detail.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<ContentDetail> GetAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Deactivates a content item by ID (i.e. marks the content item as deleted).</summary>
        /// <param name="contentId">The content ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task DeactivateAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken));

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
        /// <param name="createManyRequest">The content create many request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <returns>Created <see cref="ContentDetail"/>s.</returns>
        Task<IEnumerable<ContentDetail>> CreateManyAsync(ContentCreateManyRequest createManyRequest, CancellationToken cancellationToken = default(CancellationToken));

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
        Task<IEnumerable<ContentDetail>> UpdateMetadataManyAsync(ContentMetadataUpdateManyRequest updateManyRequest, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Batch update fields - by filter</summary>
        /// <param name="updateRequest">The metadata update request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Updated <see cref="ContentDetail"/>s.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<IEnumerable<ContentDetail>> BatchUpdateFieldsByFilterAsync(ContentFieldsFilterUpdateRequest updateRequest, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Batch update fields - by ids</summary>
        /// <param name="updateRequest">The metadata update request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Updated <see cref="ContentDetail"/>s.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<IEnumerable<ContentDetail>> BatchUpdateFieldsByIdsAsync(ContentFieldsUpdateRequest updateRequest, CancellationToken cancellationToken = default(CancellationToken));
    }
}