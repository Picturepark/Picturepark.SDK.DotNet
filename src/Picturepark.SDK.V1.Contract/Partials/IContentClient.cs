using System;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface IContentClient
	{
		Task<ContentDetail> GetAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken));

		Task DeactivateAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken));

		Task<ContentDetail> ReactivateAsync(string contentId, bool resolve = true, int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken));

		ContentDetail Reactivate(string contentId, bool resolve = true, int timeout = 60000);

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