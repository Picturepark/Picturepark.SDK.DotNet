using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface ITransferClient
	{
		/// <summary>Uploads multiple files from the filesystem.</summary>
		/// <param name="transferName">The name of the created transfer.</param>
		/// <param name="filePaths">The file paths on the filesystem.</param>
		/// <param name="uploadOptions">The file upload options.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The created transfer object.</returns>
		Task<Transfer> UploadFilesAsync(string transferName, IEnumerable<string> filePaths, UploadOptions uploadOptions, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>Uploads multiple files from the filesystem.</summary>
		/// <param name="transfer">The existing transfer object.</param>
		/// <param name="filePaths">The file paths on the filesystem.</param>
		/// <param name="uploadOptions">The file upload options.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The created transfer object.</returns>
		Task UploadFilesAsync(Transfer transfer, IEnumerable<string> filePaths, UploadOptions uploadOptions, CancellationToken cancellationToken = default(CancellationToken));

		Task ImportAndWaitForCompletionAsync(Transfer transfer, FileTransfer2ContentCreateRequest createRequest, CancellationToken cancellationToken = default(CancellationToken));

		Task<Transfer> CreateAndWaitForCompletionAsync(CreateTransferRequest request, CancellationToken cancellationToken = default(CancellationToken));

		Task<Transfer> CreateAndWaitForCompletionAsync(string transferName, IEnumerable<string> fileNames, CancellationToken cancellationToken = default(CancellationToken));
	}

	/// <summary>The file upload options.</summary>
	public class UploadOptions
	{
		/// <summary>Gets or sets the number of concurrent uploads (default: 4).</summary>
		public int ConcurrentUploads { get; set; } = 4;

		/// <summary>Gets or sets the chunk size (default: 1024 kb).</summary>
		public int ChunkSize { get; set; } = 1024 * 1024;

		/// <summary>Gets or sets a value indicating whether to wait for the completion of the transfer.</summary>
		public bool WaitForTransferCompletion { get; set; } = true;

		/// <summary>Gets or sets the success delegate which is called when a file has been uploaded.</summary>
		public Action<string> SuccessDelegate { get; set; } = null;

		/// <summary>Gets or sets the error delegate which is called when file upload failed.</summary>
		public Action<Exception> ErrorDelegate { get; set; } = null;
	}
}