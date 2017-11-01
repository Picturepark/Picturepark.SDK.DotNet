using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests
{
	public class UserTests : IClassFixture<SDKClientFixture>
	{
		private readonly SDKClientFixture _fixture;
		private readonly PictureparkClient _client;

		public UserTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "Users")]
		public async Task ShouldGetChannels()
		{
            // Act
			var channels = await _client.Users.GetChannelsAsync();

            // Assert
			Assert.True(channels.Count > 0);
		}
	}
}
