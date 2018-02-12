using System.Collections.Generic;

namespace Picturepark.SDK.V1.Contract
{
	/// <summary>The create transfer result.</summary>
	public class CreateTransferResult
	{
		/// <summary>Initializes a new instance of the <see cref="CreateTransferResult"/> class.</summary>
		/// <param name="transfer">The transfer.</param>
		/// <param name="fileUploads">The file uploads.</param>
		public CreateTransferResult(Transfer transfer, IEnumerable<TransferUploadFile> fileUploads)
		{
			Transfer = transfer;
			FileUploads = fileUploads;
		}

		/// <summary>Gets the transfer.</summary>
		public Transfer Transfer { get; }

		/// <summary>Gets the file uploads.</summary>
		public IEnumerable<TransferUploadFile> FileUploads { get; }
	}
}