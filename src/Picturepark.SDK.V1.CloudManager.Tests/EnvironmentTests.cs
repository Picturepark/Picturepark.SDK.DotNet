using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.CloudManager.Tests.Fixtures;

namespace Picturepark.SDK.V1.CloudManager.Tests
{
    [Trait("Stack-CloudManager", "Environment")]
    public class EnvironmentTests : IClassFixture<ClientFixture>
    {
        private const string EnvironmentConfiguration = "environmentconfiguration";
        private readonly ClientFixture _fixture;

        public EnvironmentTests(ClientFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldTryToGetVersion()
        {
            // make sure environment is in place
            var environmentConfiguration = await _fixture.Client.Environment.CreateAsync();

            // get version
            var result = await _fixture.Client.Environment.GetVersionAsync();
            result.Should().NotBeNull();
            result.ContractVersion.Should().Be(environmentConfiguration.ContractVersion);
        }

        [Fact]
        public async Task ShouldTryToCreateEnvironment()
        {
            var result = await _fixture.Client.Environment.CreateAsync();
            result.Should().NotBeNull();
            result.Id.Should().Be(EnvironmentConfiguration);
        }
    }
}
