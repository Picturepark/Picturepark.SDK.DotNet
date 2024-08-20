using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Picturepark.SDK.V1.AzureBlob;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Results;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients;

[Trait("Stack", "Ingest")]
public class IngestTests : IClassFixture<ClientFixture>
{
    private readonly ClientFixture _fixture;

    public IngestTests(ClientFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_import_files()
    {
        var tempFile = Path.GetTempFileName() + ".txt";
        var filename = Path.GetFileName(tempFile);

        await File.WriteAllTextAsync(tempFile, "Hello world");

        var result = await _fixture.Client.Ingest.UploadAndImportFilesAsync(new[] { tempFile });
        var detail = await result.FetchDetail();

        detail.SucceededItems.Should().ContainSingle(c => c.RequestId == filename);
    }

    [Fact]
    public async Task Should_import_files_with_metadata()
    {
        // Arrange
        const string filename = "0030_JabLtzJl8bc.jpg";

        var schemas = await _fixture.Client.Schema.GenerateSchemasAsync(typeof(SampleLayer));
        await _fixture.Client.Schema.CreateManyAsync(schemas, enableForBinaryFiles: true);

        // Act
        var result = await _fixture.Client.Ingest.UploadAndImportFilesAsync(
            new[] { Path.Combine(_fixture.ExampleFilesBasePath, filename) },
            new FileImportRequest
            {
                LayerSchemaIds = schemas.Select(s => s.Id).ToList(),
                Metadata = new DataDictionary
                {
                    [schemas.Single().Id] = new SampleLayer { ImportedFrom = nameof(Should_import_files_with_metadata) }
                },
            });

        var detail = await result.FetchDetail();
        var content = detail.SucceededItems.Should().ContainSingle(c => c.RequestId == filename).Which.Item;

        content.LayerSchemaIds.Should().Contain(schemas.Single().Id);
    }

    [Fact]
    public async Task Should_import_twice_from_same_container()
    {
        // Arrange
        const string firstFileName = "0030_JabLtzJl8bc.jpg";
        const string secondFileName = "0518_4uCSqP5OKiI.jpg";

        var files = (await _fixture.Client.Ingest.UploadFilesAsync(
            new[] { firstFileName, secondFileName }.Select(f => Path.Combine(_fixture.ExampleFilesBasePath, f)))).ToDictionary(f => f.BlobName);

        // Act
        var firstResult = await _fixture.Client.Ingest.ImportFilesAsync(
            new Dictionary<IngestFile, FileImportWithFileNameOverrideRequest>
            {
                [files[firstFileName]] = new(),
            });

        var secondResult = await _fixture.Client.Ingest.ImportFilesAsync(
            new Dictionary<IngestFile, FileImportWithFileNameOverrideRequest>
            {
                [files[secondFileName]] = new(),
            });

        async Task AssertFileName(ContentBatchOperationWithRequestIdResult result, string filename)
        {
            var detail = await result.FetchDetail(new[] { ContentResolveBehavior.Content });
            var content = detail.SucceededItems.Should().ContainSingle(c => c.RequestId == filename).Which.Item;
            content.GetFileMetadata().FileName.Should().Be(filename);
        }

        // Assert
        await AssertFileName(firstResult, firstFileName);
        await AssertFileName(secondResult, secondFileName);
    }

    [Fact]
    public async Task Should_fail_to_import_same_file_twice()
    {
        // Arrange
        const string fileName = "0030_JabLtzJl8bc.jpg";

        var file = await _fixture.Client.Ingest.UploadFileAsync(Path.Combine(_fixture.ExampleFilesBasePath, fileName));
        var request = new Dictionary<IngestFile, FileImportWithFileNameOverrideRequest>
        {
            [file] = new(),
        };

        // Act
        await _fixture.Client.Ingest.ImportFilesAsync(request);
        var result = await _fixture.Client.Ingest.ImportFilesAsync(request);
        var detail = await result.FetchDetail();

        // Assert
        result.LifeCycle.Should().Be(BusinessProcessLifeCycle.Failed);
        var error = detail.FailedItems.Should().ContainSingle(i => i.RequestId == fileName).Which.Error;
        JsonConvert.DeserializeObject<PictureparkException>(error.Exception).Should().BeOfType<IngestFileAlreadyImportedException>();
    }

    [Fact]
    public async Task Should_upload_and_then_import()
    {
        // Arrange
        const string fileName = "0030_JabLtzJl8bc.jpg";
        var filePath = Path.Combine(_fixture.ExampleFilesBasePath, fileName);

        var items = Enumerable.Range(0, 3).Select(i => new IngestUploadItem($"file{i}.jpg", () => File.OpenRead(filePath)));

        // Act
        var files = await _fixture.Client.Ingest.UploadFilesAsync(items, new IngestUploadOptions { ConcurrentUploads = 2 });
        var result = await _fixture.Client.Ingest.ImportFilesAsync(
            files,
            new ImportOptions
            {
                CreateCollection = true,
                CollectionName = nameof(Should_upload_and_then_import),
                NotifyProgress = true
            });

        // Assert
        var detail = await result.FetchDetail();
        detail.SucceededItems.Should().HaveCount(3);
    }

    [Fact]
    public async Task Should_use_file_name_override()
    {
        // Arrange
        const string fileName = "0030_JabLtzJl8bc.jpg";

        var file = await _fixture.Client.Ingest.UploadFileAsync(Path.Combine(_fixture.ExampleFilesBasePath, fileName));
        var request = new Dictionary<IngestFile, FileImportWithFileNameOverrideRequest>
        {
            [file] = new()
            {
                FileNameOverride = "file.jpg"
            },
        };

        // Act
        var result = await _fixture.Client.Ingest.ImportFilesAsync(request);
        var detail = await result.FetchDetail(new[] { ContentResolveBehavior.Content });

        // Assert
        detail.SucceededItems.Should().ContainSingle().Which.Item.GetFileMetadata().FileName.Should().Be("file.jpg");
    }

    [Fact]
    public async Task Should_replace_content()
    {
        // Arrange
        var sourceContentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg -0030_JabLtzJl8bc", 20);
        var targetContentId = await _fixture.GetRandomContentIdAsync($"fileMetadata.fileExtension:.jpg -0030_JabLtzJl8bc -id:{sourceContentId}", 20);

        using var memoryStream = new MemoryStream();
        using (var fileResponse = await _fixture.Client.Content.DownloadAsync(sourceContentId, "Original"))
        {
            await fileResponse.Stream.CopyToAsync(memoryStream);
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        // Act
        var location = await _fixture.Client.Ingest.UploadFileAsync(new IngestUploadItem("file.jpg", () => memoryStream, leaveStreamOpen: true));
        var businessProcess = await _fixture.Client.Content.UpdateFileAsync(
            targetContentId,
            new ContentFileUpdateRequest { IngestFile = location });
        await _fixture.Client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id, TimeSpan.FromMinutes(2));

        // Assert
        var contentDetails = (await _fixture.Client.Content.GetManyAsync(new[] { sourceContentId, targetContentId }, new[] { ContentResolveBehavior.Content }))
            .ToDictionary(c => c.Id);

        var sourceFileHash = contentDetails[sourceContentId].GetFileMetadata().Sha1Hash;
        var targetFileHash = contentDetails[targetContentId].GetFileMetadata().Sha1Hash;

        targetFileHash.Should().Be(sourceFileHash);
    }

    [Fact]
    public async Task Should_import_urls()
    {
        // Arrange
        const string url = "https://upload.wikimedia.org/wikipedia/commons/1/19/Atlantis_Kennedy_Space_Center_Visitor_Complex_2.jpg";

        // Act
        var result = await _fixture.Client.Ingest.ImportFromUrlsAsync(
            new Dictionary<string, UrlImportRequest>
            {
                [url] = new()
                {
                    FileNameOverride = "file1.jpg"
                }
            },
            new ImportOptions
            {
                CreateCollection = true,
                CollectionName = nameof(Should_import_urls)
            });

        // Assert
        var detail = await result.FetchDetail();
        detail.SucceededItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task Should_check_file_for_content_schema_change()
    {
        // Arrange
        var namePrefix = $"{nameof(Should_check_file_for_content_schema_change).Replace("_", string.Empty)}{Guid.NewGuid():N}";
        var contentSchemaId = namePrefix + "Content";
        var layerId = namePrefix + "Layer";

        var schemas = await _fixture.Client.Schema.CreateManyAsync(
            new SchemaCreateManyRequest
            {
                Schemas = new List<SchemaCreateRequest>
                {
                    new()
                    {
                        Id = contentSchemaId,
                        Names = new TranslatedStringDictionary { ["en"] = contentSchemaId },
                        Descriptions = new TranslatedStringDictionary { ["en"] = contentSchemaId },
                        Types = new[] { SchemaType.Content },
                        ViewForAll = true
                    },
                    new()
                    {
                        Id = layerId,
                        Names = new TranslatedStringDictionary { ["en"] = layerId },
                        Descriptions = new TranslatedStringDictionary { ["en"] = layerId },
                        Types = new[] { SchemaType.Layer },
                        ReferencedInContentSchemaIds = new[]
                        {
                            contentSchemaId,
                            nameof(DocumentMetadata)
                        }
                    },
                }
            });

        var createdSchemas = await schemas.FetchDetail();
        createdSchemas.FailedItems.Should().BeEmpty();

        var content = await _fixture.Client.Content.CreateAsync(
            new ContentCreateRequest
            {
                ContentSchemaId = contentSchemaId,
                LayerSchemaIds = new[] { layerId }
            });

        content.LayerSchemaIds.Should().ContainSingle().Which.Should().Be(layerId);

        var uploadedImage = await UploadContent();

        // check up-front if the replacement can be done or would cause layer removal
        var replacementCheck = await _fixture.Client.Content.CheckUpdateFileAsync(
            content.Id,
            new ContentFileUpdateCheckRequest { IngestFile = uploadedImage });

        replacementCheck.Errors.Should().BeEmpty();
        var problematicChange = replacementCheck.ProblematicChanges.Should().ContainSingle().Which;
        problematicChange.IncompatibleLayerAssignments.Should().Contain(layerId, $"layer is not available for {nameof(ImageMetadata)}");

        // try to use a document instead
        var ex = await Record.ExceptionAsync(() => UploadAndReplaceContent(content.Id, "*.pdf"));
        var typeMismatchException = ex.Should().BeOfType<ContentFileReplaceTypeMismatchException>().Which;
        typeMismatchException.OriginalContentType.Should().Be(ContentType.Virtual);
        typeMismatchException.NewContentType.Should().Be(ContentType.InterchangeDocument);

        // we also need to opt-in to change ContentType
        await UploadAndReplaceContent(content.Id, "*.pdf", requestModifier: request => request.AllowContentTypeChange = true);
        content = await _fixture.Client.Content.GetAsync(content.Id);
        content.ContentType.Should().Be(ContentType.InterchangeDocument);

        // we can opt-in to let the system remove the layer
        await ReplaceContentFile(
            content.Id,
            new ContentFileUpdateRequest
            {
                IngestFile = uploadedImage,
                AllowContentTypeChange = true,
                AcceptableLayerUnassignments = problematicChange.IncompatibleLayerAssignments
            });
    }

    [Fact]
    public async Task Should_import_all_from_container_twice_and_fail_first_then_succeed()
    {
        // Arrange
        const string fileName = "0030_JabLtzJl8bc.jpg";

        var files = (await _fixture.Client.Ingest.UploadFilesAsync(
            new[] { fileName }.Select(f => Path.Combine(_fixture.ExampleFilesBasePath, f)))).ToDictionary(f => f.BlobName);

        // Act
        var firstResult = await _fixture.Client.Ingest.ImportFilesAsync(
            new Dictionary<IngestFile, FileImportWithFileNameOverrideRequest>
            {
                [files[fileName]] = new()
                {
                    ContentPermissionSetIds = new List<string> { $"{Guid.NewGuid():N}" }
                }
            });

        var secondResult = await _fixture.Client.Ingest.ImportFilesAsync(
            new Dictionary<IngestFile, FileImportWithFileNameOverrideRequest>
            {
                [files[fileName]] = new()
            });

        // Assert
        var firstDetail = await firstResult.FetchDetail();
        firstDetail.FailedItems.Should().ContainSingle();

        var secondDetail = await secondResult.FetchDetail();
        secondDetail.SucceededItems.Should().ContainSingle().Which.RequestId.Should().Be(fileName);
    }

    private async Task<IngestFile> UploadContent(string searchPattern = "*.jpg")
    {
        var filesInDirectory = Directory.GetFiles(_fixture.ExampleFilesBasePath, searchPattern).ToList();

        var file = filesInDirectory[Random.Shared.Next(filesInDirectory.Count - 1)];

        return (await _fixture.Client.Ingest.UploadFilesAsync(new[] { file })).Single();
    }

    private async Task UploadAndReplaceContent(string contentId, string searchPattern = "*.jpg", Action<ContentFileUpdateRequest> requestModifier = null)
    {
        var file = await UploadContent(searchPattern);

        var contentFileUpdateRequest = new ContentFileUpdateRequest { IngestFile = file };
        requestModifier?.Invoke(contentFileUpdateRequest);

        await ReplaceContentFile(contentId, contentFileUpdateRequest);
    }

    private async Task ReplaceContentFile(string contentId, ContentFileUpdateRequest updateRequest)
    {
        var businessProcess = await _fixture.Client.Content.UpdateFileAsync(contentId, updateRequest);
        var waitResult = await _fixture.Client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

        waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);
    }

    [PictureparkSchema(SchemaType.Layer)]
    private class SampleLayer
    {
        public string ImportedFrom { get; set; }
    }
}
