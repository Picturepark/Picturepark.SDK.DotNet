using FluentAssertions;
using System.Threading.Tasks;
using Picturepark.SDK.V1.CloudManager.Contract;
using Xunit;
using Picturepark.SDK.V1.CloudManager.Tests.Fixtures;

namespace Picturepark.SDK.V1.CloudManager.Tests
{
    [Trait("Stack-CloudManager", "ServiceProvider")]
    public class ServiceProviderTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;

        public ServiceProviderTests(ClientFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldCreateAndDeleteServiceProvider()
        {
            ServiceProviderCreateRequest request = _fixture.CreateSampleProviderRequest();

            var createResult = await _fixture.Client.ServiceProvider.CreateAsync(request).ConfigureAwait(false);

            createResult.Should().NotBeNull();
            createResult.Id.Should().NotBeNullOrEmpty();
            createResult.Name.Should().Be(createResult.Name);
            createResult.ExternalId.Should().Be(createResult.ExternalId);
            createResult.BaseUrl.Should().Be(request.BaseUrl);

            await _fixture.Client.ServiceProvider.DeleteAsync(createResult.Id).ConfigureAwait(false);
        }
    }
}
