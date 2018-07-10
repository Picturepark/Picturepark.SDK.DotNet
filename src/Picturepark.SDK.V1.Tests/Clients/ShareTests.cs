using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Extensions;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Linq;
using FluentAssertions;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class ShareTests : IClassFixture<ShareFixture>
    {
        private readonly ShareFixture _fixture;
        private readonly PictureparkClient _client;

        public ShareTests(ShareFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldAggregate()
        {
            /// Arrange
            var createRequest = new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            };

            var createResult = await _client.Shares.CreateAsync(createRequest);
            _fixture.CreatedShareIds.Enqueue(createResult.ShareId);

            /// Act
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

            var result = await _client.Shares.AggregateAsync(request);

            /// Assert
            var aggregation = result.GetByName("ShareType");
            Assert.NotNull(aggregation);
            Assert.True(aggregation.AggregationResultItems.Count >= 1);
        }

        [Fact(Skip = "Fix")]
        [Trait("Stack", "Shares")]
        public async Task ShouldUpdate()
        {
            /// Arrange
            var createRequest = new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            };

            var createResult = await _client.Shares.CreateAsync(createRequest);
            _fixture.CreatedShareIds.Enqueue(createResult.ShareId);

            /// Act
            var request = new ShareEmbedUpdateRequest
            {
                Id = createResult.ShareId,
                Description = "Foo"
            };

            var result = await _client.Shares.UpdateAsync(createResult.ShareId, request);

            /// Assert
            var share = await _client.Shares.GetAsync(createResult.ShareId);
            Assert.Equal("Foo", share.Description);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldDeleteMany()
        {
            /// Arrange
            var createRequest = new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            };

            var createResult = await _client.Shares.CreateAsync(createRequest);
            _fixture.CreatedShareIds.Enqueue(createResult.ShareId);

            var share = await _client.Shares.GetAsync(createResult.ShareId);
            Assert.Equal(createResult.ShareId, share.Id);

            /// Act
            var shareIds = new List<string> { createResult.ShareId };
            var bulkResponse = await _client.Shares.DeleteManyAsync(shareIds);

            /// Assert
            Assert.All(bulkResponse.Rows, i => Assert.True(i.Succeeded));
            await Assert.ThrowsAsync<ShareNotFoundException>(async () => await _client.Shares.GetAsync(createResult.ShareId));
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldCreateBasicShare()
        {
            /// Arrange
            var request = new ShareBasicCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Basic share aaa",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Basic share aaa",
                RecipientsEmail = new List<UserEmail>
                {
                    _fixture.Configuration.EmailRecipient
                }
            };

            /// Act
            var createResult = await _client.Shares.CreateAsync(request);
            _fixture.CreatedShareIds.Enqueue(createResult.ShareId);

            /// Assert
            Assert.NotNull(createResult);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldCreateBasicShareWithWrongContentsAndFail()
        {
            /// Arrange
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

            /// Act and Assert
            await Assert.ThrowsAsync<ContentNotFoundException>(async () =>
            {
                var result = await _client.Shares.CreateAsync(request);
            });
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldCreateEmbedShare()
        {
            // Arrange
            var request = new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            };

            // Act
            var createResult = await _client.Shares.CreateAsync(request);
            _fixture.CreatedShareIds.Enqueue(createResult.ShareId);

            /// Assert
            var share = await _client.Shares.GetAsync(createResult.ShareId);
            Assert.Equal(createResult.ShareId, share.Id);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldSearch()
        {
            // Arrange
            var createRequest = new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share to search" + new Random().Next(0, 999999)
            };
            var createResult = await _client.Shares.CreateAsync(createRequest);
            _fixture.CreatedShareIds.Enqueue(createResult.ShareId);

            // Act
            var request = new ShareSearchRequest
            {
                Start = 0,
                Limit = 100,
                Filter = new AndFilter()
                {
                    Filters = new List<FilterBase>
                    {
                        FilterBase.FromExpression<Share>(i => i.ShareType, ShareType.Embed.ToString()),
                        FilterBase.FromExpression<Share>(i => i.Name, createRequest.Name)
                    }
                }
            };

            /// Act
            var result = await _client.Shares.SearchAsync(request);

            /// Assert
            result.TotalResults.Should().Be(1);
            result.Results.First().Name.Should().Be(createRequest.Name);
        }

        private async Task<List<ShareContent>> GetRandomShareContent(int count = 2)
        {
            var outputFormatIds = new List<string> { "Original" };

            var randomContents = await _fixture.GetRandomContentsAsync(string.Empty, count);
            var shareContentItems = randomContents.Results.Select(i =>
                new ShareContent
                {
                    ContentId = i.Id,
                    OutputFormatIds = outputFormatIds
                }).ToList();

            return shareContentItems;
        }
    }
}
