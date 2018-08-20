using System;
using System.Collections.Generic;
using System.Linq;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class PublicAccessTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkClient _client;

        public PublicAccessTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        [Fact]
        [Trait("Stack", "PublicAccess")]
        public async Task ShouldGetVersion()
        {
            // Act
            var version = await _client.PublicAccess.GetVersionAsync().ConfigureAwait(false);

            // Assert
            Assert.NotNull(version.ContractVersion);
            Assert.NotNull(version.FileProductVersion);
            Assert.NotNull(version.FileVersion);
            Assert.NotNull(version.Release);
        }

        [Fact]
        [Trait("Stack", "PublicAccess")]
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

            var createResult = await _client.Shares.CreateAsync(request).ConfigureAwait(false);
            var embedDetail = await _client.Shares.GetAsync(createResult.ShareId).ConfigureAwait(false);

            // Act
            var result = await _client.PublicAccess.GetShareAsync(((ShareDataEmbed)embedDetail.Data).Token).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
        }
    }
}
