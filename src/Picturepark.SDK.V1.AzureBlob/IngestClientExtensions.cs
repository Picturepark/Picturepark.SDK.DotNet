using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Results;
using Picturepark.SDK.V1.Partial;

namespace Picturepark.SDK.V1.AzureBlob;

public static class IngestClientExtensions
{
    public static async Task<IReadOnlyCollection<IngestFile>> UploadFilesAsync(
        this IIngestClient client,
        IEnumerable<string> files,
        CancellationToken ct = default)
    {
        var items = new List<IngestFile>();

        var container = await client.CreateIngestContainerAsync(ct).ConfigureAwait(false);

        var containerClient = new BlobContainerClient(new Uri(container.SasToken));

        foreach (var file in files)
        {
            var fileName = Path.IsPathRooted(file) ? Path.GetFileName(file) : file;
            var blobClient = containerClient.GetBlobClient(fileName);

            using var fs = File.OpenRead(file);
            await blobClient.UploadAsync(fs, ct).ConfigureAwait(false);

            items.Add(new IngestFile { ContainerName = container.ContainerName, BlobName = fileName });
        }

        return items;
    }

    public static async Task<IngestFile> UploadFileAsync(
        this IIngestClient client,
        string filePath,
        CancellationToken ct = default)
    {
        var fileName = Path.IsPathRooted(filePath) ? Path.GetFileName(filePath) : filePath;

        using var fs = File.OpenRead(filePath);
        return await client.UploadFileAsync(fileName, fs, ct).ConfigureAwait(false);
    }

    public static async Task<IngestFile> UploadFileAsync(
        this IIngestClient client,
        string fileName,
        Stream file,
        CancellationToken ct = default)
    {
        var container = await client.CreateIngestContainerAsync(ct).ConfigureAwait(false);

        var containerClient = new BlobContainerClient(new Uri(container.SasToken));
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(file, ct).ConfigureAwait(false);

        return new IngestFile { ContainerName = container.ContainerName, BlobName = fileName };
    }

    public static async Task<ContentBatchOperationWithRequestIdResult> ImportFilesAsync(
        this IIngestClient client,
        IReadOnlyCollection<IngestFile> files,
        FileImportRequest? metadataForAllFiles = null,
        TimeSpan? timeout = null,
        bool waitSearchDocCreation = true,
        CancellationToken ct = default)
    {
        if (!files.Any())
            return ContentBatchOperationWithRequestIdResult.Empty;

        var containerNames = new HashSet<string>(files.Select(f => f.ContainerName));
        if (containerNames.Count > 1)
            throw new InvalidOperationException("Only files from the same container can be imported in one request");

        var containerName = containerNames.Single();
        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentNullException(nameof(IngestFile.ContainerName));

        return await client.ImportFilesAsync(
            () => client.ImportAllAsync(
                containerName,
                new ImportAllFromContainerRequest
                {
                    LayerSchemaIds = metadataForAllFiles?.LayerSchemaIds,
                    DisplayContentId = metadataForAllFiles?.DisplayContentId,
                    Metadata = metadataForAllFiles?.Metadata,
                    ContentPermissionSetIds = metadataForAllFiles?.ContentPermissionSetIds
                },
                ct),
            timeout,
            waitSearchDocCreation,
            ct).ConfigureAwait(false);
    }

    public static async Task<ContentBatchOperationWithRequestIdResult> ImportFilesAsync(
        this IIngestClient client,
        IReadOnlyCollection<string> files,
        FileImportRequest? metadataForAllFiles = null,
        TimeSpan? timeout = null,
        bool waitSearchDocCreation = true,
        CancellationToken ct = default)
    {
        var fileLocations = await client.UploadFilesAsync(files, ct).ConfigureAwait(false);
        return await client.ImportFilesAsync(fileLocations, metadataForAllFiles, timeout, waitSearchDocCreation, ct).ConfigureAwait(false);
    }

    public static async Task<ContentBatchOperationWithRequestIdResult> ImportFilesAsync(
        this IIngestClient client,
        IDictionary<IngestFile, FileImportWithFileNameOverrideRequest> requests,
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

        return await client.ImportFilesAsync(
            () => client.ImportPartialAsync(
                containerName,
                new ImportPartialFromContainerRequest
                {
                    Items = requests.ToDictionary(r => r.Key.BlobName, r => r.Value)
                },
                ct),
            timeout,
            waitSearchDocCreation,
            ct).ConfigureAwait(false);
    }

    private static async Task<ContentBatchOperationWithRequestIdResult> ImportFilesAsync(
        this IIngestClient client,
        Func<Task<BusinessProcess>> businessProcessFactory,
        TimeSpan? timeout = null,
        bool waitSearchDocCreation = true,
        CancellationToken ct = default)
    {
        if (client is not InternalIngestClient internalClient)
            throw new InvalidArgumentException();

        var businessProcess = await businessProcessFactory().ConfigureAwait(false);

        var result = await internalClient.BusinessProcessClient.WaitForCompletionAsync(businessProcess.Id, timeout, waitSearchDocCreation, ct)
            .ConfigureAwait(false);

        return new ContentBatchOperationWithRequestIdResult(
            internalClient.ContentClient,
            businessProcess.Id,
            result.LifeCycleHit,
            internalClient.BusinessProcessClient);
    }
}
