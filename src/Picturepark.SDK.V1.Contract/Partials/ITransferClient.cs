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

		/// <summary>Transfers the uploaded files and waits for its completions.</summary>
		/// <param name="transfer">The transfer.</param>
		/// <param name="createRequest">The create request.</param>
		/// <param name="timeout">The timeout in ms to wait for completion.</param>
		/// <param name="cancellationToken">The cancellcation token.</param>
		/// <returns>The task.</returns>
		Task ImportAndWaitForCompletionAsync(Transfer transfer, FileTransfer2ContentCreateRequest createRequest, int timeout = 60 * 1000, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>Creates a transfer and waits for its completion.</summary>
		/// <param name="request">The create request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The transfer.</returns>
		Task<Transfer> CreateAndWaitForCompletionAsync(CreateTransferRequest request, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>Creates a transfer and waits for its completion.</summary>
		/// <param name="transferName">The name of the transfer.</param>
		/// <param name="fileNames">The file names of the transfer.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The transfer.</returns>
		Task<Transfer> CreateAndWaitForCompletionAsync(string transferName, IEnumerable<string> fileNames, CancellationToken cancellationToken = default(CancellationToken));
	}
}