﻿using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Picturepark.SDK.V1.Tests.FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Picturepark.SDK.V1.AzureBlob;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class OutputFormatTests : IClassFixture<OutputFormatFixture>
    {
        private readonly OutputFormatFixture _fixture;
        private readonly IPictureparkService _client;

        public OutputFormatTests(OutputFormatFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldCreateOutputFormat()
        {
            // Arrange
            var outputFormat = await _fixture.CreateOutputFormat();

            // Act
            var result = await _client.OutputFormat.GetAsync(outputFormat.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(outputFormat.Id);

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldCreateMultipleOutputFormats()
        {
            // Arrange
            var outputFormat = await _fixture.CreateOutputFormats(3);
            var outputFormatIds = outputFormat.Select(s => s.Id).ToArray();

            // Act
            var result = await _client.OutputFormat.GetManyAsync(outputFormatIds);

            // Assert
            result.Should().NotBeNull();
            result.Select(of => of.Id).Should().BeEquivalentTo(outputFormatIds);

            result.ToList().ForEach(outputFormatDetail =>
                {
                    outputFormatDetail.Audit.CreatedByUser.Should().BeResolved();
                    outputFormatDetail.Audit.ModifiedByUser.Should().BeResolved();
                }
            );
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldUpdateOutputFormatCorrectly()
        {
            // Arrange
            var outputFormat = await _fixture.CreateOutputFormat();
            const int updatedJpegQuality = 42;

            var update = new OutputFormatEditable
            {
                Names = outputFormat.Names,
                Format = outputFormat.Format,
                RetentionTime = outputFormat.RetentionTime,
                SourceOutputFormats = outputFormat.SourceOutputFormats
            };
            update.Format.As<JpegFormat>().Quality = updatedJpegQuality;
            update.SourceOutputFormats.Audio = "Preview";

            // Act
            var response = await _client.OutputFormat.UpdateAsync(outputFormat.Id, update);

            var result = await response.FetchResult();

            // Assert
            result.Should().NotBeNull();
            result.Format.As<JpegFormat>().Quality.Should().Be(updatedJpegQuality);
            result.SourceOutputFormats.Audio.Should().Be("Preview");

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();

            var verifyOutputFormat = await _client.OutputFormat.GetAsync(outputFormat.Id);

            verifyOutputFormat.Format.As<JpegFormat>().Quality.Should().Be(updatedJpegQuality);
            verifyOutputFormat.SourceOutputFormats.Audio.Should().Be("Preview");
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldUpdateMultipleOutputFormatsCorrectly()
        {
            // Arrange
            var outputFormats = await _fixture.CreateOutputFormats(3);
            var outputFormatIds = outputFormats.Select(of => of.Id);

            var updateRequests = outputFormats.Select((of, i) =>
            {
                var request = new OutputFormatUpdateManyRequestItem
                {
                    Id = of.Id,
                    Names = of.Names,
                    Format = of.Format,
                    RetentionTime = of.RetentionTime,
                    SourceOutputFormats = of.SourceOutputFormats
                };
                request.Format.As<JpegFormat>().HorizontalResolution = (i + 1) * 100;
                request.SourceOutputFormats.Audio = "Preview";
                request.Format.As<JpegFormat>().Quality = i;
                request.DownloadFileNamePatterns = new TranslatedStringDictionary
                {
                    { _fixture.DefaultLanguage, "{{ fileNamePattern }} (updated)" }
                };

                return request;
            }).ToArray();

            // Act
            var response = await _client.OutputFormat.UpdateManyAsync(new OutputFormatUpdateManyRequest { Items = updateRequests });
            var detail = await response.FetchDetail();

            // Assert
            detail.Should().NotBeNull();
            detail.SucceededIds.Should().HaveSameCount(outputFormats);
            detail.FailedIds.Should().BeEmpty();

            var verifyOutputFormats = await _client.OutputFormat.GetManyAsync(outputFormatIds);

            for (var j = 0; j < outputFormats.Count; j++)
            {
                var verifyFormat = verifyOutputFormats.Should().ContainSingle(of => of.Id == outputFormats[j].Id).Subject;
                verifyFormat.Format.As<JpegFormat>().HorizontalResolution.Should().Be((j + 1) * 100);
                verifyFormat.Format.As<JpegFormat>().Quality.Should().Be(j);
                verifyFormat.SourceOutputFormats.Audio.Should().Be("Preview");
                verifyFormat.DownloadFileNamePatterns[_fixture.DefaultLanguage].Should().Be("{{ fileNamePattern }} (updated)");
            }
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldDeleteOutputFormat()
        {
            // Arrange
            var outputFormat = await _fixture.CreateOutputFormat();

            // Act
            await _client.OutputFormat.DeleteAsync(outputFormat.Id);

            // Assert
            var ex = await Record.ExceptionAsync(() => _client.OutputFormat.GetAsync(outputFormat.Id));
            ex.Should().BeOfType<OutputFormatNotFoundException>();
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldDeleteMultipleOutputFormats()
        {
            // Arrange
            var permissionSets = await _fixture.CreateOutputFormats(5);
            var toDeleteIds = permissionSets.Take(3).Select(s => s.Id).ToArray();
            var shouldStayIds = permissionSets.Skip(3).Select(s => s.Id).ToArray();

            // Act
            await _client.OutputFormat.DeleteManyAsync(
                new OutputFormatDeleteManyRequest
                {
                    Ids = toDeleteIds
                });

            // Assert
            var verifyDeletedOutputFormats = await _client.OutputFormat.GetManyAsync(toDeleteIds);
            verifyDeletedOutputFormats.Should().BeEmpty();

            var verifyOutputFormatsStayed = await _client.OutputFormat.GetManyAsync(shouldStayIds);
            verifyOutputFormatsStayed.Select(s => s.Id).Should().BeEquivalentTo(shouldStayIds);
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldCreateSingleOutputFormat()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString("N");

            // Act
            var outputFormat = new OutputFormat
            {
                Id = $"OF-test-{guid}",
                Names = new TranslatedStringDictionary { ["en"] = $"OF_test_{guid}" },
                Dynamic = true,
                Format = new JpegFormat
                {
                    Quality = 95
                },
                SourceOutputFormats = new SourceOutputFormats
                {
                    Image = "Original",
                    Video = "VideoPreview",
                    Document = "DocumentPreview",
                    Audio = "AudioPreview"
                }
            };
            var bpResult = await _client.OutputFormat.CreateAsync(outputFormat);

            var result = await bpResult.FetchResult();

            // Assert
            result.Should().NotBeNull();
            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldRenderDynamicOutputSingle()
        {
            // Arrange
            var format = await _fixture.CreateOutputFormat();
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);

            var fileName = new Random().Next(0, 999999) + "-" + contentId + ".jpg";
            var filePath = Path.Combine(_fixture.TempDirectory, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            // Act
            using (var response = await _client.Content.DownloadAsync(contentId, format.Id))
                await response.Stream.WriteToFileAsync(filePath);

            // Assert
            var fileInfo = new FileInfo(filePath);
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldRenderDynamicOutputMulti()
        {
            // Arrange
            var format = await _fixture.CreateOutputFormat();
            var contents = await _fixture.GetRandomContentsAsync("fileMetadata.fileExtension:.jpg", 10);

            var folderName = nameof(ShouldRenderDynamicOutputMulti) + new Random().Next(0, 999999);

            var folderAbsolutePathRendered = Path.Combine(_fixture.TempDirectory, folderName);
            Directory.CreateDirectory(folderAbsolutePathRendered);

            var errorDelegateCalled = false;

            // Act
            await _client.Content.DownloadFilesAsync(
                contents,
                folderAbsolutePathRendered,
                overwriteIfExists: false,
                outputFormat: format.Id,
                contentIdAsFilename: true,
                errorDelegate: _ => errorDelegateCalled = true);

            // Assert
            errorDelegateCalled.Should().BeFalse();

            var renderedDirectoryInfo = new DirectoryInfo(folderAbsolutePathRendered);
            renderedDirectoryInfo.EnumerateFiles().Count().Should().Be(contents.Results.Count);
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldAllowDownloadOfMultipleOutputFormatsForMultipleContents()
        {
            // Arrange
            var numberOfFormats = 3;

            var formats = await _fixture.CreateOutputFormats(numberOfFormats);
            var contents = await _fixture.GetRandomContentsAsync("fileMetadata.fileExtension:.jpg", 10);

            var folderName = new Random().Next(0, 999999) + "-" + "multi-download-dynamic";
            var folderPath = Path.Combine(_fixture.TempDirectory, folderName);
            var filePath = folderPath + ".zip";

            Directory.CreateDirectory(folderPath);
            if (File.Exists(filePath))
                File.Delete(filePath);

            var combinations =
                from content in contents.Results
                from format in formats
                select new ContentDownloadRequestItem()
                {
                    ContentId = content.Id,
                    OutputFormatId = format.Id
                };

            // Act
            var downloadLinkResponse = await _client.Content.CreateAndAwaitDownloadLinkAsync(new ContentDownloadLinkCreateRequest { Contents = combinations.ToList() });

            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync(downloadLinkResponse.DownloadUrl))
            {
                response.EnsureSuccessStatusCode();

                await using (var stream = await response.Content.ReadAsStreamAsync())
                await using (var fileStream = File.Create(filePath))
                    await stream.CopyToAsync(fileStream);
            }

            // Assert
            ZipFile.ExtractToDirectory(filePath, folderPath);

            var folderInfo = new DirectoryInfo(folderPath);
            var directories = folderInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).ToList();

            directories.Count.Should().Be(numberOfFormats);

            foreach (var directory in directories)
            {
                directory.Name.Should().BeOneOf(formats.Select(f => f.Id));

                var filesInOutputFormat = directory.EnumerateFiles().ToList();
                filesInOutputFormat.Should().HaveCount(contents.Results.Count);
                filesInOutputFormat.Should().OnlyContain(f => f.Length > 0);
                filesInOutputFormat.Should().OnlyContain(f => Path.GetExtension(f.Name) == ".jpg");
            }
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task Should_change_filename_patterns_single()
        {
            var format = await _fixture.CreateOutputFormat();
            var pattern = "Custom-Format {{ fileNamePattern }}";

            var bp = await _client.OutputFormat.SetDownloadFileNamePatternsAsync(
                format.Id,
                new Dictionary<string, string>
                {
                    { _fixture.DefaultLanguage, pattern }
                });

            await _client.BusinessProcess.WaitForCompletionAsync(bp.Id);

            var formatRetrieved = await _client.OutputFormat.GetAsync(format.Id);
            formatRetrieved.DownloadFileNamePatterns[_fixture.DefaultLanguage].Should().Be(pattern);
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task Should_change_filename_patterns()
        {
            var formats = await _fixture.CreateOutputFormats(2);
            var pattern = "Custom-Format {{ fileNamePattern }}";

            var bp = await _client.OutputFormat.SetDownloadFileNamePatternsManyAsync(
                new OutputFormatDownloadFileNamePatternUpdateManyRequest
                {
                    Items = formats.Select(
                        f => new OutputFormatDownloadFileNamePatternUpdateRequestItem
                        {
                            Id = f.Id,
                            Patterns = new TranslatedStringDictionary { { _fixture.DefaultLanguage, pattern } }
                        }).ToArray()
                });

            await _client.BusinessProcess.WaitForCompletionAsync(bp.Id);

            var formatsRetrieved = await _client.OutputFormat.GetManyAsync(formats.Select(f => f.Id));
            formatsRetrieved.Should().OnlyContain(f => f.DownloadFileNamePatterns[_fixture.DefaultLanguage] == pattern);
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldAllowPreviewOfFormat()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            var format = new OutputFormatRenderingSpecification
            {
                SourceOutputFormats = new SourceOutputFormats
                {
                    Image = "Original"
                },
                Format = new JpegFormat
                {
                    Quality = 75,
                    ResizeAction = new ResizeAction
                    {
                        Height = 133,
                        Width = 337
                    }
                }
            };
            var previewRequest = new OutputFormatRenderPreviewRequest
            {
                ContentId = contentId,
                OutputFormat = format
            };

            var fileName = nameof(ShouldAllowPreviewOfFormat) + new Random().Next(0, 999999) + "-" + contentId + ".jpg";
            var filePath = Path.Combine(_fixture.TempDirectory, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            // Act
            using (var response = await _client.OutputFormat.RenderFormatPreviewAsync(previewRequest))
                await response.Stream.WriteToFileAsync(filePath);

            // Assert
            var fileInfo = new FileInfo(filePath);
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldCreateCopyFormatAndCopySourceFile()
        {
            // Arrange
            var uploadResult = await _client.Ingest.UploadAndImportFilesAsync(
                new[] { Path.Combine(_fixture.ExampleFilesBasePath, "sample006.pdf") });

            var details = await uploadResult.FetchDetail(new[] { ContentResolveBehavior.Content });
            var content = details.SucceededItems.Single().Item;

            var formatId = $"{nameof(ShouldCreateCopyFormatAndCopySourceFile)}{Guid.NewGuid():N}";
            var format = new OutputFormat
            {
                Id = formatId,
                Names = new TranslatedStringDictionary { ["en"] = formatId },
                Dynamic = true,
                SourceOutputFormats = new SourceOutputFormats
                {
                    Document = "Original" // CopyFormat is allowed for all regular files, not only for Images
                },
                Format = new CopyFormat()
            };

            await _fixture.Client.OutputFormat.CreateAsync(format);

            var outputsForContent = await _fixture.Client.Content.GetOutputsAsync(content.Id);
            var outputForFormat = outputsForContent.Should().ContainSingle(o => o.OutputFormatId == formatId).Which;

            outputForFormat.DynamicRendering.Should().BeTrue("it is a dynamic format");
            outputForFormat.FileSize.Should().Be(content.GetFileMetadata().FileSizeInBytes);

            var cpVersion = Version.Parse((await _client.Info.GetVersionAsync()).FileVersion);

            if (cpVersion >= new Version(11, 12))
                outputForFormat.RenderingStateV2.Should().Be(OutputRenderingStateV2.Renderable, "newly created dynamic format, rendered lazily");

#pragma warning disable CS0618 // Type or member is obsolete
            outputForFormat.RenderingState.Should().Be(OutputRenderingState.Skipped, "newly created dynamic format, rendered lazily");
#pragma warning restore CS0618 // Type or member is obsolete

            var fileName = nameof(ShouldCreateCopyFormatAndCopySourceFile) + new Random().Next(0, 999999) + "-" + content.Id + ".pdf";
            var filePath = Path.Combine(_fixture.TempDirectory, fileName);
            File.Delete(filePath);

            // Act
            using (var response = await _client.Content.DownloadAsync(content.Id, formatId))
                await response.Stream.WriteToFileAsync(filePath);

            // Assert
            var fileInfo = new FileInfo(filePath);
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().Be(outputForFormat.FileSize);
        }
    }
}
