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

            var result = await CreateAndWaitForCompletionAsync(transferName, fileLocations, timeout, cancellationToken).ConfigureAwait(false);
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

            var filteredFileNames = await FilterFilesByBlacklist(files).ConfigureAwait(false);

            // Limit concurrent uploads
            // while limiting the chunks would be sufficient to enforce the concurrentUploads setting,
            // we should also not flood the system by prematurely enqueueing tasks for files
            using (var fileLimiter = new SemaphoreSlim(uploadOptions.ConcurrentUploads))
            using (var chunkLimiter = new SemaphoreSlim(uploadOptions.ConcurrentUploads))
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
                        uploadOptions.ErrorDelegate?.Invoke(ex);
                        break;
                    }

                    tasks.Add(Task.Run(async () =>
                    {
                        Exception caughtException = null;

                        try
                        {
                            await UploadFileAsync(chunkLimiter, transfer.Id, ourFile.Identifier, ourFile, uploadOptions.ChunkSize, cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
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
                                uploadOptions.SuccessDelegate?.Invoke(ourFile);
                            else
                                uploadOptions.ErrorDelegate?.Invoke(caughtException);
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
            var importedTransfer = await ImportTransferAsync(transfer.Id, createRequest, cancellationToken).ConfigureAwait(false);
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
        /// <returns>The transfer.</returns>
        public async Task<CreateTransferResult> CreateAndWaitForCompletionAsync(string transferName, IEnumerable<FileLocations> files, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filteredFileNames = await FilterFilesByBlacklist(files).ConfigureAwait(false);

            var request = new CreateTransferRequest
            {
                Name = !string.IsNullOrEmpty(transferName) ? transferName : new Random().Next(1000, 9999).ToString(),
                TransferType = TransferType.FileUpload,
                Files = filteredFileNames.Select(f => new TransferUploadFile
                {
                    Identifier = f.Identifier,
                    FileName = f.UploadAs
                }).ToList()
            };

            var transfer = await CreateAsync(request, cancellationToken).ConfigureAwait(false);
            await _businessProcessClient.WaitForStatesAsync(transfer.BusinessProcessId, new[] { TransferState.Created.ToString() }, timeout, cancellationToken).ConfigureAwait(false);

            return new CreateTransferResult(transfer, request.Files);
        }

        private async Task UploadFileAsync(SemaphoreSlim chunkLimiter, string transferId, string identifier, FileLocations fileLocation, int chunkSize, CancellationToken cancellationToken = default)
        {
            var fileSize = new FileInfo(fileLocation.AbsoluteSourcePath).Length;
            var totalChunks = (long)Math.Ceiling((decimal)fileSize / chunkSize);

            var uploadTasks = new List<Task>();

            using (var perFileCts = new CancellationTokenSource())
            using (cancellationToken.Register(perFileCts.Cancel))
            {
                var perFileCt = perFileCts.Token;

                for (var chunkNumber = 1; chunkNumber <= totalChunks; chunkNumber++)
                {
                    var ourChunkNumber = chunkNumber;
                    var acquiredSemaphore = await WaitSemaphoreAsync(chunkLimiter, perFileCt).ConfigureAwait(false);

                    // don't enqueue any additional tasks for this file
                    if (!acquiredSemaphore)
                        break;

                    uploadTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await UploadChunkAsync(transferId, identifier, fileLocation, chunkSize, ourChunkNumber, totalChunks, perFileCt).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && perFileCt.IsCancellationRequested)
                        {
                            // we already have the real exception, no need to also record all the cancellations we triggered ourselves
                        }
                        catch
                        {
                            perFileCts.Cancel();
                            throw;
                        }
                        finally
                        {
                            chunkLimiter.Release();
                        }
                    }));
                }

                await Task.WhenAll(uploadTasks).ConfigureAwait(false);
            }
        }

        private async Task UploadChunkAsync(
            string transferId,
            string identifier,
            FileLocations fileLocation,
            int chunkSize,
            int number,
            long totalChunks,
            CancellationToken ct)
        {
            var sourceFileName = Path.GetFileName(fileLocation.AbsoluteSourcePath);

            using (var fileStream = File.OpenRead(fileLocation.AbsoluteSourcePath))
            {
                var fileSize = fileStream.Length;
                var position = (number - 1) * (long)chunkSize;

                // last chunk may have a different size.
                // not using long because
                // - fileStream.Read's count argument is int
                // - position for the last chunk is already close to the end of the file
                if (number == totalChunks)
                    chunkSize = (int)(fileSize - position);

                var buffer = new byte[chunkSize];
                fileStream.Position = position;

                await fileStream.ReadAsync(buffer, 0, chunkSize, ct).ConfigureAwait(false);

                using (var memoryStream = new MemoryStream(buffer))
                {
                    await UploadFileAsync(
                        number,
                        chunkSize,
                        fileSize,
                        totalChunks,
                        transferId,
                        identifier,
                        new FileParameter(memoryStream, sourceFileName),
                        ct).ConfigureAwait(false);
                }
            }
        }

        private async Task<bool> WaitSemaphoreAsync(SemaphoreSlim semaphore, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return false;

            try
            {
                await semaphore.WaitAsync(ct).ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        private async Task<IEnumerable<FileLocations>> FilterFilesByBlacklist(IEnumerable<FileLocations> files)
        {
            var blacklist = await GetCachedBlacklist().ConfigureAwait(false);
            return files.Where(i => !blacklist.Contains(Path.GetFileName(i.AbsoluteSourcePath)?.ToLowerInvariant()));
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
