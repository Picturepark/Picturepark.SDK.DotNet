using System;

namespace Picturepark.SDK.V1.Contract
{
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