using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface ITransferClient
	{
		Task<Transfer> UploadFilesAsync(string transferName, IEnumerable<string> filePaths, UploadOptions uploadOptions, CancellationToken cancellationToken = default(CancellationToken));

		Task UploadFilesAsync(Transfer transfer, IEnumerable<string> filePaths, UploadOptions uploadOptions, CancellationToken cancellationToken = default(CancellationToken));

		Task ImportAndWaitForCompletionAsync(Transfer transfer, FileTransfer2ContentCreateRequest createRequest, CancellationToken cancellationToken = default(CancellationToken));

		Task<Transfer> CreateAndWaitForCompletionAsync(CreateTransferRequest request, CancellationToken cancellationToken = default(CancellationToken));

		Task<Transfer> CreateAndWaitForCompletionAsync(string transferName, IEnumerable<string> fileNames, CancellationToken cancellationToken = default(CancellationToken));
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