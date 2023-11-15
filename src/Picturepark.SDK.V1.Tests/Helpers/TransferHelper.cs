using System;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Helpers
{
    public static class TransferHelper
    {
        public static async Task<(CreateTransferResult, string fileId)> CreateSingleFileTransferAsync(IPictureparkService client, string filePath, UploadOptions uploadOptions = null)
        {
            var transferName = new Random().Next(1000, 9999).ToString();
            var files = new FileLocations[] { filePath };

            var createTransferResult = await client.Transfer.CreateAndWaitForCompletionAsync(transferName, files);
            await client.Transfer.UploadFilesAsync(createTransferResult.Transfer, files, uploadOptions ?? new UploadOptions());

            var searchRequest = new FileTransferSearchRequest
            {
                Limit = 1,
                SearchString = "*",
                Filter = FilterBase.FromExpression<FileTransfer>(i => i.TransferId, createTransferResult.Transfer.Id)
            };
            var searchResult = await client.Transfer.SearchFilesAsync(searchRequest);
            var fileId = searchResult.Results.ToList()[0].Id;

            return (createTransferResult, fileId);
        }
    }
}
