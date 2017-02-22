using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Authentication;
using Picturepark.SDK.V1.Contract.Extensions;

namespace Picturepark.SDK.V1
{
	public partial class AssetsClient
    {
        public AssetsClient(string baseUrl, IAuthClient authClient) : this(authClient)
        {
            BaseUrl = baseUrl;
        }

        /// <summary>Gets an asset.</summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The asset details.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<AssetDetailViewItem> GetAsync(string assetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAsync(assetId, true, string.Empty, cancellationToken);
        }

        // TODO(ubr): Describe resolve parameter

        /// <summary>Gets an asset.</summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="resolve">If set to <c>true</c> resolves the asset relations.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The asset details.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<AssetDetailViewItem> GetAsync(string assetId, bool resolve, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAsync(assetId, resolve, string.Empty, cancellationToken);
        }

        /// <summary>Deactivates the an asset.</summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task DeactivateAsync(string assetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            await DeactivateAsync(assetId, 60000, cancellationToken);
        }

        // TODO(ubr): Describe resolve parameter

        /// <summary>Reactivates an asset.</summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="resolve">If set to <c>true</c> resolves the asset relations.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<AssetDetailViewItem> ReactivateAsync(string assetId, bool resolve = true, int timeout = 60000)
        {
            return await ReactivateAsync(assetId, resolve, timeout, string.Empty);
        }

        /// <summary>Reactivates the specified asset identifier.</summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="resolve">if set to <c>true</c> [resolve].</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public AssetDetailViewItem Reactivate(string assetId, bool resolve = true, int timeout = 60000)
        {
            return Task.Run(async () => await ReactivateAsync(assetId, resolve, timeout)).GetAwaiter().GetResult();
        }

        public async Task DownloadFilesAsync(
            AssetSearchResult assets,
            string exportDirectory,
            bool overwriteIfExists,
            int concurrentDownloads = 4,
            string outputFormat = "Original",
            string outputExtension = "",
            Action<AssetDetailViewItem> successDelegate = null,
            Action<Exception> errorDelegate = null)
        {
            List<Task> allTasks = new List<Task>();

            // Limits Concurrent Downloads
            SemaphoreSlim throttler = new SemaphoreSlim(initialCount: concurrentDownloads);

            // Create directory if it does not exist
            if (!Directory.Exists(exportDirectory))
                Directory.CreateDirectory(exportDirectory);

            foreach (var asset in assets.Results)
            {
                await throttler.WaitAsync();
                allTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var assetDetail = await GetAsync(asset.Id);
                        var metadata = assetDetail.GetFileMetadata();
                        string fileNameOriginal = metadata.FileName;

                        try
                        {
                            var fileName = string.IsNullOrEmpty(outputExtension) ? fileNameOriginal : fileNameOriginal.Replace(Path.GetExtension(fileNameOriginal), outputExtension);

                            if (string.IsNullOrEmpty(fileName))
                                throw new Exception("Filename empty: " + metadata);

                            var filePath = Path.Combine(exportDirectory, fileName);

                            if (!new FileInfo(filePath).Exists || overwriteIfExists)
                            {
                                try
                                {
                                    using (var response = await DownloadAsync(asset.Id, outputFormat))
                                    {
                                        using (var fileStream = File.Create(filePath))
                                        {
                                            response.Stream.Seek(0, SeekOrigin.Begin);
                                            response.Stream.CopyTo(fileStream);
                                            //// Dispose closes the stream fileStream.Close();
                                        }
                                    }
                                    if (successDelegate != null)
                                        successDelegate.Invoke(assetDetail);
                                }
                                catch (Exception ex)
                                {
                                    if (errorDelegate != null)
                                        errorDelegate.Invoke(ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (errorDelegate != null)
                                errorDelegate.Invoke(ex);
                        }
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(allTasks).ConfigureAwait(true);
        }
    }
}
