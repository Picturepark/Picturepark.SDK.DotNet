using Picturepark.SDK.V1.Tests.Fixtures;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class InfoTests : IClassFixture<ClientFixture>
    {
        private readonly IPictureparkService _client;

        public InfoTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Info")]
        public async Task ShouldGetVersion()
        {
            // Act
            var version = await _client.Info.GetVersionAsync();

            // Assert
            Assert.NotNull(version.ContractVersion);
            Assert.NotNull(version.FileProductVersion);
            Assert.NotNull(version.FileVersion);
            Assert.NotNull(version.Release);
        }

        [Fact]
        [Trait("Stack", "Info")]
        public async Task ShouldGetStatus()
        {
            // Act
            var status = await _client.Info.GetStatusAsync();

            // Assert
            status.DisplayValuesStatus.Should().NotBeNull();
            status.SearchIndicesStatus.Should().NotBeNull();
        }
    }
}