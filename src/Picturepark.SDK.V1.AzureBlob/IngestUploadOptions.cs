using Azure.Storage.Blobs.Models;

namespace Picturepark.SDK.V1.AzureBlob;

/// <summary>
/// Options for uploading to ingest containers.
/// </summary>
public class IngestUploadOptions
{
    /// <summary>
    /// Number of concurrent uploads.
    /// </summary>
    public int ConcurrentUploads { get; set; } = 4;

    /// <summary>
    /// Options for uploading each blob.
    /// </summary>
    /// <remarks>Use this to attach a progress handler.</remarks>
    public BlobUploadOptions? BlobUploadOptions { get; set; }
}
