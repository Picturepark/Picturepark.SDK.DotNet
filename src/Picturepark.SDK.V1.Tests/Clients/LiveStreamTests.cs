using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class LiveStreamTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public LiveStreamTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "LiveStream")]
        public async Task ShouldReturnSearchResultsCorrectly()
        {
            // Arrange
            var time = DateTime.Now;
            var createdUser = await _fixture.Users.Create();

            var request = new LiveStreamSearchRequest
            {
                Limit = 10,
                From = time - TimeSpan.FromSeconds(10), // handle potential clock skew
                ScopeType = "DocumentChange"
            };

            // Give some time for the live stream event to be processed
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Act
            var result = await _client.LiveStream.SearchAsync(request);

            // Assert
            result.Results.Should().NotBeEmpty();

            var livestreamMessages = result.Results.Select(r => LiveStreamMessage.FromJson(r.Document));
            livestreamMessages.Select(m => m.DocumentChange.DocumentId).Should().Contain(createdUser.Id);
        }
    }
}
