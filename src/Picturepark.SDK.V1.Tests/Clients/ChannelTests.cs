using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class ChannelTests : IClassFixture<ClientFixture>
    {
        private readonly IPictureparkService _client;

        public ChannelTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Channels")]
        public async Task ShouldGetChannels()
        {
            // Act
            var channels = await _client.Channel.GetChannelsAsync().ConfigureAwait(false);

            // Assert
            Assert.True(channels.Count > 0);
        }
    }
}
