using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Helpers;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class TransferTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public TransferTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldGetBlacklist()
        {
            // Act
            var blacklist = await _client.Transfer.GetBlacklistAsync();

            // Assert
            Assert.NotNull(blacklist?.Items);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldCreateTransferFromFiles()
        {
            // Arrange
            var transferName = new Random().Next(1000, 9999).ToString();
            var files = new FileLocations[]
            {
                Path.Combine(_fixture.ExampleFilesBasePath, "0030_JabLtzJl8bc.jpg")
            };

            // Act
            var result = await _client.Transfer.CreateAndWaitForCompletionAsync(transferName, files);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TransferState.Draft, result.Transfer.State);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldDeleteFiles()
        {
            // Arrange
            var (createTransferResult, fileId) = await CreateFileTransferAsync();

            // Act
            var request = new FileTransferDeleteRequest
            {
                TransferId = createTransferResult.Transfer.Id,
                FileTransferIds = new List<string> { fileId }
            };

            await _client.Transfer.GetFileAsync(fileId);

            await _client.Transfer.DeleteFilesAsync(request);

            // Assert
            await Assert.ThrowsAsync<FileTransferNotFoundException>(async () =>
            {
                await _client.Transfer.GetFileAsync(fileId);
            });
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldCancelTransfer()
        {
            // Arrange
            var transferName = new Random().Next(1000, 9999).ToString();
            var files = new FileLocations[]
            {
                Path.Combine(_fixture.ExampleFilesBasePath, "0030_JabLtzJl8bc.jpg")
            };

            var result = await _client.Transfer.CreateAndWaitForCompletionAsync(transferName, files);

            // Act
            await _client.Transfer.CancelAsync(result.Transfer.Id);

            // Assert
            var currentTransfer = await _client.Transfer.GetAsync(result.Transfer.Id);

            new[] { TransferState.TransferReady, TransferState.UploadCancellationInProgress, TransferState.UploadCancelled }.Should()
                .Contain(currentTransfer.State);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldWaitForCompletionAndStateShouldBeTransferReady()
        {
            // Arrange
            var transferName = new Random().Next(1000, 9999).ToString();
            var files = new FileLocations[]
            {
                Path.Combine(_fixture.ExampleFilesBasePath, "0030_JabLtzJl8bc.jpg")
            };

            var result = await _client.Transfer.UploadFilesAsync(transferName, files, new UploadOptions());

            // Act
            var waitResult = await _client.BusinessProcess.WaitForCompletionAsync(result.Transfer.BusinessProcessId);

            // Assert
            waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);

            var transfer = await _client.Transfer.GetAsync(result.Transfer.Id);
            transfer.State.Should().Be(TransferState.TransferReady);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldCreateTransferFromWebUrls()
        {
            // Arrange
            var transferName = "UrlImport " + new Random().Next(1000, 9999);
            var urls = new List<string>
            {
                "https://picturepark.com/wp-content/uploads/2013/06/home-marquee.jpg",
                "http://cdn1.spiegel.de/images/image-733178-900_breitwand_180x67-zgpe-733178.jpg",
                "http://cdn3.spiegel.de/images/image-1046236-700_poster_16x9-ymle-1046236.jpg"
            };

            // Act
            var request = new CreateTransferRequest
            {
                Name = transferName,
                TransferType = TransferType.WebDownload,
                WebLinks = urls.Select(url => new TransferWebLink
                {
                    Url = url,
                    RequestId = Guid.NewGuid().ToString()
                }).ToList()
            };

            var result = await _client.Transfer.CreateAndWaitForCompletionAsync(request);

            // Assert
            Assert.NotNull(result.Transfer);
        }

#pragma warning disable 618
        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldSupportRequestIdAndLegacyIdentifier()
        {
            // Arrange
            var transferName = "UrlImportRequestIdAndLegacyIdentifier " + new Random().Next(1000, 9999);
            var urlsAndIds = new List<(string url, string id, bool legacy)>
            {
                ("https://picturepark.com/wp-content/uploads/2013/06/home-marquee.jpg", "marquee", true),
                ("http://cdn1.spiegel.de/images/image-733178-900_breitwand_180x67-zgpe-733178.jpg", "breitwand", false)
            };

            // Act
            var request = new CreateTransferRequest
            {
                Name = transferName,
                TransferType = TransferType.WebDownload,
                WebLinks = urlsAndIds.Select(urlAndId =>
                {
                    var webLink = new TransferWebLink() { Url = urlAndId.url };

                    if (urlAndId.legacy)
                        webLink.Identifier = urlAndId.id;
                    else
                        webLink.RequestId = urlAndId.id;

                    return webLink;
                }).ToList()
            };

            var createTransferResult = await _client.Transfer.CreateAndWaitForCompletionAsync(request);
            var fileTransfers = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id);

            // Assert
            fileTransfers.Should().HaveCount(2);
            fileTransfers.Should().OnlyContain(fileTransfer => urlsAndIds.Any(urlAndId =>
                fileTransfer.RequestId == fileTransfer.Identifier && fileTransfer.RequestId == urlAndId.id));
        }
#pragma warning restore 618

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldDelete()
        {
            // Arrange
            var urls = new List<string>
            {
                "https://picturepark.com/wp-content/uploads/2013/06/home-marquee.jpg",
                "http://cdn1.spiegel.de/images/image-733178-900_breitwand_180x67-zgpe-733178.jpg",
                "http://cdn3.spiegel.de/images/image-1046236-700_poster_16x9-ymle-1046236.jpg"
            };
            var result = await CreateWebTransferAsync(urls);

            // Act
            var transferId = result.Transfer.Id;
            var transfer = await _client.Transfer.GetAsync(result.Transfer.Id);

            await _client.Transfer.DeleteAsync(transferId);

            // Assert
            Assert.NotNull(transfer);
            await Assert.ThrowsAsync<TransferNotFoundException>(async () => await _client.Transfer.GetAsync(transferId));
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldGet()
        {
            // Arrange
            var urls = new List<string>
            {
                "https://picturepark.com/wp-content/uploads/2013/06/home-marquee.jpg",
                "http://cdn1.spiegel.de/images/image-733178-900_breitwand_180x67-zgpe-733178.jpg",
                "http://cdn3.spiegel.de/images/image-1046236-700_poster_16x9-ymle-1046236.jpg"
            };
            var result = await CreateWebTransferAsync(urls);

            // Act
            TransferDetail transfer = await _client.Transfer.GetAsync(result.Transfer.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Transfer.Id, transfer.Id);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldGetFile()
        {
            // Arrange
            var (createdTransfer, fileId) = await CreateFileTransferAsync();

            // Act
            var transfer = await _client.Transfer.GetFileAsync(fileId);

            // Assert
            transfer.Should().NotBeNull();
            transfer.TransferId.Should().Be(createdTransfer.Transfer.Id);
            transfer.Id.Should().Be(fileId);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldSearch()
        {
            // Arrange
            var request = new TransferSearchRequest { Limit = 1, SearchString = "*" };

            // Act
            TransferSearchResult result = await _client.Transfer.SearchAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Results);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldSearchFiles()
        {
            // Arrange
            var request = new FileTransferSearchRequest { Limit = 20, SearchString = "*" };

            // Act
            FileTransferSearchResult result = await _client.Transfer.SearchFilesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Count >= 1);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldPartialImport()
        {
            // Arrange
            const int desiredUploadFiles = 4;

            var transferName = nameof(ShouldUploadAndImportFiles) + "-" + new Random().Next(1000, 9999);
            var filesInDirectory = Directory.GetFiles(_fixture.ExampleFilesBasePath, "*").ToList();

            var numberOfFilesInDirectory = filesInDirectory.Count;
            var numberOfUploadFiles = Math.Min(desiredUploadFiles, numberOfFilesInDirectory);

            var randomNumber = new Random().Next(1, numberOfFilesInDirectory - numberOfUploadFiles);
            var importFilePaths = filesInDirectory
                .Skip(randomNumber)
                .Take(numberOfUploadFiles)
                .Select(path => new FileLocations(path)).ToList();

            // Act
            var uploadOptions = new UploadOptions
            {
                ConcurrentUploads = 4,
                SuccessDelegate = Console.WriteLine,
                ErrorDelegate = args => Console.WriteLine(args.Exception)
            };

            var createTransferResult = await _client.Transfer.UploadFilesAsync(transferName, importFilePaths, uploadOptions);
            var uploadFiles = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id, numberOfUploadFiles);

            var transfer = await _client.Transfer.PartialImportAsync(createTransferResult.Transfer.Id, new ImportTransferPartialRequest
            {
                Items = new List<FileTransferCreateItem>
                {
                    new FileTransferCreateItem
                    {
                        FileId = uploadFiles.First().Id
                    }
                }
            });

            await _client.BusinessProcess.WaitForCompletionAsync(transfer.BusinessProcessId);

            // Assert
            var result = await _client.Transfer.SearchFilesByTransferIdAsync(transfer.Id, numberOfUploadFiles);
            result.Should().ContainSingle(x => x.State == FileTransferState.ImportCompleted);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldPartialImportWithMetadata()
        {
            // Arrange
            var fileTransfers = new List<FileTransferCreateItem>();
            var timeout = TimeSpan.FromMinutes(2);

            await SetupSchema(typeof(PersonShot));
            string personId = await CreatePerson();

            var urls = new[]
            {
                "https://en.wikipedia.org/static/images/project-logos/enwiki-1.5x.png"
            };

            var createTransferResult = await CreateWebTransferAsync(urls);
            var files = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id, 1);

            foreach (FileTransfer file in files)
            {
                var personTag = new Person
                {
                    RefId = personId
                };
                var metadata = Metadata.From(new PersonShot { Persons = new[] { personTag } });
                fileTransfers.Add(new FileTransferCreateItem
                {
                    FileId = file.Id,
                    LayerSchemaIds = new[] { nameof(PersonShot) },
                    Metadata = metadata
                });
            }

            var partialRequest = new ImportTransferPartialRequest()
            {
                Items = fileTransfers,
            };

            // Act
            var importResult = await _client.Transfer.PartialImportAsync(createTransferResult.Transfer.Id, partialRequest);
            var waitResult = await _client.BusinessProcess.WaitForCompletionAsync(importResult.BusinessProcessId, timeout);

            // Assert
            waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);

            var result = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id);
            var contentIds = result.Select(r => r.ContentId);
            contentIds.Should().HaveCount(1);

            var createdContent = await _client.Content.GetAsync(contentIds.First(), new[] { ContentResolveBehavior.Metadata });
            var jMetadata = JObject.FromObject(createdContent.Metadata);
            jMetadata[nameof(PersonShot).ToLowerCamelCase()]["persons"][0]["_refId"].Value<string>().Should().Be(personId);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldUploadAndImportFiles()
        {
            // Arrange
            const int desiredUploadFiles = 10;
            var timeout = TimeSpan.FromMinutes(2);

            var transferName = nameof(ShouldUploadAndImportFiles) + "-" + new Random().Next(1000, 9999);

            var filesInDirectory = Directory.GetFiles(_fixture.ExampleFilesBasePath, "*").ToList();

            var numberOfFilesInDirectory = filesInDirectory.Count;
            var numberOfUploadFiles = Math.Min(desiredUploadFiles, numberOfFilesInDirectory);

            var randomNumber = new Random().Next(0, numberOfFilesInDirectory - numberOfUploadFiles);
            var importFilePaths = filesInDirectory
                .Skip(randomNumber)
                .Take(numberOfUploadFiles)
                .Select(fn => new FileLocations(fn, $"{Path.GetFileNameWithoutExtension(fn)}_1{Path.GetExtension(fn)}"))
                .ToList();

            // Act
            var uploadOptions = new UploadOptions
            {
                ConcurrentUploads = 4,
                SuccessDelegate = Console.WriteLine,
                ErrorDelegate = args => Console.WriteLine(args.Exception)
            };
            var createTransferResult = await _client.Transfer.UploadFilesAsync(transferName, importFilePaths, uploadOptions);

            var importRequest = new ImportTransferRequest
            {
                ContentPermissionSetIds = new List<string>(),
                Metadata = null,
                LayerSchemaIds = new List<string>()
            };

            await _client.Transfer.ImportAndWaitForCompletionAsync(createTransferResult.Transfer, importRequest, timeout);

            // Assert
            var result = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id);
            var contentIds = result.Select(r => r.ContentId);

            Assert.Equal(importFilePaths.Count(), contentIds.Count());
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldUploadSameFileTwiceInSameTransfer()
        {
            // Arrange
            var transferName = Guid.NewGuid().ToString();
            var file = Path.GetTempFileName();

            File.WriteAllText(file, "foobar");

            var files = Enumerable.Range(0, 2).Select(x => new FileLocations(file));

            // Act
            var result = await _client.Transfer.UploadFilesAsync(
                transferName,
                files,
                new UploadOptions()
                {
                    WaitForTransferCompletion = true,
                    ErrorDelegate = args => Console.WriteLine(args.Exception),
                    SuccessDelegate = Console.WriteLine
                });

            // Assert
            result.FileUploads.Should().HaveCount(2);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldReportChunkSizeRangeErrorQuickly()
        {
            var file = Path.GetTempFileName();
            File.WriteAllBytes(file, Enumerable.Repeat((byte)42, 1024 * 1024).ToArray()); // 1MiB

            var weTriggeredCancellation = false;

            var transfer = await _client.Transfer.CreateAsync(
                new CreateTransferRequest
                {
                    TransferType = TransferType.FileUpload,
                    Name = nameof(ShouldReportChunkSizeRangeErrorQuickly),
                    Files = new List<TransferUploadFile>
                    {
                        new TransferUploadFile { RequestId = "file", FileName = Path.GetFileName(file) }
                    }
                });

            await _client.BusinessProcess.WaitForStatesAsync(transfer.BusinessProcessId, new[] { TransferState.Created.ToString() }, TimeSpan.FromMinutes(2))
                ;

            await Assert.ThrowsAnyAsync<ChunkSizeOutOfRangeException>(
                async () =>
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)))
                    using (cts.Token.Register(() => weTriggeredCancellation = true))
                    {
                        using (var fs = File.OpenRead(file))
                        {
                            var buffer = new byte[1024];
                            await fs.ReadAsync(buffer, 0, 1024, cts.Token);

                            using (var ms = new MemoryStream(buffer))
                                await _client.Transfer.UploadFileAsync(1, 1024, 1024 * 1024, 1024, transfer.Id, "file", ms, cts.Token);
                        }
                    }
                });

            File.Delete(file);

            weTriggeredCancellation.Should().BeFalse();
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldUploadAndReportLastProgressTimeStamp()
        {
            // Arrange
            var transferName = Guid.NewGuid().ToString();
            var file = Path.GetTempFileName();

            File.WriteAllText(file, "foobar");

            var files = new[] { new FileLocations(file) };

            // Act
            var result = await _client.Transfer.UploadFilesAsync(
                transferName,
                files,
                new UploadOptions()
                {
                    WaitForTransferCompletion = false,
                    ErrorDelegate = args => Console.WriteLine(args.Exception),
                    SuccessDelegate = Console.WriteLine
                });

            // Assert
            var businessProcess = await _client.BusinessProcess.WaitForCompletionAsync(result.Transfer.BusinessProcessId);
            businessProcess.BusinessProcess.LastReportedProgress.Should().NotBeNull();
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldCallErrorDelegateForBlacklistedFile()
        {
            // Arrange
            var blacklist = await _fixture.Client.Transfer.GetBlacklistAsync();
            var filename = blacklist.Items.First().Match;
            var called = false;

            var options = new UploadOptions
            {
                ErrorDelegate = args => called = true
            };

            // Act
            await _fixture.Client.Transfer.UploadFilesAsync(
                $"{Guid.NewGuid():N}",
                new[] { new FileLocations(filename) },
                options);

            // Assert
            called.Should().BeTrue();
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldReturnEmptyResultIfAllFilesBlacklisted()
        {
            // Arrange
            var blacklist = await _fixture.Client.Transfer.GetBlacklistAsync();
            var filename = blacklist.Items.First().Match;

            // Act
            var result = await _fixture.Client.Transfer.UploadFilesAsync(
                $"{Guid.NewGuid():N}",
                new[] { new FileLocations(filename) },
                null);

            // Assert
            result.Transfer.Should().BeNull();
            result.FileUploads.Should().BeEmpty();
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldCallErrorDelegateWhenFileDoesNotExist()
        {
            // Arrange
            var filename = $"{Guid.NewGuid():N}";

            (FileLocations file, Exception ex) errorDelegateArgs = (null, null);

            var options = new UploadOptions
            {
                ErrorDelegate = args => errorDelegateArgs = args
            };

            // Act
            await _fixture.Client.Transfer.UploadFilesAsync(
                $"{Guid.NewGuid():N}",
                new[]
                {
                    new FileLocations(filename),
                },
                options);

            // Assert
            errorDelegateArgs.file.AbsoluteSourcePath.Should().Be(filename);
            errorDelegateArgs.ex.Should().NotBeNull();
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldUploadEmptyFiles()
        {
            // Arrange
            var uploadOptions = new UploadOptions { WaitForTransferCompletion = true };
            var filename = Path.GetTempFileName();

            try
            {
                // Act
                var createTransferResult = await _fixture.Client.Transfer.UploadFilesAsync(
                    nameof(ShouldUploadEmptyFiles) + "-" + Guid.NewGuid().ToString("N"),
                    new[] { new FileLocations(filename) },
                    uploadOptions,
                    TimeSpan.FromMinutes(2));

                // Assert
                var transferDetail = await _client.Transfer.GetAsync(createTransferResult.Transfer.Id);
                transferDetail.State.Should().Be(TransferState.TransferReady);

                var fileTransfers = await _client.Transfer.SearchFilesByTransferIdAsync(transferDetail.Id);
                var fileTransferId = fileTransfers.Single().Id;
                var fileTransferDetail = await _client.Transfer.GetFileAsync(fileTransferId);

                var fileMetadata = fileTransferDetail.FileMetadata.Should().BeOfType<FileMetadata>().Which;

                fileMetadata.FileSizeInBytes.Should().Be(0);
                fileMetadata.Sha1Hash.Should().Be("DA39A3EE5E6B4B0D3255BFEF95601890AFD80709");
            }
            finally
            {
                File.Delete(filename);
            }
        }

        private Task<(CreateTransferResult, string fileId)> CreateFileTransferAsync() =>
           TransferHelper.CreateSingleFileTransferAsync(_client, Path.Combine(_fixture.ExampleFilesBasePath, "0030_JabLtzJl8bc.jpg"));

        private async Task<CreateTransferResult> CreateWebTransferAsync(IReadOnlyList<string> urls)
        {
            return await CreateWebTransferAsync(urls.Select(url => (url, (string)null)).ToArray());
        }

        private async Task<CreateTransferResult> CreateWebTransferAsync(IReadOnlyList<(string url, string targetFileName)> urls)
        {
            var transferName = "UrlImport " + new Random().Next(1000, 9999);

            var request = new CreateTransferRequest
            {
                Name = transferName,
                TransferType = TransferType.WebDownload,
                WebLinks = urls.Select(item => new TransferWebLink
                {
                    Url = item.url,
                    FileName = item.targetFileName,
                    RequestId = Guid.NewGuid().ToString()
                }).ToList()
            };

            return await _client.Transfer.CreateAndWaitForCompletionAsync(request);
        }

        private async Task SetupSchema(Type type)
        {
            var schemas = await _client.Schema.GenerateSchemasAsync(type);
            var schemasToCreate = new List<SchemaDetail>();

            foreach (var schema in schemas)
            {
                if (!await _client.Schema.ExistsAsync(schema.Id))
                {
                    schemasToCreate.Add(schema);
                }
            }

            await _client.Schema.CreateManyAsync(schemasToCreate, true, TimeSpan.FromMinutes(1));
        }

        private async Task<string> CreatePerson()
        {
            var operationResult = await _client.ListItem.CreateFromObjectAsync(new Person
            {
                Firstname = "FirstName",
                LastName = "LastName",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                EmailAddress = "first.last@testyyy.com"
            });
            return (await operationResult.FetchDetail()).SucceededIds.First();
        }

        private async Task<List<string>> GetContentsDownloadUrls(ContentSearchResult contentSearchResult)
        {
            var urls = new List<string>();
            foreach (var content in contentSearchResult.Results)
            {
                var downloadRequest = new ContentDownloadLinkCreateRequest()
                {
                    Contents = new List<ContentDownloadRequestItem>
                    {
                        new ContentDownloadRequestItem
                        {
                            ContentId = content.Id,
                            OutputFormatId = "Original",
                        }
                    }
                };
                var downloadResult = await _client.Content.CreateAndAwaitDownloadLinkAsync(downloadRequest);
                urls.Add(downloadResult.DownloadUrl);
            }

            return urls;
        }
    }
}
