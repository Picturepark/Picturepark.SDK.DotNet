using FluentAssertions;
using System.Threading.Tasks;
using Picturepark.SDK.V1.CloudManager.Contract;
using Xunit;
using Picturepark.SDK.V1.CloudManager.Tests.Fixtures;

namespace Picturepark.SDK.V1.CloudManager.Tests
{
    [Trait("Stack-CloudManager", "ServiceProvider")]
    public class CustomerServiceProviderTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;

        public CustomerServiceProviderTests(ClientFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldCreateAndDeleteCustomerServiceProviderConfiguration()
        {
            // create provider
            var provider = await _fixture.Client.ServiceProvider.CreateAsync(_fixture.CreateSampleProviderRequest()).ConfigureAwait(false);

            CustomerServiceProviderCreateRequest request = new CustomerServiceProviderCreateRequest()
            {
                CustomerAlias = _fixture.Configuration.CustomerAlias,
                ServiceProvider = new CustomerServiceProvider()
                {
                    Id = provider.Id
                }
            };

            // create customer/service provider
            var result = await _fixture.Client.CustomerServiceProvider.CreateAsync(request).ConfigureAwait(false);
            result.Should().NotBeNull();
            result.Id.Should().Be(request.ServiceProvider.Id);

            // delete customer/service provider configuration
            await _fixture.Client.CustomerServiceProvider.DeleteAsync(request.CustomerAlias, provider.Id).ConfigureAwait(false);

            // delete provider
            await _fixture.Client.ServiceProvider.DeleteAsync(provider.Id).ConfigureAwait(false);
        }
    }
}
