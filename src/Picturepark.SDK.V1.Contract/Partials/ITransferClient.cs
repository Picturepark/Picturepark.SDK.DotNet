using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface ITransferClient
	{
		Task UploadFilesAsync(
			IEnumerable<string> files,
			string exportDirectory,
			TransferViewItem transfer,
			int concurrentUploads = 4,
			bool waitForTransferCompletion = true,
			Action<string> successDelegate = null,
			Action<Exception> errorDelegate = null
		);

		Task ImportBatchAsync(TransferViewItem transfer, FileTransfer2ContentCreateRequest createRequest);

		Task<TransferViewItem> CreateBatchAsync(CreateTransferRequest request);

		Task<TransferViewItem> CreateBatchAsync(List<string> fileNames, string batchName);
	}
}