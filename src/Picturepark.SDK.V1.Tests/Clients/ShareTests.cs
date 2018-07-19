using System;
using System.Collections.Generic;
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

            var result = await _client.Shares.AggregateAsync(request).ConfigureAwait(false);

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

            var share = await _client.Shares.GetAsync(createResult.ShareId).ConfigureAwait(false);

            // Act
            // TODO: Simplify (ShareDetail.AsEmbedUpdateRequest()) ?
            var request = new ShareEmbedUpdateRequest
            {
                Description = "Foo",
                ExpirationDate = share.ExpirationDate,
                LayerSchemaIds = share.LayerSchemaIds,
                Name = share.Name,
                OutputAccess = share.OutputAccess,
                ShareContentItems = share.ContentSelections.Select(i =>
                    new ShareContent
                    {
                        ContentId = i.Id,
                        OutputFormatIds = i.Outputs.Select(output => output.OutputFormatId).ToList()
                    }).ToList(),
                Template = share.Template
            };

            await _client.Shares.UpdateAsync(createResult.ShareId, request).ConfigureAwait(false);

            // Assert
            var updatedShare = await _client.Shares.GetAsync(createResult.ShareId).ConfigureAwait(false);
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

            var share = await _client.Shares.GetAsync(createResult.ShareId).ConfigureAwait(false);
            createResult.ShareId.Should().Be(share.Id);

            // Act
            var shareIds = new List<string> { createResult.ShareId };
            var bulkResponse = await _client.Shares.DeleteManyAsync(shareIds).ConfigureAwait(false);

            // Assert
            bulkResponse.Rows.Should().OnlyContain(i => i.Succeeded);
            await Assert.ThrowsAsync<ShareNotFoundException>(async () =>
            {
                await _client.Shares.GetAsync(createResult.ShareId).ConfigureAwait(false);
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
                await _client.Shares.CreateAsync(request).ConfigureAwait(false);
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
            var share = await _client.Shares.GetAsync(createResult.ShareId).ConfigureAwait(false);
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
            var result = await _client.Shares.SearchAsync(request).ConfigureAwait(false);

            // Assert
            result.TotalResults.Should().Be(1);
            result.Results.First().Name.Should().Be(name);
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

        private async Task<CreateShareResult> CreateShare(ShareBaseCreateRequest createRequest)
        {
            var createResult = await _client.Shares.CreateAsync(createRequest).ConfigureAwait(false);
            _fixture.CreatedShareIds.Enqueue(createResult.ShareId);
            return createResult;
        }
    }
}
