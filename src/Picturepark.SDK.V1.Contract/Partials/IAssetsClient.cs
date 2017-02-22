using System;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IAssetsClient
    {
        Task<AssetDetailViewItem> GetAsync(string assetId, CancellationToken cancellationToken = default(CancellationToken));

        Task<AssetDetailViewItem> GetAsync(string assetId, bool resolve, CancellationToken cancellationToken = default(CancellationToken));

        Task DeactivateAsync(string assetId, CancellationToken cancellationToken = default(CancellationToken));

        Task<AssetDetailViewItem> ReactivateAsync(string assetId, bool resolve = true, int timeout = 60000);

        AssetDetailViewItem Reactivate(string assetId, bool resolve = true, int timeout = 60000);

        Task DownloadFilesAsync(
            AssetSearchResult assets,
            string exportDirectory,
            bool overwriteIfExists,
            int concurrentDownloads = 4,
            string outputFormat = "Original",
            string outputExtension = "",
            Action<AssetDetailViewItem> successDelegate = null,
            Action<Exception> errorDelegate = null);
    }
}