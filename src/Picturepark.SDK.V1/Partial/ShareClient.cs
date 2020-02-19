using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public partial class ShareClient
    {
        public async Task<FileResponse> DownloadWithContentIdAsync(string token, string contentId, int? width = null, int? height = null, string range = null, CancellationToken cancellationToken = default)
            => await DownloadSingleContentAsync(token, contentId, null, width, height, range, cancellationToken).ConfigureAwait(false);

        public async Task<FileResponse> DownloadWithOutputFormatIdAsync(
            string token, string contentId, string outputFormatId, int? width = null, int? height = null, string range = null, CancellationToken cancellationToken = default)
            => await DownloadSingleContentAsync(token, contentId, outputFormatId, width, height, range, cancellationToken).ConfigureAwait(false);
    }
}
