using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Results;
using Picturepark.SDK.V1.Partial;

namespace Picturepark.SDK.V1.AzureBlob;

/// <summary>
/// Convenience extensions for <see cref="IIngestClient"/>
/// </summary>
public static class IngestClientExtensions
{
    /// <summary>
    /// Uploads files from local filesystem and imports them into Content Platform.
    /// </summary>
    /// <param name="client">Ingest client</param>
    /// <param name="filePaths">Path to files</param>
    /// <param name="requestForAllFiles">Metadata, permission sets etc. to assign for all files on import</param>
    /// <param name="uploadOptions">Upload options</param>
    /// <param name="importOptions">Import options</param>
    /// <param name="timeout">Time to wait for business process completion</param>
    /// <param name="waitSearchDocCreation">Indicates if search document creation should be awaited</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>ContentBatchOperationWithRequestIdResult</returns>
    public static async Task<ContentBatchOperationWithRequestIdResult> UploadAndImportFilesAsync(
        this IIngestClient client,
        IEnumerable<string> filePaths,
        FileImportRequest? requestForAllFiles = null,
        IngestUploadOptions? uploadOptions = null,
        ImportOptions? importOptions = null,
        TimeSpan? timeout = null,
        bool waitSearchDocCreation = true,
        CancellationToken ct = default)
    {
        var fileLocations = await client.UploadFilesAsync(filePaths, uploadOptions, ct).ConfigureAwait(false);
        return await client.ImportFilesAsync(fileLocations, importOptions, requestForAllFiles, timeout, waitSearchDocCreation, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Uploads items and imports them into Content Platform.
    /// </summary>
    /// <param name="client">Ingest client</param>
    /// <param name="items">Items to upload</param>
    /// <param name="requestForAllItems">Metadata, permission sets etc. to assign for all files on import</param>
    /// <param name="uploadOptions">Upload options</param>
    /// <param name="importOptions">Import options</param>
    /// <param name="timeout">Time to wait for business process completion</param>
    /// <param name="waitSearchDocCreation">Indicates if search document creation should be awaited</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>ContentBatchOperationWithRequestIdResult</returns>
    public static async Task<ContentBatchOperationWithRequestIdResult> UploadAndImportFilesAsync(
        this IIngestClient client,
        IEnumerable<IngestUploadItem> items,
        FileImportRequest? requestForAllItems = null,
        IngestUploadOptions? uploadOptions = null,
        ImportOptions? importOptions = null,
        TimeSpan? timeout = null,
        bool waitSearchDocCreation = true,
        CancellationToken ct = default)
    {
        var fileLocations = await client.UploadFilesAsync(items, uploadOptions, ct).ConfigureAwait(false);
        return await client.ImportFilesAsync(fileLocations, importOptions, requestForAllItems, timeout, waitSearchDocCreation, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Upload items to ingest container
    /// </summary>
    /// <remarks>
    /// This only uploads items, to import them, call one of the Import* methods later.
    /// </remarks>
    /// <param name="client">Ingest client</param>
    /// <param name="items">Items to upload</param>
    /// <param name="options">Upload options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of <see cref="IngestFile"/> instances</returns>
    public static async Task<IReadOnlyCollection<IngestFile>> UploadFilesAsync(
        this IIngestClient client,
        IEnumerable<IngestUploadItem> items,
        IngestUploadOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new IngestUploadOptions();
        if (options.ConcurrentUploads < 1)
            throw new ArgumentOutOfRangeException(nameof(options.ConcurrentUploads), "At least one concurrent upload is required");

        var container = await client.CreateIngestContainerAsync(ct).ConfigureAwait(false);
        var containerClient = new BlobContainerClient(new Uri(container.SasToken));

        var fileQueue = new ConcurrentQueue<IngestUploadItem>(items);
        var result = new ConcurrentBag<IngestFile>();

        var consumers = Enumerable.Range(0, options.ConcurrentUploads).Select(
            _ => Task.Run(
                async () =>
                {
                    while (fileQueue.TryDequeue(out var item))
                    {
                        var (fileName, openStream, leaveStreamOpen) = item;

                        var blobClient = containerClient.GetBlobClient(fileName);
                        var stream = openStream();

                        try
                        {
                            await blobClient.UploadAsync(
                                stream,
                                options?.BlobUploadOptions ?? new BlobUploadOptions(),
                                ct).ConfigureAwait(false);

                            result.Add(new IngestFile { ContainerName = container.ContainerName, BlobName = fileName });
                        }
                        finally
                        {
                            if (!leaveStreamOpen)
                                stream.Dispose();
                        }
                    }
                },
                CancellationToken.None));

        await Task.WhenAll(consumers).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Upload files from local filesystem to ingest container
    /// </summary>
    /// <remarks>
    /// This only uploads files, to import them, call one of the Import* methods later.
    /// </remarks>
    /// <param name="client">Ingest client</param>
    /// <param name="filePaths">Path to files</param>
    /// <param name="options">Upload options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of <see cref="IngestFile"/> instances</returns>
    public static async Task<IReadOnlyCollection<IngestFile>> UploadFilesAsync(
        this IIngestClient client,
        IEnumerable<string> filePaths,
        IngestUploadOptions? options = null,
        CancellationToken ct = default)
    {
        return await client.UploadFilesAsync(
            filePaths.Select(
                fp =>
                {
                    var fileName = Path.IsPathRooted(fp) ? Path.GetFileName(fp) : fp;
                    return new IngestUploadItem(fileName, () => File.OpenRead(fp), leaveStreamOpen: false);
                }),
            options,
            ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Uploads a file from local filesystem to ingest container
    /// </summary>
    /// <remarks>
    /// This only uploads the file, to import it, call one of the Import* methods later.
    /// </remarks>
    /// <param name="client">Ingest client</param>
    /// <param name="filePath">Path to file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns><see cref="IngestFile"/> instance</returns>
    public static async Task<IngestFile> UploadFileAsync(
        this IIngestClient client,
        string filePath,
        CancellationToken ct = default)
    {
        var result = await client.UploadFilesAsync(new[] { filePath }, options: null, ct).ConfigureAwait(false);
        return result.Single();
    }

    /// <summary>
    /// Uploads a single item to ingest container
    /// </summary>
    /// <remarks>
    /// This only uploads the item, to import it, call one of the Import* methods later.
    /// </remarks>
    /// <param name="client">Ingest client</param>
    /// <param name="item">Item to upload</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns><see cref="IngestFile"/> instance</returns>
    public static async Task<IngestFile> UploadFileAsync(
        this IIngestClient client,
        IngestUploadItem item,
        CancellationToken ct = default)
    {
        var result = await client.UploadFilesAsync(new[] { item }, options: null, ct).ConfigureAwait(false);
        return result.Single();
    }

    /// <summary>
    /// Imports files from container into Content Platform.
    /// </summary>
    /// <param name="client">Ingest client</param>
    /// <param name="files">Files to import</param>
    /// <param name="options">Options for import</param>
    /// <param name="requestForAllFiles">Metadata, permission sets etc. to assign for all files on import</param>
    /// <param name="timeout">Time to wait for business process completion</param>
    /// <param name="waitSearchDocCreation">Indicates if search document creation should be awaited</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>ContentBatchOperationWithRequestIdResult</returns>
    /// <remarks>All files must reside in the same ingest container otherwise <see cref="InvalidOperationException"/> is thrown.</remarks>
    public static async Task<ContentBatchOperationWithRequestIdResult> ImportFilesAsync(
        this IIngestClient client,
        IReadOnlyCollection<IngestFile> files,
        ImportOptions? options = null,
        FileImportRequest? requestForAllFiles = null,
        TimeSpan? timeout = null,
        bool waitSearchDocCreation = true,
        CancellationToken ct = default)
    {
        return await client.ImportFilesAsync(
            files.ToDictionary(
                f => f,
                f => new FileImportWithFileNameOverrideRequest
                {
                    FileNameOverride = f.FileNameOverride,
                    LayerSchemaIds = requestForAllFiles?.LayerSchemaIds,
                    DisplayContentId = requestForAllFiles?.DisplayContentId,
                    Metadata = requestForAllFiles?.Metadata,
                    ContentPermissionSetIds = requestForAllFiles?.ContentPermissionSetIds
                }),
            options,
            timeout,
            waitSearchDocCreation,
            ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Imports files from container into Content Platform allowing assignment of different metadata for each file.
    /// </summary>
    /// <param name="client">Ingest client</param>
    /// <param name="requests">Request for each file</param>
    /// <param name="options">Import options</param>
    /// <param name="timeout">Time to wait for business process completion</param>
    /// <param name="waitSearchDocCreation">Indicates if search document creation should be awaited</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>ContentBatchOperationWithRequestIdResult</returns>
    /// <remarks>All items must reside in the same ingest container otherwise <see cref="InvalidOperationException"/> is thrown.</remarks>
    public static async Task<ContentBatchOperationWithRequestIdResult> ImportFilesAsync(
        this IIngestClient client,
        IDictionary<IngestFile, FileImportWithFileNameOverrideRequest> requests,
        ImportOptions? options = null,
        TimeSpan? timeout = null,
        bool waitSearchDocCreation = true,
        CancellationToken ct = default)
    {
        if (!requests.Any())
            return ContentBatchOperationWithRequestIdResult.Empty;

        var containerNames = new HashSet<string>(requests.Keys.Select(f => f.ContainerName));
        if (containerNames.Count > 1)
            throw new InvalidOperationException("Only files from the same container can be imported in one request");

        var containerName = containerNames.Single();
        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentNullException(nameof(IngestFile.ContainerName));

        if (client is not InternalIngestClient internalClient)
            throw new InvalidArgumentException();

        var businessProcess = await client.ImportPartialAsync(
            containerName,
            new ImportPartialFromContainerRequest
            {
                Items = requests.ToDictionary(r => r.Key.BlobName, r => r.Value)
            },
            ct).ConfigureAwait(false);

        var result = await internalClient.BusinessProcessClient.WaitForCompletionAsync(businessProcess.Id, timeout, waitSearchDocCreation, ct)
            .ConfigureAwait(false);

        return new ContentBatchOperationWithRequestIdResult(
            internalClient.ContentClient,
            businessProcess.Id,
            result.LifeCycleHit,
            internalClient.BusinessProcessClient);
    }
}
