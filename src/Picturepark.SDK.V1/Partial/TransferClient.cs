using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using System.Net.Http;

namespace Picturepark.SDK.V1
{
    [Obsolete("These methods are deprecated and will be removed in future release. Please use Ingest methods for new projects and consider switching existing code to use Ingest methods as well.")]
    public partial class TransferClient
    {
        private static readonly SemaphoreSlim BlacklistCacheSemaphore = new SemaphoreSlim(1, 1);

        private readonly IBusinessProcessClient _businessProcessClient;
        private volatile ISet<string> _fileNameBlacklist;

        public TransferClient(IBusinessProcessClient businessProcessClient, IPictureparkServiceSettings settings, HttpClient httpClient)
            : this(settings, httpClient)
        {
            _businessProcessClient = businessProcessClient;
        }

        /// <summary>Searches files of a given transfer ID.</summary>
        /// <param name="transferId">The transfer ID.</param>
        /// <param name="limit">The maximum number of search results. Use null to retrieve all files in a transfer.</param>
        /// <returns>The result.</returns>
        public async Task<IReadOnlyCollection<FileTransfer>> SearchFilesByTransferIdAsync(string transferId, int? limit = null)
        {
            var results = new List<FileTransfer>();

            string pageToken = null;

            do
            {
                var request = new FileTransferSearchRequest()
                {
                    Limit = 500,
                    SearchString = "*",
                    PageToken = pageToken,
                    Filter = new TermFilter
                    {
                        Field = "transferId",
                        Term = transferId
                    }
                };

                var result = await SearchFilesAsync(request).ConfigureAwait(false);
                pageToken = result.PageToken;

                if (limit != null && results.Count + result.Results.Count > limit)
                    results.AddRange(result.Results.Take(limit.Value - results.Count));
                else
                    results.AddRange(result.Results);
            }
            while (pageToken != null && (limit == null || results.Count < limit.Value));

            return results;
        }

        /// <summary>Uploads multiple files from the filesystem.</summary>
        /// <param name="transferName">The name of the created transfer.</param>
        /// <param name="files">The file paths on the filesystem with optional overrides.</param>
        /// <param name="uploadOptions">The file upload options.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created transfer object.</returns>
        public async Task<CreateTransferResult> UploadFilesAsync(string transferName, IEnumerable<FileLocations> files, UploadOptions uploadOptions, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fileLocations = files as FileLocations[] ?? files.ToArray();

            var result = await CreateAndWaitForCompletionAsync(transferName, fileLocations, timeout, cancellationToken, uploadOptions).ConfigureAwait(false);
            if (result.Transfer == null)
                return result;

            await UploadFilesAsync(result.Transfer, fileLocations, uploadOptions, timeout, cancellationToken).ConfigureAwait(false);
            return result;
        }

        /// <summary>Uploads multiple files from the filesystem.</summary>
        /// <param name="transfer">The existing transfer object.</param>
        /// <param name="files">The file paths on the filesystem with optional overrides.</param>
        /// <param name="uploadOptions">The file upload options.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created transfer object.</returns>
        public async Task UploadFilesAsync(Transfer transfer, IEnumerable<FileLocations> files, UploadOptions uploadOptions, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            uploadOptions = uploadOptions ?? new UploadOptions();

            var filteredFileNames = await FilterFilesByBlacklist(files, uploadOptions.ErrorDelegate).ConfigureAwait(false);
            int atLeastOneFailed = 1;

            // Limit concurrent uploads
            // while limiting the chunks would be sufficient to enforce the concurrentUploads setting,
            // we should also not flood the system by prematurely enqueueing tasks for files
            using (var fileLimiter = new SemaphoreSlim(uploadOptions.ConcurrentUploads))
            {
                var tasks = new List<Task>();
                var exceptions = new List<Exception>();

                foreach (var file in filteredFileNames)
                {
                    var ourFile = file;
                    try
                    {
                        await fileLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                        uploadOptions.ErrorDelegate?.Invoke((file, ex));
                        break;
                    }

                    tasks.Add(Task.Run(async () =>
                    {
                        Exception caughtException = null;

                        try
                        {
                            await UploadFileAsync(transfer.Id, ourFile.Identifier, ourFile, cancellationToken).ConfigureAwait(false);
                            Interlocked.CompareExchange(ref atLeastOneFailed, 0, 1);
                        }
                        catch (Exception ex)
                        {
                            caughtException = ex;
                        }
                        finally
                        {
                            fileLimiter.Release();
                        }

                        // call user-supplied delegates outside of lock
                        try
                        {
                            if (caughtException == null)
                            {
                                uploadOptions.SuccessDelegate?.Invoke(ourFile);
                            }
                            else
                            {
                                if (uploadOptions.ErrorDelegate != null)
                                    uploadOptions.ErrorDelegate.Invoke((file, caughtException));
                                else
                                    exceptions.Add(caughtException);
                            }
                        }
                        catch
                        {
                            // exception from user-supplied delegate ignored
                        }
                    }));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                if (exceptions.Any())
                    throw new AggregateException(exceptions);
            }

            // all failed, cancel transfer and return
            if (atLeastOneFailed == 1)
            {
                await CancelAsync(transfer.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
                return;
            }

            if (uploadOptions.WaitForTransferCompletion)
            {
                await _businessProcessClient.WaitForCompletionAsync(transfer.BusinessProcessId, timeout, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>Transfers the uploaded files and waits for its completions.</summary>
        /// <param name="transfer">The transfer.</param>
        /// <param name="createRequest">The create request.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        public async Task ImportAndWaitForCompletionAsync(Transfer transfer, ImportTransferRequest createRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var importedTransfer = await ImportAsync(transfer.Id, createRequest, cancellationToken).ConfigureAwait(false);
            await _businessProcessClient.WaitForCompletionAsync(importedTransfer.BusinessProcessId, timeout, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Creates a transfer and waits for its completion.</summary>
        /// <param name="request">The create request.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The transfer.</returns>
        public async Task<CreateTransferResult> CreateAndWaitForCompletionAsync(CreateTransferRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var transfer = await CreateAsync(request, cancellationToken).ConfigureAwait(false);
            await _businessProcessClient.WaitForCompletionAsync(transfer.BusinessProcessId, timeout, cancellationToken: cancellationToken).ConfigureAwait(false);

            return new CreateTransferResult(transfer, request.Files);
        }

        /// <summary>Creates a transfer and waits for its completion.</summary>
        /// <param name="transferName">The name of the transfer.</param>
        /// <param name="files">The file names of the transfer.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="uploadOptions">The upload options.</param>
        /// <returns>The transfer.</returns>
        public async Task<CreateTransferResult> CreateAndWaitForCompletionAsync(string transferName, IEnumerable<FileLocations> files, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken), UploadOptions uploadOptions = null)
        {
            var filteredFileNames = await FilterFilesByBlacklist(files, uploadOptions?.ErrorDelegate).ConfigureAwait(false);
            if (!filteredFileNames.Any())
            {
                return new CreateTransferResult(null, Enumerable.Empty<TransferUploadFile>());
            }

            var request = new CreateTransferRequest
            {
                Name = !string.IsNullOrEmpty(transferName) ? transferName : new Random().Next(1000, 9999).ToString(),
                TransferType = TransferType.FileUpload,
                Files = filteredFileNames.Select(f => new TransferUploadFile
                {
                    RequestId = f.Identifier,
                    FileName = f.UploadAs
                }).ToList()
            };

            var transfer = await CreateAsync(request, cancellationToken).ConfigureAwait(false);
            await _businessProcessClient.WaitForStatesAsync(transfer.BusinessProcessId, new[] { TransferState.Created.ToString() }, timeout, cancellationToken).ConfigureAwait(false);

            return new CreateTransferResult(transfer, request.Files);
        }

        public async Task CancelTransferAsync(string id, CancellationToken cancellationToken = default)
            => await CancelAsync(id, cancellationToken).ConfigureAwait(false);

        public async Task<Transfer> ImportTransferAsync(string id, ImportTransferRequest request, CancellationToken cancellationToken = default)
            => await ImportAsync(id, request, cancellationToken).ConfigureAwait(false);

        public async Task UploadFileAsync(long chunkNumber, long currentChunkSize, long totalSize, long totalChunks, string transferId, string requestId, FileParameter formFile = null, CancellationToken cancellationToken = default)
        {
            if (formFile == null)
                throw new ArgumentNullException(nameof(formFile), "Parameter `formFile` cannot be null. You're probably using an obsolete method call.");

            await UploadFileAsync(chunkNumber, currentChunkSize, totalSize, totalChunks, transferId, requestId, formFile.Data, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task UploadFileAsync(
            FileParameter formFile,
            long chunkNumber,
            long currentChunkSize,
            long totalSize,
            long totalChunks,
            string transferId,
            string requestId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await UploadFileAsync(chunkNumber, currentChunkSize, totalSize, totalChunks, transferId, requestId, formFile.Data, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task UploadFileAsync(string transferId, string identifier, FileLocations fileLocation, CancellationToken cancellationToken = default)
        {
            var fileSize = new FileInfo(fileLocation.AbsoluteSourcePath).Length;

            using (var fileStream = File.OpenRead(fileLocation.AbsoluteSourcePath))
                await UploadFileAsync(1, fileSize, fileSize, 1, transferId, identifier, fileStream, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IEnumerable<FileLocations>> FilterFilesByBlacklist(IEnumerable<FileLocations> files, Action<(FileLocations, Exception)> errorDelegate)
        {
            var blacklist = await GetCachedBlacklist().ConfigureAwait(false);
            var okFiles = new List<FileLocations>();

            foreach (var file in files)
            {
                if (blacklist.Contains(Path.GetFileName(file.AbsoluteSourcePath)?.ToLowerInvariant()))
                {
                    errorDelegate?.Invoke((file, new ArgumentException($"File {file} cannot be uploaded, file name is blacklisted")));
                    continue;
                }

                okFiles.Add(file);
            }

            return okFiles;
        }

        private async Task<ISet<string>> GetCachedBlacklist()
        {
            if (_fileNameBlacklist != null)
                return _fileNameBlacklist;

            await BlacklistCacheSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_fileNameBlacklist != null)
                    return _fileNameBlacklist;

                var blacklist = await GetBlacklistAsync().ConfigureAwait(false);
                _fileNameBlacklist = new HashSet<string>(blacklist.Items.Select(i => i.Match.ToLowerInvariant()));
            }
            finally
            {
                BlacklistCacheSemaphore.Release();
            }

            return _fileNameBlacklist;
        }
    }
}
