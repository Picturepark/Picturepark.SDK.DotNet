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
            var blacklist = await _client.Transfer.GetBlacklistAsync().ConfigureAwait(false);

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
            var result = await _client.Transfer.CreateAndWaitForCompletionAsync(transferName, files).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TransferState.Draft, result.Transfer.State);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldDeleteFiles()
        {
            // Arrange
            var (createTransferResult, fileId) = await CreateFileTransferAsync().ConfigureAwait(false);

            // Act
            var request = new FileTransferDeleteRequest
            {
                TransferId = createTransferResult.Transfer.Id,
                FileTransferIds = new List<string> { fileId }
            };

            await _client.Transfer.GetFileAsync(fileId).ConfigureAwait(false);

            await _client.Transfer.DeleteFilesAsync(request).ConfigureAwait(false);

            // Assert
            await Assert.ThrowsAsync<FileTransferNotFoundException>(async () =>
            {
                await _client.Transfer.GetFileAsync(fileId).ConfigureAwait(false);
            }).ConfigureAwait(false);
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

            var result = await _client.Transfer.CreateAndWaitForCompletionAsync(transferName, files).ConfigureAwait(false);

            // Act
            await _client.Transfer.CancelTransferAsync(result.Transfer.Id).ConfigureAwait(false);

            // Assert
            var currentTransfer = await _client.Transfer.GetAsync(result.Transfer.Id).ConfigureAwait(false);

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

            var result = await _client.Transfer.UploadFilesAsync(transferName, files, new UploadOptions()).ConfigureAwait(false);

            // Act
            var waitResult = await _client.BusinessProcess.WaitForCompletionAsync(result.Transfer.BusinessProcessId).ConfigureAwait(false);

            // Assert
            waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);

            var transfer = await _client.Transfer.GetAsync(result.Transfer.Id).ConfigureAwait(false);
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

            var result = await _client.Transfer.CreateAndWaitForCompletionAsync(request).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result.Transfer);
        }

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

            var createTransferResult = await _client.Transfer.CreateAndWaitForCompletionAsync(request).ConfigureAwait(false);
            var fileTransfers = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id).ConfigureAwait(false);

            // Assert
            fileTransfers.Should().HaveCount(2);
            fileTransfers.Should().OnlyContain(fileTransfer => urlsAndIds.Any(urlAndId =>
                fileTransfer.RequestId == fileTransfer.Identifier && fileTransfer.RequestId == urlAndId.id));
        }

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
            var result = await CreateWebTransferAsync(urls).ConfigureAwait(false);

            // Act
            var transferId = result.Transfer.Id;
            var transfer = await _client.Transfer.GetAsync(result.Transfer.Id).ConfigureAwait(false);

            await _client.Transfer.DeleteAsync(transferId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(transfer);
            await Assert.ThrowsAsync<TransferNotFoundException>(async () => await _client.Transfer.GetAsync(transferId).ConfigureAwait(false));
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
            var result = await CreateWebTransferAsync(urls).ConfigureAwait(false);

            // Act
            TransferDetail transfer = await _client.Transfer.GetAsync(result.Transfer.Id).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Transfer.Id, transfer.Id);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldGetFile()
        {
            // Arrange
            var (createdTransfer, fileId) = await CreateFileTransferAsync().ConfigureAwait(false);

            // Act
            var transfer = await _client.Transfer.GetFileAsync(fileId).ConfigureAwait(false);

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
            TransferSearchResult result = await _client.Transfer.SearchAsync(request).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Results.Count);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldSearchFiles()
        {
            // Arrange
            var request = new FileTransferSearchRequest { Limit = 20, SearchString = "*" };

            // Act
            FileTransferSearchResult result = await _client.Transfer.SearchFilesAsync(request).ConfigureAwait(false);

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
                ChunkSize = 1024 * 1024,
                SuccessDelegate = Console.WriteLine,
                ErrorDelegate = args => Console.WriteLine(args.Exception)
            };

            var createTransferResult = await _client.Transfer.UploadFilesAsync(transferName, importFilePaths, uploadOptions).ConfigureAwait(false);
            var uploadFiles = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id, numberOfUploadFiles).ConfigureAwait(false);

            var transfer = await _client.Transfer.PartialImportAsync(createTransferResult.Transfer.Id, new ImportTransferPartialRequest
            {
                Items = new List<FileTransferCreateItem>
                {
                    new FileTransferCreateItem
                    {
                        FileId = uploadFiles.First().Id
                    }
                }
            }).ConfigureAwait(false);

            await _client.BusinessProcess.WaitForCompletionAsync(transfer.BusinessProcessId).ConfigureAwait(false);

            // Assert
            var result = await _client.Transfer.SearchFilesByTransferIdAsync(transfer.Id, numberOfUploadFiles).ConfigureAwait(false);
            result.Should().ContainSingle(x => x.State == FileTransferState.ImportCompleted);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldPartialImportWithMetadata()
        {
            // Arrange
            var fileTransfers = new List<FileTransferCreateItem>();
            var timeout = TimeSpan.FromMinutes(2);

            await SetupSchema(typeof(PersonShot)).ConfigureAwait(false);
            string personId = await CreatePerson().ConfigureAwait(false);

            var contentSearchResult = await RandomHelper.GetRandomContentsAsync(_client, string.Empty, 1, new[] { ContentType.Bitmap }).ConfigureAwait(false);
            var urls = await GetContentsDownloadUrls(contentSearchResult).ConfigureAwait(false);

            var createTransferResult = await CreateWebTransferAsync(urls).ConfigureAwait(false);
            var files = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id, 1).ConfigureAwait(false);

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
            var importResult = await _client.Transfer.PartialImportAsync(createTransferResult.Transfer.Id, partialRequest).ConfigureAwait(false);
            var waitResult = await _client.BusinessProcess.WaitForCompletionAsync(importResult.BusinessProcessId, timeout).ConfigureAwait(false);

            // Assert
            waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);

            var result = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id).ConfigureAwait(false);
            var contentIds = result.Select(r => r.ContentId);
            contentIds.Should().HaveCount(1);

            var createdContent = await _client.Content.GetAsync(contentIds.First(), new[] { ContentResolveBehavior.Metadata }).ConfigureAwait(false);
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
                ChunkSize = 1024 * 1024,
                SuccessDelegate = Console.WriteLine,
                ErrorDelegate = args => Console.WriteLine(args.Exception)
            };
            var createTransferResult = await _client.Transfer.UploadFilesAsync(transferName, importFilePaths, uploadOptions).ConfigureAwait(false);

            var importRequest = new ImportTransferRequest
            {
                ContentPermissionSetIds = new List<string>(),
                Metadata = null,
                LayerSchemaIds = new List<string>()
            };

            await _client.Transfer.ImportAndWaitForCompletionAsync(createTransferResult.Transfer, importRequest, timeout).ConfigureAwait(false);

            // Assert
            var result = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id).ConfigureAwait(false);
            var contentIds = result.Select(r => r.ContentId);

            Assert.Equal(importFilePaths.Count(), contentIds.Count());
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldRespectTimeoutWhileUploading()
        {
            var transferName = Guid.NewGuid().ToString();

            var ex = await Assert.ThrowsAnyAsync<Exception>(
                async () =>
                    await _client.Transfer.UploadFilesAsync(
                        transferName,
                        new FileLocations[0],
                        new UploadOptions { WaitForTransferCompletion = true },
                        TimeSpan.FromMilliseconds(1)).ConfigureAwait(false)).ConfigureAwait(false);

            Assert.Contains(ex.GetType(), new[] { typeof(BusinessProcessLifeCycleNotHitException), typeof(BusinessProcessStateNotHitException) });
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
                }).ConfigureAwait(false);

            // Assert
            result.FileUploads.Should().HaveCount(2);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldReportChunkSizeRangeErrorQuickly()
        {
            var file = Path.GetTempFileName();
            File.WriteAllBytes(file, Enumerable.Repeat((byte)42, 1024 * 1024).ToArray()); // 1MiB

            var files = Enumerable.Range(0, 20).Select(x => new FileLocations(file)).ToList();

            var weTriggeredCancellation = false;

            var exception = await Assert.ThrowsAnyAsync<AggregateException>(
                async () =>
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)))
                    using (cts.Token.Register(() => weTriggeredCancellation = true))
                    {
                        await _client.Transfer.UploadFilesAsync(
                            nameof(ShouldReportChunkSizeRangeErrorQuickly),
                            files,
                            new UploadOptions
                            {
                                ChunkSize = 1024, // this is out of range, needs to be between 1 and 100 MiB and given in Bytes
                                ConcurrentUploads = 10,
                                WaitForTransferCompletion = false
                            },
                            TimeSpan.FromMinutes(2),
                            cts.Token
                        ).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

            File.Delete(file);

            weTriggeredCancellation.Should().BeFalse();
            exception.InnerExceptions.Should().NotBeNullOrEmpty();
            exception.InnerExceptions.Should().OnlyContain(ex => ex is ChunkSizeOutOfRangeException);
            exception.InnerExceptions.Should().HaveCount(files.Count);
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
                }).ConfigureAwait(false);

            // Assert
            var businessProcess = await _client.BusinessProcess.WaitForCompletionAsync(result.Transfer.BusinessProcessId).ConfigureAwait(false);
            businessProcess.BusinessProcess.LastReportedProgress.Should().NotBeNull();
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldUseTargetFileNameForWebLinkDownloads()
        {
            // Act
            var result = await CreateWebTransferAsync(
                new[]
                {
                    ("http://cdn1.spiegel.de/images/image-733178-900_breitwand_180x67-zgpe-733178.jpg", "image.jpg")
                }).ConfigureAwait(false);

            // Assert
            var files = await _client.Transfer.SearchFilesByTransferIdAsync(result.Transfer.Id).ConfigureAwait(false);
            files.Should().HaveCount(1);
            files.Single().Name.Should().Be("image.jpg");
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldCallErrorDelegateForBlacklistedFile()
        {
            // Arrange
            var blacklist = await _fixture.Client.Transfer.GetBlacklistAsync().ConfigureAwait(false);
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
                options).ConfigureAwait(false);

            // Assert
            called.Should().BeTrue();
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldReturnEmptyResultIfAllFilesBlacklisted()
        {
            // Arrange
            var blacklist = await _fixture.Client.Transfer.GetBlacklistAsync().ConfigureAwait(false);
            var filename = blacklist.Items.First().Match;

            // Act
            var result = await _fixture.Client.Transfer.UploadFilesAsync(
                $"{Guid.NewGuid():N}",
                new[] { new FileLocations(filename) },
                null).ConfigureAwait(false);

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
                options).ConfigureAwait(false);

            // Assert
            errorDelegateArgs.file.AbsoluteSourcePath.Should().Be(filename);
            errorDelegateArgs.ex.Should().NotBeNull();
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldCallErrorDelegateWhenFileDoesNotExist1()
        {
            // Arrange
            var filename = $"{Guid.NewGuid():N}";
            var existing = $"{Guid.NewGuid():N}";
            File.WriteAllText(existing, "foo");

            (FileLocations file, Exception ex) errorDelegateArgs = (null, null);

            var options = new UploadOptions
            {
                ErrorDelegate = args => errorDelegateArgs = args,
                WaitForTransferCompletion = true
            };

            // Act
            await _fixture.Client.Transfer.UploadFilesAsync(
                $"{Guid.NewGuid():N}",
                new[]
                {
                    new FileLocations(filename),
                    new FileLocations(existing),
                },
                options).ConfigureAwait(false);

            // Assert
            errorDelegateArgs.file.AbsoluteSourcePath.Should().Be(filename);
            errorDelegateArgs.ex.Should().NotBeNull();
        }

        private async Task<(CreateTransferResult, string fileId)> CreateFileTransferAsync()
        {
            var transferName = new Random().Next(1000, 9999).ToString();
            var files = new FileLocations[]
            {
                Path.Combine(_fixture.ExampleFilesBasePath, "0030_JabLtzJl8bc.jpg")
            };

            var createTransferResult = await _client.Transfer.CreateAndWaitForCompletionAsync(transferName, files).ConfigureAwait(false);
            await _client.Transfer.UploadFilesAsync(createTransferResult.Transfer, files, new UploadOptions()).ConfigureAwait(false);

            var searchRequest = new FileTransferSearchRequest
            {
                Limit = 1,
                SearchString = "*",
                Filter = FilterBase.FromExpression<FileTransfer>(i => i.TransferId, createTransferResult.Transfer.Id)
            };
            var searchResult = await _client.Transfer.SearchFilesAsync(searchRequest).ConfigureAwait(false);
            var fileId = searchResult.Results.ToList()[0].Id;

            return (createTransferResult, fileId);
        }

        private async Task<CreateTransferResult> CreateWebTransferAsync(IReadOnlyList<string> urls)
        {
            return await CreateWebTransferAsync(urls.Select(url => (url, (string)null)).ToArray()).ConfigureAwait(false);
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

            return await _client.Transfer.CreateAndWaitForCompletionAsync(request).ConfigureAwait(false);
        }

        private async Task SetupSchema(Type type)
        {
            var schemas = await _client.Schema.GenerateSchemasAsync(type).ConfigureAwait(false);
            var schemasToCreate = new List<SchemaDetail>();

            foreach (var schema in schemas)
            {
                if (!await _client.Schema.ExistsAsync(schema.Id).ConfigureAwait(false))
                {
                    schemasToCreate.Add(schema);
                }
            }

            await _client.Schema.CreateManyAsync(schemasToCreate, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        }

        private async Task<string> CreatePerson()
        {
            var operationResult = await _client.ListItem.CreateFromObjectAsync(new Person
            {
                Firstname = "FirstName",
                LastName = "LastName",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                EmailAddress = "first.last@testyyy.com"
            }).ConfigureAwait(false);
            return (await operationResult.FetchDetail().ConfigureAwait(false)).SucceededIds.First();
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
                var downloadResult = await _client.Content.CreateDownloadLinkAsync(downloadRequest).ConfigureAwait(false);
                urls.Add(downloadResult.DownloadUrl);
            }

            return urls;
        }
    }
}
