using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface ITransferClient
	{
		Task<Transfer> UploadFilesAsync(string transferName, IEnumerable<string> filePaths, UploadOptions uploadOptions);

		Task UploadFilesAsync(Transfer transfer, IEnumerable<string> filePaths, UploadOptions uploadOptions);

		Task ImportTransferAsync(Transfer transfer, FileTransfer2ContentCreateRequest createRequest);

		Task<Transfer> CreateTransferAsync(CreateTransferRequest request);

		Task<Transfer> CreateTransferAsync(List<string> fileNames, string transferName);
	}

	public class UploadOptions
	{
		public int ConcurrentUploads { get; set; } = 4;

		public int ChunkSize { get; set; } = 1024 * 1024;

		public bool WaitForTransferCompletion { get; set; } = true;

		public Action<string> SuccessDelegate { get; set; } = null;

		public Action<Exception> ErrorDelegate { get; set; } = null;
	}
}