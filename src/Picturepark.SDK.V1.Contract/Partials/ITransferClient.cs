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
			int chunkSize = 1024 * 1024,
			bool waitForTransferCompletion = true,
			Action<string> successDelegate = null,
			Action<Exception> errorDelegate = null
		);

		Task ImportTransferAsync(Transfer transfer, FileTransfer2ContentCreateRequest createRequest);

		Task<Transfer> CreateTransferAsync(CreateTransferRequest request);

		Task<Transfer> CreateTransferAsync(List<string> fileNames, string transferName);
	}
}