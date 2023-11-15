using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "IdentityProviders")]
    public class IdentityProviderTests : IClassFixture<ClientFixture>
    {
        private readonly IPictureparkService _client;

        public IdentityProviderTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task ShouldContainSomeSynchronizableAttributes()
        {
            // Act
            var results = await _client.IdentityProvider.GetSynchronizableAttributesAsync();

            // Assert
            results.Should().NotBeEmpty("there should be some synchronizable attributes");
        }
    }
}