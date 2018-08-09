using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class TransferTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly PictureparkClient _client;

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
            var blacklist = await _client.Transfers.GetBlacklistAsync();

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
            var result = await _client.Transfers.CreateAndWaitForCompletionAsync(transferName, files);

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

            await _client.Transfers.GetFileAsync(fileId).ConfigureAwait(false);

            await _client.Transfers.DeleteFilesAsync(request).ConfigureAwait(false);

            // Assert
            await Assert.ThrowsAsync<FileTransferNotFoundException>(async () =>
            {
                await _client.Transfers.GetFileAsync(fileId).ConfigureAwait(false);
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

            var result = await _client.Transfers.CreateAndWaitForCompletionAsync(transferName, files);
            var originalTransfer = await _client.Transfers.GetAsync(result.Transfer.Id);

            // Act
            await _client.Transfers.CancelTransferAsync(result.Transfer.Id);

            // Assert
            var currentTransfer = await _client.Transfers.GetAsync(result.Transfer.Id);

            Assert.Equal(TransferState.Created, originalTransfer.State);
            Assert.Equal(TransferState.TransferReady, currentTransfer.State);
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
                    Identifier = Guid.NewGuid().ToString()
                }).ToList()
            };

            var result = await _client.Transfers.CreateAndWaitForCompletionAsync(request);

            // Assert
            Assert.NotNull(result.Transfer);
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldDelete()
        {
            // Arrange
            var result = await CreateWebTransferAsync();

            // Act
            var transferId = result.Transfer.Id;
            var transfer = await _client.Transfers.GetAsync(result.Transfer.Id);

            await _client.Transfers.DeleteAsync(transferId);

            // Assert
            Assert.NotNull(transfer);
            await Assert.ThrowsAsync<TransferNotFoundException>(async () => await _client.Transfers.GetAsync(transferId));
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldGet()
        {
            // Arrange
            var result = await CreateWebTransferAsync();

            // Act
            TransferDetail transfer = await _client.Transfers.GetAsync(result.Transfer.Id);

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
            var transfer = await _client.Transfers.GetFileAsync(fileId);

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
            TransferSearchResult result = await _client.Transfers.SearchAsync(request);

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
            FileTransferSearchResult result = await _client.Transfers.SearchFilesAsync(request);

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

            var randomNumber = new Random().Next(0, numberOfFilesInDirectory - numberOfUploadFiles);
            var importFilePaths = filesInDirectory
                .Skip(randomNumber)
                .Take(numberOfUploadFiles)
                .Select(path => new FileLocations(path)).ToList();

            // Act
            var uploadOptions = new UploadOptions
            {
                ConcurrentUploads = 4,
                ChunkSize = 20 * 1024,
                SuccessDelegate = Console.WriteLine,
                ErrorDelegate = Console.WriteLine
            };

            var createTransferResult = await _client.Transfers.UploadFilesAsync(transferName, importFilePaths, uploadOptions);
            var transfer = await _client.Transfers.PartialImportAsync(createTransferResult.Transfer.Id, new FileTransferPartial2ContentCreateRequest
            {
                Items = new List<FileTransferCreateItem>
                {
                    new FileTransferCreateItem
                    {
                        FileId = createTransferResult.FileUploads.First().Identifier
                    }
                }
            });

            // Assert
            var result = await _client.Transfers.SearchFilesByTransferIdAsync(transfer.Id);
            Assert.Equal(importFilePaths.Count, result.Results.Count);
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
                ChunkSize = 20 * 1024,
                SuccessDelegate = Console.WriteLine,
                ErrorDelegate = Console.WriteLine
            };
            var createTransferResult = await _client.Transfers.UploadFilesAsync(transferName, importFilePaths, uploadOptions);

            var importRequest = new FileTransfer2ContentCreateRequest
            {
                ContentPermissionSetIds = new List<string>(),
                Metadata = null,
                LayerSchemaIds = new List<string>()
            };

            await _client.Transfers.ImportAndWaitForCompletionAsync(createTransferResult.Transfer, importRequest, timeout);

            // Assert
            var result = await _client.Transfers.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id);
            var contentIds = result.Results.Select(r => r.ContentId);

            Assert.Equal(importFilePaths.Count(), contentIds.Count());
        }

        [Fact]
        [Trait("Stack", "Transfers")]
        public async Task ShouldRespectTimeoutWhileUploading()
        {
            var transferName = Guid.NewGuid().ToString();

            var ex = await Assert.ThrowsAnyAsync<Exception>(
                async () =>
                    await _client.Transfers.UploadFilesAsync(
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
            var result = await _client.Transfers.UploadFilesAsync(
                transferName,
                files,
                new UploadOptions()
                {
                    WaitForTransferCompletion = true,
                    ErrorDelegate = Console.WriteLine,
                    SuccessDelegate = Console.WriteLine
                }).ConfigureAwait(false);

            // Assert
            result.FileUploads.Should().HaveCount(2);
        }

        private async Task<(CreateTransferResult, string fileId)> CreateFileTransferAsync()
        {
            var transferName = new Random().Next(1000, 9999).ToString();
            var files = new FileLocations[]
            {
                Path.Combine(_fixture.ExampleFilesBasePath, "0030_JabLtzJl8bc.jpg")
            };

            var createTransferResult = await _client.Transfers.CreateAndWaitForCompletionAsync(transferName, files).ConfigureAwait(false);
            await _client.Transfers.UploadFilesAsync(createTransferResult.Transfer, files, new UploadOptions()).ConfigureAwait(false);

            var searchRequest = new FileTransferSearchRequest
            {
                Limit = 1,
                SearchString = "*",
                Filter = FilterBase.FromExpression<FileTransfer>(i => i.TransferId, createTransferResult.Transfer.Id)
            };
            var searchResult = await _client.Transfers.SearchFilesAsync(searchRequest).ConfigureAwait(false);
            var fileId = searchResult.Results.ToList()[0].Id;

            return (createTransferResult, fileId);
        }

        private async Task<CreateTransferResult> CreateWebTransferAsync()
        {
            var transferName = "UrlImport " + new Random().Next(1000, 9999);
            var urls = new List<string>
            {
                "https://picturepark.com/wp-content/uploads/2013/06/home-marquee.jpg",
                "http://cdn1.spiegel.de/images/image-733178-900_breitwand_180x67-zgpe-733178.jpg",
                "http://cdn3.spiegel.de/images/image-1046236-700_poster_16x9-ymle-1046236.jpg"
            };

            var request = new CreateTransferRequest
            {
                Name = transferName,
                TransferType = TransferType.WebDownload,
                WebLinks = urls.Select(url => new TransferWebLink
                {
                    Url = url,
                    Identifier = Guid.NewGuid().ToString()
                }).ToList()
            };

            return await _client.Transfers.CreateAndWaitForCompletionAsync(request).ConfigureAwait(false);
        }
    }
}
