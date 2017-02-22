using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Clients;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
    public class TransferClient : TransfersClientBase
    {
        private readonly BusinessProcessClient _businessProcessClient;

        public TransferClient(BusinessProcessClient businessProcessClient, IAuthClient authClient)
            : base(authClient)
        {
            BaseUrl = businessProcessClient.BaseUrl;
            _businessProcessClient = businessProcessClient;
        }

        // TODO: Cleanup code
        public async Task UploadFilesAsync(
            IEnumerable<string> files,
            string exportDirectory,
            TransferViewItem transfer,
            int concurrentUploads = 4,
            bool waitForTransferCompletion = true,
            Action<string> successDelegate = null,
            Action<Exception> errorDelegate = null
            )
        {
            List<Task> allTasks = new List<Task>();
            var exceptions = new List<Exception>();

            // Limits concurrent downloads
            SemaphoreSlim throttler = new SemaphoreSlim(initialCount: concurrentUploads);

            foreach (var file in files)
            {
                await throttler.WaitAsync();
                allTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        string identifier = fileName;

                        long filesize = new FileInfo(file).Length;
                        int flowChunkNumber = 1;
                        long flowCurrentChunkSize = filesize;
                        string flowRelativePath = fileName;
                        int flowTotalChunks = 1;
                        long flowTotalSize = filesize;

                        using (FileStream fileStream = File.OpenRead(file))
                        {
                            await UploadFileAsync(transfer.Id, identifier, new FileParameter(fileStream, fileName), flowRelativePath, flowChunkNumber, flowCurrentChunkSize, flowTotalSize, flowTotalChunks);
                        }

                        if (successDelegate != null)
                            successDelegate.Invoke(file);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                        if (errorDelegate != null)
                            errorDelegate.Invoke(ex);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(allTasks).ConfigureAwait(false);

            if (exceptions.Any())
                throw new AggregateException(exceptions);

            if (waitForTransferCompletion)
            {
                var waitResult = await _businessProcessClient.WaitForStatesAsync(transfer.BusinessProcessId, TransferState.UploadCompleted.ToString(), 10 * 60 * 1000);
            }
        }

        public async Task ImportBatchAsync(TransferViewItem transfer, FileTransfer2AssetCreateRequest createRequest)
        {
            var importBatch = await ImportBatchAsync(transfer.Id, createRequest);
            var result = await _businessProcessClient.WaitForStatesAsync(importBatch.BusinessProcessId, TransferState.ImportCompleted.ToString(), 10 * 60 * 1000);
        }

        public async Task<TransferViewItem> CreateBatchAsync(CreateTransferRequest request)
        {
            var result = await CreateAsync(request);
            var waitResult = await _businessProcessClient.WaitForStatesAsync(result.BusinessProcessId, TransferState.Created.ToString(), 10 * 60 * 1000);

            return result;
        }

        public async Task<TransferViewItem> CreateBatchAsync(List<string> fileNames, string batchName)
        {
            var request = new CreateTransferRequest()
            {
                Name = string.IsNullOrEmpty(batchName) ? new Random().Next(1000, 9999).ToString() : batchName,
                TransferType = TransferType.FileUpload,
                Files = fileNames.Select(i => new TransferUploadFile { FileName = i, Identifier = Guid.NewGuid().ToString() }).ToList()
            };

            var result = await CreateAsync(request);
            var waitResult = await _businessProcessClient.WaitForStatesAsync(result.BusinessProcessId, TransferState.Created.ToString(), 10 * 60 * 1000);

            if (!waitResult.HasStateHit)
            {
                var exception = waitResult.BusinessProcess.StateHistory.SingleOrDefault(i => i.Error != null);
                throw new Exception(exception.Error.Exception);
            }

            return result;
        }

        internal async Task<string> GetFileTransferIdFromBatchTransferId(string batchTransferId)
        {
            var request = new FileTransferSearchRequest()
            {
                Limit = 1000
            };

            FileTransferSearchResult result = await SearchFilesAsync(request);
            string fileTransferId = result.Results.Where(i => i.TransferId == batchTransferId).Select(i => i.Id).FirstOrDefault();

            return fileTransferId;
        }
    }
}
