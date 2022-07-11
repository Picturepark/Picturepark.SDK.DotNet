using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "ConversionPresetTemplates")]
    public class CommentTests : IClassFixture<ClientFixture>
    {
        private readonly IPictureparkService _client;

        public CommentTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task ShouldCreateUpdateAndDelete()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<ContentItem>(_client).ConfigureAwait(false);

            var request = new ContentCreateRequest
            {
                Content = new ContentItem { Name = "Something" },
                ContentSchemaId = nameof(ContentItem)
            };

            var content = await _client.Content.CreateAsync(request, new[] { ContentResolveBehavior.OuterDisplayValueThumbnail }, timeout: TimeSpan.FromMinutes(5)).ConfigureAwait(false);

            var message = Guid.NewGuid().ToString("N");
            var createRequest = new CommentCreateRequest
            {
                Message = message
            };

            var comment = await _client.Content.CreateCommentAsync(content.Id, createRequest).ConfigureAwait(false);
            comment.Should().NotBeNull();
            comment.Message.Should().Be(message);

            var updatedMessage = Guid.NewGuid().ToString("N");
            var updateRequest = new CommentEditable
            {
                Message = updatedMessage
            };
            comment = await _client.Content.UpdateCommentAsync(comment.Id, updateRequest).ConfigureAwait(false);
            comment.Should().NotBeNull();
            comment.Message.Should().Be(updatedMessage);

            await _client.Content.DeleteCommentAsync(comment.Id).ConfigureAwait(false);

            await Assert.ThrowsAsync<CommentNotFoundException>(() => _client.Content.GetCommentAsync(comment.Id)).ConfigureAwait(false);
        }

        [Fact]
        public async Task ShouldSearch()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<ContentItem>(_client).ConfigureAwait(false);

            var request = new ContentCreateRequest
            {
                Content = new ContentItem { Name = "Something" },
                ContentSchemaId = nameof(ContentItem)
            };

            var content = await _client.Content.CreateAsync(request, new[] { ContentResolveBehavior.OuterDisplayValueThumbnail }, timeout: TimeSpan.FromMinutes(5)).ConfigureAwait(false);

            var messages = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid().ToString("N")).ToArray();

            foreach (var message in messages)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                var createRequest = new CommentCreateRequest
                {
                    Message = message
                };

                await _client.Content.CreateCommentAsync(content.Id, createRequest).ConfigureAwait(false);
            }

            var comments = await _client.Content.SearchCommentsAsync(content.Id, new CommentSearchRequest()).ConfigureAwait(false);
            comments.Results.Should().HaveCount(10);
            comments.Results.Select(c => c.Message).Should().BeEquivalentTo(messages);

            foreach (var comment in comments.Results)
            {
                await _client.Content.DeleteCommentAsync(comment.Id).ConfigureAwait(false);
            }
        }
    }
}