﻿using System.Net.Http;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public partial class ShareAccessClient
    {
        /// <summary>Download Shared outputs</summary>
        /// <param name="token">The token</param>
        /// <param name="width">Optional width in pixels to resize image</param>
        /// <param name="height">Optional height in pixels to resize image</param>
        /// <param name="range">The range of bytes to download (http range header): bytes={from}-{to} (e.g. bytes=0-100000)</param>
        /// <returns>HttpResponseMessage</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async System.Threading.Tasks.Task<FileResponse> DownloadAsync(string token, int? width = null, int? height = null, string range = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return await DownloadCoreAsync(token, width, height, range, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Download Shared outputs</summary>
        /// <param name="token">The token</param>
        /// <param name="contentId">Content Id of shared content</param>
        /// <param name="width">Optional width in pixels to resize image</param>
        /// <param name="height">Optional height in pixels to resize image</param>
        /// <param name="range">The range of bytes to download (http range header): bytes={from}-{to} (e.g. bytes=0-100000)</param>
        /// <returns>HttpResponseMessage</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async System.Threading.Tasks.Task<FileResponse> DownloadAsync(string token, string contentId, int? width = null, int? height = null, string range = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return await DownloadWithContentIdCoreAsync(token, contentId, width, height, range, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Download Shared outputs</summary>
        /// <param name="token">The token</param>
        /// <param name="contentId">Content Id of shared content</param>
        /// <param name="outputFormatId">Outputformat Id of shared content</param>
        /// <param name="width">Optional width in pixels to resize image</param>
        /// <param name="height">Optional height in pixels to resize image</param>
        /// <param name="range">The range of bytes to download (http range header): bytes={from}-{to} (e.g. bytes=0-100000)</param>
        /// <returns>HttpResponseMessage</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async System.Threading.Tasks.Task<FileResponse> DownloadAsync(string token, string contentId, string outputFormatId, int? width = null, int? height = null, string range = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return await DownloadWithOutputFormatIdCoreAsync(token, contentId, outputFormatId, width, height, range, cancellationToken).ConfigureAwait(false);
        }
    }
}