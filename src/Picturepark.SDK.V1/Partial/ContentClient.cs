using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Extensions;

namespace Picturepark.SDK.V1
{
	public partial class ContentClient
    {
        /// <summary>Gets an content.</summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The content details.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<ContentDetail> GetAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAsync(contentId, true, null, cancellationToken);
        }

        // TODO(ubr): Describe resolve parameter

        /// <summary>Gets an content.</summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="resolve">If set to <c>true</c> resolves the content relations.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The content details.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<ContentDetail> GetAsync(string contentId, bool resolve, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAsync(contentId, resolve, null, cancellationToken);
        }

        /// <summary>Deactivates the an content.</summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task DeactivateAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            await DeactivateAsync(contentId, 60000, cancellationToken);
        }

		// TODO(ubr): Describe resolve parameter

		/// <summary>Reactivates an content.</summary>
		/// <param name="contentId">The content identifier.</param>
		/// <param name="resolve">If set to <c>true</c> resolves the content relations.</param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The content detail.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task<ContentDetail> ReactivateAsync(string contentId, bool resolve = true, int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ReactivateAsync(contentId, resolve, timeout, null, cancellationToken);
        }

        /// <summary>Reactivates the specified content identifier.</summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="resolve">if set to <c>true</c> [resolve].</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The content detail.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ContentDetail Reactivate(string contentId, bool resolve = true, int timeout = 60000)
        {
            return Task.Run(async () => await ReactivateAsync(contentId, resolve, timeout)).GetAwaiter().GetResult();
        }

        public async Task DownloadFilesAsync(
            ContentSearchResult contents,
            string exportDirectory,
            bool overwriteIfExists,
            int concurrentDownloads = 4,
            string outputFormat = "Original",
            string outputExtension = "",
            Action<ContentDetail> successDelegate = null,
            Action<Exception> errorDelegate = null,
			CancellationToken cancellationToken = default(CancellationToken))
        {
            List<Task> allTasks = new List<Task>();

            // Limits Concurrent Downloads
            SemaphoreSlim throttler = new SemaphoreSlim(initialCount: concurrentDownloads);

            // Create directory if it does not exist
            if (!Directory.Exists(exportDirectory))
                Directory.CreateDirectory(exportDirectory);

            foreach (var content in contents.Results)
            {
                await throttler.WaitAsync(cancellationToken);
                allTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var contentDetail = await GetAsync(content.Id, cancellationToken);
                        var metadata = contentDetail.GetFileMetadata();
                        string fileNameOriginal = metadata.FileName;

                        try
                        {
                            var fileName = string.IsNullOrEmpty(outputExtension) ?
								fileNameOriginal :
								fileNameOriginal.Replace(Path.GetExtension(fileNameOriginal), outputExtension);

                            if (string.IsNullOrEmpty(fileName))
                                throw new Exception("Filename empty: " + metadata);

                            var filePath = Path.Combine(exportDirectory, fileName);

                            if (!new FileInfo(filePath).Exists || overwriteIfExists)
                            {
                                try
                                {
                                    using (var response = await DownloadAsync(content.Id, outputFormat, cancellationToken: cancellationToken))
                                    {
                                        using (var fileStream = File.Create(filePath))
                                        {
                                            response.Stream.CopyTo(fileStream);
                                        }
                                    }
	                                successDelegate?.Invoke(contentDetail);
                                }
                                catch (Exception ex)
                                {
	                                errorDelegate?.Invoke(ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
	                        errorDelegate?.Invoke(ex);
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
