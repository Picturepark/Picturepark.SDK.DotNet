using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "Outputs")]
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
        public async Task ShouldGet()
        {
            // Arrange
            string contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            Assert.False(string.IsNullOrEmpty(contentId));

            ContentDetail contentDetail = await _client.Content.GetAsync(contentId, new[] { ContentResolveBehavior.Outputs });
            Assert.True(contentId == contentDetail.Id, "Delivery goes wrong. We never ordered such pizza.");

            Assert.True(contentDetail.Outputs.Any());
            var outputId = contentDetail.Outputs.FirstOrDefault(o => !o.DynamicRendering)?.Id;
            Assert.False(string.IsNullOrEmpty(outputId));

            // Act
            OutputDetail result = await _client.Output.GetAsync(outputId);

            // Assert
            Assert.True(result.ContentId == contentId);
        }
    }
}
