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
			Transfer transfer,
			int concurrentUploads = 4,
			bool waitForTransferCompletion = true,
			Action<string> successDelegate = null,
			Action<Exception> errorDelegate = null
		);

		Task ImportBatchAsync(Transfer transfer, FileTransfer2ContentCreateRequest createRequest);

		Task<Transfer> CreateBatchAsync(CreateTransferRequest request);

		Task<Transfer> CreateBatchAsync(List<string> fileNames, string batchName);
	}
}