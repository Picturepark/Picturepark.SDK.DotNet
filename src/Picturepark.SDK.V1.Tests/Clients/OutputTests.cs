using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class OutputTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public OutputTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Outputs")]
        public async Task ShouldGet()
        {
            // Arrange
            string contentId = await _fixture.GetRandomContentIdAsync(".jpg", 20).ConfigureAwait(false);
            Assert.False(string.IsNullOrEmpty(contentId));

            ContentDetail contentDetail = await _client.Content.GetAsync(contentId, new[] { ContentResolveBehaviour.Outputs }).ConfigureAwait(false);
            Assert.True(contentId == contentDetail.Id, "Delivery goes wrong. We never ordered such pizza.");

            Assert.True(contentDetail.Outputs.Any());
            var outputId = contentDetail.Outputs.FirstOrDefault()?.Id;
            Assert.False(string.IsNullOrEmpty(outputId));

            // Act
            OutputDetail result = await _client.Output.GetAsync(outputId).ConfigureAwait(false);

            // Assert
            Assert.True(result.ContentId == contentId);
        }

        [Fact]
        [Trait("Stack", "Outputs")]
        public async Task ShouldGetByContentIds()
        {
            // Arrange
            string contentId = await _fixture.GetRandomContentIdAsync(".jpg", 20).ConfigureAwait(false);
            var request = new OutputSearchRequest
            {
                ContentIds = new List<string> { contentId }
            };

            // Act
            var result = await _client.Output.SearchAsync(request).ConfigureAwait(false);

            // Assert
            Assert.True(result.Results.ToList()[0].ContentId == contentId);
        }
    }
}
