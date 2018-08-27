using Picturepark.SDK.V1.Contract;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class ContentPermissionSetTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public ContentPermissionSetTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "ContentPermissionSets")]
        public async Task ShouldGetContentPermissionSet()
        {
            // Arrange
            string permissionSetId = await _fixture.GetRandomContentPermissionSetIdAsync(20).ConfigureAwait(false);
            Assert.False(string.IsNullOrEmpty(permissionSetId));

            // Act
            ContentPermissionSetDetail result = await _client.ContentPermissionSet.GetAsync(permissionSetId).ConfigureAwait(false);

            // Assert
            Assert.False(string.IsNullOrEmpty(result.Id));
        }

        [Fact]
        [Trait("Stack", "ContentPermissionSets")]
        public async Task ShouldSearchContentPermissionSets()
        {
            // Arrange
            var request = new PermissionSetSearchRequest { Limit = 20 };

            // Act
            PermissionSetSearchResult result = await _client.ContentPermissionSet.SearchAsync(request).ConfigureAwait(false);

            // Assert
            Assert.True(result.Results.Count > 0);
        }
    }
}
