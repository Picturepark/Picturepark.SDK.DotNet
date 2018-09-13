using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Linq;
using FluentAssertions;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class ShareTests : IClassFixture<ShareFixture>
    {
        private readonly ShareFixture _fixture;
        private readonly IPictureparkService _client;

        public ShareTests(ShareFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldAggregate()
        {
            // Arrange
            await CreateShare(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            }).ConfigureAwait(false);

            // Act
            var request = new ShareAggregationRequest
            {
                SearchString = string.Empty,
                Aggregators = new List<AggregatorBase>
                {
                    new TermsAggregator
                    {
                        Field = nameof(Share.ShareType).ToLowerCamelCase(),
                        Size = 10,
                        Name = "ShareType"
                    }
                }
            };

            var result = await _client.Share.AggregateAsync(request).ConfigureAwait(false);

            // Assert
            var aggregation = result.GetByName("ShareType");
            aggregation.Should().NotBeNull();
            aggregation.AggregationResultItems.Count.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldUpdateEmbed()
        {
            // Arrange
            var createResult = await CreateShare(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            }).ConfigureAwait(false);

            var share = await _client.Share.GetAsync(createResult.ShareId).ConfigureAwait(false);

            // Act
            var request = share.AsEmbedUpdateRequest(r => r.Description = "Foo");

            await _client.Share.UpdateAsync(createResult.ShareId, request).ConfigureAwait(false);

            // Assert
            var updatedShare = await _client.Share.GetAsync(createResult.ShareId).ConfigureAwait(false);
            updatedShare.Description.Should().Be("Foo");
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldDeleteMany()
        {
            // Arrange
            var createResult = await CreateShare(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            }).ConfigureAwait(false);

            _fixture.CreatedShareIds.Enqueue(createResult.ShareId);

            var share = await _client.Share.GetAsync(createResult.ShareId).ConfigureAwait(false);
            createResult.ShareId.Should().Be(share.Id);

            // Act
            var deleteManyRequest = new ShareDeleteManyRequest
            {
                Ids = new List<string> { createResult.ShareId }
            };
            var bulkResponse = await _client.Share.DeleteManyAsync(deleteManyRequest).ConfigureAwait(false);

            // Assert
            bulkResponse.Rows.Should().OnlyContain(i => i.Succeeded);
            await Assert.ThrowsAsync<ShareNotFoundException>(async () =>
            {
                await _client.Share.GetAsync(createResult.ShareId).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldCreateBasicShare()
        {
            // Arrange
            // Act
            var createResult = await CreateShare(new ShareBasicCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Basic share aaa",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Basic share aaa",
                RecipientsEmail = new List<UserEmail>
                {
                    _fixture.Configuration.EmailRecipient
                }
            }).ConfigureAwait(false);

            // Assert
            createResult.Should().NotBeNull();
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldCreateBasicShareWithWrongContentsAndFail()
        {
            // Arrange
            var outputFormatIds = new List<string> { "Original" };

            var shareContentItems = new List<ShareContent>
            {
                new ShareContent { ContentId = "NonExistingId1", OutputFormatIds = outputFormatIds },
                new ShareContent { ContentId = "NonExistingId2", OutputFormatIds = outputFormatIds }
            };

            var request = new ShareBasicCreateRequest
            {
                Contents = shareContentItems,
                Description = "Description of share with wrong content ids",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Share with wrong content ids",
                RecipientsEmail = new List<UserEmail>
                {
                    _fixture.Configuration.EmailRecipient
                }
            };

            // Act and Assert
            await Assert.ThrowsAsync<ContentNotFoundException>(async () =>
            {
                await _client.Share.CreateAsync(request).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldCreateEmbedShare()
        {
            // Arrange
            // Act
            var createResult = await CreateShare(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            }).ConfigureAwait(false);

            // Assert
            var share = await _client.Share.GetAsync(createResult.ShareId).ConfigureAwait(false);
            share.Id.Should().Be(createResult.ShareId);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldFailOnSharingDuplicatedContentOutputs()
        {
            // Arrange
            var contents = await GetRandomShareContent(1).ConfigureAwait(false);

            // Share content twice
            contents.Add(contents.First());

            await Assert.ThrowsAsync<InvalidArgumentException>(async () =>
            {
                await CreateShare(new ShareEmbedCreateRequest
                {
                    Contents = contents,
                    Description = "Description of Embed share bbb",
                    ExpirationDate = new DateTime(2020, 12, 31),
                    Name = "Embed share bbb"
                }).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldSearch()
        {
            // Arrange
            var name = "Embed share to search" + new Random().Next(0, 999999);
            await CreateShare(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = name
            }).ConfigureAwait(false);

            // Act
            var request = new ShareSearchRequest
            {
                Start = 0,
                Limit = 100,
                Filter = new AndFilter
                {
                    Filters = new List<FilterBase>
                    {
                        FilterBase.FromExpression<Share>(i => i.ShareType, ShareType.Embed.ToString()),
                        FilterBase.FromExpression<Share>(i => i.Name, name)
                    }
                }
            };

            // Act
            var result = await _client.Share.SearchAsync(request).ConfigureAwait(false);

            // Assert
            result.TotalResults.Should().Be(1);
            result.Results.First().Name.Should().Be(name);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldGetShareByToken()
        {
            // Arrange
            var outputFormatIds = new List<string> { "Original", "Preview" };
            var shareContentItems = new List<ShareContent>
            {
                new ShareContent { ContentId = await _fixture.GetRandomContentIdAsync(string.Empty, 30).ConfigureAwait(false), OutputFormatIds = outputFormatIds },
                new ShareContent { ContentId = await _fixture.GetRandomContentIdAsync(string.Empty, 30).ConfigureAwait(false), OutputFormatIds = outputFormatIds }
            };

            var request = new ShareEmbedCreateRequest
            {
                Contents = shareContentItems,
                Description = "Description of Embed share",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share"
            };

            var createResult = await _client.Share.CreateAsync(request).ConfigureAwait(false);
            var embedDetail = await _client.Share.GetAsync(createResult.ShareId).ConfigureAwait(false);

            // Act
            var result = await _client.Share.GetShareJsonAsync(((ShareDataEmbed)embedDetail.Data).Token).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldGetShareOutputByToken()
        {
            // Arrange
            var contents = await GetRandomShareContent(".jpg").ConfigureAwait(false);
            var createResult = await CreateShare(new ShareEmbedCreateRequest
            {
                Contents = contents,
                Description = "Description of Embed share",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share"
            }).ConfigureAwait(false);
            var embedDetail = await _client.Share.GetAsync(createResult.ShareId).ConfigureAwait(false);

            var shareOutput = (ShareOutputEmbed)embedDetail.ContentSelections.First().Outputs.First();

            // Act
            using (var result = await _client.Share.DownloadAsync(shareOutput.Token).ConfigureAwait(false))
            {
                // Assert
                Assert.NotNull(result);
            }
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldGetShareOutputByContentIdAndOutputFormatId()
        {
            var contents = await GetRandomShareContent(".jpg").ConfigureAwait(false);
            var createResult = await CreateShare(new ShareEmbedCreateRequest
            {
                Contents = contents,
                Description = "Description of Embed share",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share"
            }).ConfigureAwait(false);
            var embedDetail = await _client.Share.GetAsync(createResult.ShareId).ConfigureAwait(false);

            // Act
            using (var result = await _client.Share
                .DownloadWithOutputFormatIdAsync(((ShareDataEmbed)embedDetail.Data).Token, contents.First().ContentId, "Original")
                .ConfigureAwait(false))
            {
                // Assert
                Assert.NotNull(result);
            }
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldGetShareOutputByContentIdAndSize()
        {
            // Arrange transfer
            var timeout = TimeSpan.FromMinutes(2);
            var transferName = nameof(ShouldGetShareOutputByContentIdAndSize) + "-" + new Random().Next(1000, 9999);

            var filesInDirectory = Directory.GetFiles(_fixture.ExampleFilesBasePath, "0033_aeVA-j1y2BY.jpg").ToList();

            var importFilePaths = filesInDirectory.Select(fn => new FileLocations(fn, $"{Path.GetFileNameWithoutExtension(fn)}_1{Path.GetExtension(fn)}")).ToList();

            // Act
            var uploadOptions = new UploadOptions
            {
                SuccessDelegate = Console.WriteLine,
                ErrorDelegate = Console.WriteLine
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
            var transferResult = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id).ConfigureAwait(false);
            var contentIds = transferResult.Results.Select(r => r.ContentId).ToList();

            Assert.Equal(importFilePaths.Count, contentIds.Count);

            // Arrange get share
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

            var createResult = await _client.Share.CreateAsync(request).ConfigureAwait(false);
            var embedDetail = await _client.Share.GetAsync(createResult.ShareId).ConfigureAwait(false);

            var shareOutput = (ShareOutputEmbed)embedDetail.ContentSelections.Single().Outputs.First();

            // Act
            using (var result = await _client.Share.DownloadAsync(shareOutput.Token, 10, 10).ConfigureAwait(false))
            {
                // Assert
                Assert.NotNull(result);
            }
        }

        private async Task<List<ShareContent>> GetRandomShareContent(int count = 2)
        {
            var outputFormatIds = new List<string> { "Original" };

            var randomContents = await _fixture.GetRandomContentsAsync(string.Empty, count).ConfigureAwait(false);
            var shareContentItems = randomContents.Results.Select(i =>
                new ShareContent
                {
                    ContentId = i.Id,
                    OutputFormatIds = outputFormatIds
                }).ToList();

            return shareContentItems;
        }

        private async Task<List<ShareContent>> GetRandomShareContent(string searchstring, int count = 2)
        {
            var outputFormatIds = new List<string> { "Original", "Preview" };

            var randomContents = await _fixture.GetRandomContentsAsync(searchstring, count).ConfigureAwait(false);
            var shareContentItems = randomContents.Results.Select(i =>
                new ShareContent
                {
                    ContentId = i.Id,
                    OutputFormatIds = outputFormatIds
                }).ToList();

            return shareContentItems;
        }

        private async Task<CreateShareResult> CreateShare(ShareBaseCreateRequest createRequest)
        {
            var createResult = await _client.Share.CreateAsync(createRequest).ConfigureAwait(false);
            _fixture.CreatedShareIds.Enqueue(createResult.ShareId);
            return createResult;
        }
    }
}
