using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class ShareAccessTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly PictureparkClient _client;

        public ShareAccessTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        [Fact]
        [Trait("Stack", "ShareAccess")]
        public async Task ShouldGetShareByToken()
        {
            /// Arrange
            var outputFormatIds = new List<string> { "Original", "Preview" };
            var shareContentItems = new List<ShareContent>
            {
                new ShareContent { ContentId = await _fixture.GetRandomContentIdAsync(string.Empty, 30), OutputFormatIds = outputFormatIds },
                new ShareContent { ContentId = await _fixture.GetRandomContentIdAsync(string.Empty, 30), OutputFormatIds = outputFormatIds }
            };

            var request = new ShareEmbedCreateRequest
            {
                Contents = shareContentItems,
                Description = "Description of Embed share",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share"
            };

            var createResult = await _client.Shares.CreateAsync(request);
            var embedDetail = await _client.Shares.GetAsync(createResult.ShareId);

            /// Act
            var result = await _client.ShareAccess.DownloadAsync(((ShareDataEmbed)embedDetail.Data).Token);

            /// Assert
            Assert.NotNull(result);
        }

        [Fact]
        [Trait("Stack", "ShareAccess")]
        public async Task ShouldGetShareOutputByToken()
        {
            /// Arrange
            var outputFormatIds = new List<string> { "Original", "Preview" };
            var contentId1 = await _fixture.GetRandomContentIdAsync(".jpg", 30);
            var contentId2 = await _fixture.GetRandomContentIdAsync(".jpg", 30);

            var shareContentItems = new List<ShareContent>
            {
                new ShareContent { ContentId = contentId1, OutputFormatIds = outputFormatIds },
                new ShareContent { ContentId = contentId2, OutputFormatIds = outputFormatIds }
            };

            var request = new ShareEmbedCreateRequest
            {
                Contents = shareContentItems,
                Description = "Description of Embed share",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share"
            };

            var createResult = await _client.Shares.CreateAsync(request);
            var embedDetail = await _client.Shares.GetAsync(createResult.ShareId);

            var shareOutputs = embedDetail.ContentSelections.First().Outputs.First() as ShareOutputEmbed;

            /// Act
            var result = await _client.ShareAccess.DownloadAsync(shareOutputs.Token);

            /// Assert
            Assert.NotNull(result);
        }

        [Fact]
        [Trait("Stack", "ShareAccess")]
        public async Task ShouldGetShareOutputByContentIdAndOutputFormatId()
        {
            /// Arrange
            var outputFormatIds = new List<string> { "Original", "Preview" };
            var contentId1 = await _fixture.GetRandomContentIdAsync(".jpg", 30);
            var contentId2 = await _fixture.GetRandomContentIdAsync(".jpg", 30);

            var shareContentItems = new List<ShareContent>
            {
                new ShareContent { ContentId = contentId1, OutputFormatIds = outputFormatIds },
                new ShareContent { ContentId = contentId2, OutputFormatIds = outputFormatIds }
            };

            var request = new ShareEmbedCreateRequest
            {
                Contents = shareContentItems,
                Description = "Description of Embed share",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share"
            };

            var createResult = await _client.Shares.CreateAsync(request);
            var embedDetail = await _client.Shares.GetAsync(createResult.ShareId);

            var shareOutputs = embedDetail.ContentSelections.First().Outputs.First() as ShareOutputEmbed;

            /// Act
            var result = await _client.ShareAccess.DownloadAsync(((ShareDataEmbed)embedDetail.Data).Token, contentId1, "Original");

            /// Assert
            Assert.NotNull(result);
        }

        [Fact]
        [Trait("Stack", "ShareAccess")]
        public async Task ShouldGetShareOutputByContentIdAndSize()
        {
            /// Arrange transfer
            var timeout = TimeSpan.FromMinutes(2);
            var transferName = nameof(ShouldGetShareOutputByContentIdAndSize) + "-" + new Random().Next(1000, 9999);

            var filesInDirectory = Directory.GetFiles(_fixture.ExampleFilesBasePath, "0033_aeVA-j1y2BY.jpg").ToList();

            var importFilePaths = filesInDirectory.Select(fn => new FileLocations(fn, $"{Path.GetFileNameWithoutExtension(fn)}_1{Path.GetExtension(fn)}"));

            /// Act
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
                TransferId = createTransferResult.Transfer.Id,
                ContentPermissionSetIds = new List<string>(),
                Metadata = null,
                LayerSchemaIds = new List<string>()
            };

            await _client.Transfers.ImportAndWaitForCompletionAsync(createTransferResult.Transfer, importRequest, timeout);

            /// Assert
            var transferResult = await _client.Transfers.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id);
            var contentIds = transferResult.Results.Select(r => r.ContentId);

            Assert.Equal(importFilePaths.Count(), contentIds.Count());

            /// Arrange get share
            var contentId = contentIds.First();
            var outputFormatIds = new List<string> { "Original", "Preview" };
            var shareContentItems = new List<ShareContent>
            {
                new ShareContent { ContentId = contentId, OutputFormatIds = outputFormatIds },
            };

            var request = new ShareEmbedCreateRequest
            {
                Contents = shareContentItems,
                Description = "Description of Embed share",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share"
            };

            var createResult = await _client.Shares.CreateAsync(request);
            var embedDetail = await _client.Shares.GetAsync(createResult.ShareId);

            var shareOutputs = embedDetail.ContentSelections.Single().Outputs.First() as ShareOutputEmbed;

            /// Act
            var result = await _client.ShareAccess.DownloadAsync(((ShareDataEmbed)embedDetail.Data).Token, contentId, 10, 10);

            /// Assert
            Assert.NotNull(result);
        }
    }
}
