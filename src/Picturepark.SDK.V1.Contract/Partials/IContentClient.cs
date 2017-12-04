using System;
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
	}
}