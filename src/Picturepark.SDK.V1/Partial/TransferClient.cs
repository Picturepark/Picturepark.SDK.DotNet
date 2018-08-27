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

        private readonly BusinessProcessClient _businessProcessClient;
        private volatile ISet<string> _fileNameBlacklist;

        public TransferClient(BusinessProcessClient businessProcessClient, IPictureparkServiceSettings settings, HttpClient httpClient)
            : this(settings, httpClient)
        {
            _businessProcessClient = businessProcessClient;
        }

        /// <summary>Searches files of a given transfer ID.</summary>
        /// <param name="transferId">The transfer ID.</param>
        /// <param name="limit">The maximum number of search results.</param>
        /// <returns>The result.</returns>
        public async Task<FileTransferSearchResult> SearchFilesByTransferIdAsync(string transferId, int limit = 20)
        {
            var request = new FileTransferSearchRequest()
            {
                Limit = limit,
                SearchString = "*",
                Filter = new TermFilter
                {
                    Field = "transferId",
                    Term = transferId
                }
            };

            return await SearchFilesAsync(request).ConfigureAwait(false);
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

            var exceptions = new List<Exception>();

            // Limits concurrent uploads
            var throttler = new SemaphoreSlim(uploadOptions.ConcurrentUploads);
            var filteredFileNames = await FilterFilesByBlacklist(files).ConfigureAwait(false);

            var tasks = filteredFileNames
                .Select(file => Task.Run(async () =>
                {
                    try
                    {
                        await UploadFileAsync(throttler, transfer.Id, file.Identifier, file, uploadOptions.ChunkSize, cancellationToken).ConfigureAwait(false);
                        uploadOptions.SuccessDelegate?.Invoke(file);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                        uploadOptions.ErrorDelegate?.Invoke(ex);
                    }
                }));

            await Task.WhenAll(tasks).ConfigureAwait(false);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }

            if (uploadOptions.WaitForTransferCompletion)
            {
                await _businessProcessClient.WaitForCompletionAsync(transfer.BusinessProcessId, timeout, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>Transfers the uploaded files and waits for its completions.</summary>
        /// <param name="transfer">The transfer.</param>
        /// <param name="createRequest">The create request.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellcation token.</param>
        /// <returns>The task.</returns>
        public async Task ImportAndWaitForCompletionAsync(Transfer transfer, ImportTransferRequest createRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var importedTransfer = await ImportTransferAsync(transfer.Id, createRequest, cancellationToken).ConfigureAwait(false);
            await _businessProcessClient.WaitForCompletionAsync(importedTransfer.BusinessProcessId, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Creates a transfer and waits for its completion.</summary>
        /// <param name="request">The create request.</param>
        /// <param name="timeout">The timeout to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The transfer.</returns>
        public async Task<CreateTransferResult> CreateAndWaitForCompletionAsync(CreateTransferRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var transfer = await CreateAsync(request, cancellationToken).ConfigureAwait(false);
            await _businessProcessClient.WaitForCompletionAsync(transfer.BusinessProcessId, timeout, cancellationToken).ConfigureAwait(false);

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

        private async Task UploadFileAsync(SemaphoreSlim throttler, string transferId, string identifier, FileLocations fileLocation, int chunkSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sourceFileName = Path.GetFileName(fileLocation.AbsoluteSourcePath);

            var fileSize = new FileInfo(fileLocation.AbsoluteSourcePath).Length;
            var targetFileName = Path.GetFileName(fileLocation.UploadAs);
            var totalChunks = (long)Math.Ceiling((decimal)fileSize / chunkSize);

            var uploadTasks = new List<Task>();

            for (var chunkNumber = 1; chunkNumber <= totalChunks; chunkNumber++)
            {
                await throttler.WaitAsync(cancellationToken).ConfigureAwait(false);

                var number = chunkNumber;
                uploadTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (var fileStream = File.OpenRead(fileLocation.AbsoluteSourcePath))
                        {
                            var currentChunkSize = chunkSize;
                            var position = (number - 1) * chunkSize;

                            // last chunk may have a different size.
                            if (number == totalChunks)
                            {
                                currentChunkSize = (int)(fileSize - position);
                            }

                            var buffer = new byte[currentChunkSize];
                            fileStream.Position = position;

                            await fileStream.ReadAsync(buffer, 0, currentChunkSize, cancellationToken).ConfigureAwait(false);

                            using (var memoryStream = new MemoryStream(buffer))
                            {
                                await UploadFileAsync(
                                    targetFileName,
                                    number,
                                    currentChunkSize,
                                    fileSize,
                                    totalChunks,
                                    transferId,
                                    identifier,
                                    new FileParameter(memoryStream, sourceFileName),
                                    cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(uploadTasks).ConfigureAwait(false);
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
