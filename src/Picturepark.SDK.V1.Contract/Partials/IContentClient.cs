using System;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IContentClient
    {
        Task<ContentDetailViewItem> GetAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken));

        Task DeactivateAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken));

        Task<ContentDetailViewItem> ReactivateAsync(string contentId, bool resolve = true, int timeout = 60000);

        ContentDetailViewItem Reactivate(string contentId, bool resolve = true, int timeout = 60000);

        Task DownloadFilesAsync(
            ContentSearchResult contents,
            string exportDirectory,
            bool overwriteIfExists,
            int concurrentDownloads = 4,
            string outputFormat = "Original",
            string outputExtension = "",
            Action<ContentDetailViewItem> successDelegate = null,
            Action<Exception> errorDelegate = null);
    }
}