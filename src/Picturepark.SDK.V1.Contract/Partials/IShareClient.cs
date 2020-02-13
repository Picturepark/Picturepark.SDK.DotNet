using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IShareClient
    {
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Download shared outputs</summary>
        /// <param name="token">Share token</param>
        /// <param name="contentId">The content id</param>
        /// <param name="width">Optional width in pixels to resize image</param>
        /// <param name="height">Optional height in pixels to resize image</param>
        /// <param name="range">The range of bytes to download (http range header): bytes={from}-{to} (e.g. bytes=0-100000)</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        Task<FileResponse> DownloadWithContentIdAsync(string token, string contentId, int? width = null, int? height = null, string range = null, System.Threading.CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Download shared outputs</summary>
        /// <param name="token">Share token</param>
        /// <param name="contentId">The content id</param>
        /// <param name="outputFormatId">The output format id</param>
        /// <param name="width">Optional width in pixels to resize image</param>
        /// <param name="height">Optional height in pixels to resize image</param>
        /// <param name="range">The range of bytes to download (http range header): bytes={from}-{to} (e.g. bytes=0-100000)</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        Task<FileResponse> DownloadWithOutputFormatIdAsync(string token, string contentId, string outputFormatId, int? width = null, int? height = null, string range = null, System.Threading.CancellationToken cancellationToken = default);
    }
}