using Picturepark.SDK.V1.Contract;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class SchemaPermissionSetTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public SchemaPermissionSetTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "SchemaPermissionSets")]
        public async Task ShouldGetSchemaPermissionSet()
        {
            // Arrange
            string permissionSetId = await _fixture.GetRandomSchemaPermissionSetIdAsync(20).ConfigureAwait(false);
            Assert.False(string.IsNullOrEmpty(permissionSetId));

            // Act
            SchemaPermissionSetDetail result = await _client.SchemaPermissionSet.GetAsync(permissionSetId).ConfigureAwait(false);

            // Assert
            Assert.False(string.IsNullOrEmpty(result.Id));
        }

        [Fact]
        [Trait("Stack", "SchemaPermissionSets")]
        public async Task ShouldSearchSchemaPermissionSets()
        {
            // Arrange
            var request = new PermissionSetSearchRequest { Limit = 20 };

            // Act
            PermissionSetSearchResult result = await _client.SchemaPermissionSet.SearchAsync(request).ConfigureAwait(false);

            // Assert
            Assert.True(result.Results.Count > 0);
        }
    }
}
