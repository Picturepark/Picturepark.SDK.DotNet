using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;
using Picturepark.SDK.V1.Contract;
using System.Linq;

namespace Picturepark.SDK.V1.Tests.Clients
{
	public class UserTests : IClassFixture<ClientFixture>
	{
		private readonly ClientFixture _fixture;
		private readonly PictureparkClient _client;

		public UserTests(ClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "Contents")]
		public async Task ShouldSearch()
		{
			/// Act
			var searchResult = await _client.Users.SearchAsync(new UserSearchRequest { Limit = 10 });

			/// Assert
			Assert.True(searchResult.Results.Any());
		}

		[Fact]
		[Trait("Stack", "Contents")]
		public async Task ShouldGetUser()
		{
			/// Arrange
			var contentId = await _fixture.GetRandomContentIdAsync(".jpg", 50);
			var content = await _client.Contents.GetAsync(contentId);
			var owner = await _client.Users.GetByOwnerTokenAsync(content.OwnerTokenId);

			/// Act
			var user = await _client.Users.GetAsync(owner.Id);

			/// Assert
			Assert.Equal(owner.Id, user.Id);
		}

		[Fact]
		[Trait("Stack", "Contents")]
		public async Task ShouldGetByOwnerToken()
		{
			/// Arrange
			var contentId = await _fixture.GetRandomContentIdAsync(".jpg", 50);
			var content = await _client.Contents.GetAsync(contentId);

			/// Act
			var owner = await _client.Users.GetByOwnerTokenAsync(content.OwnerTokenId);

			/// Assert
			Assert.NotNull(owner);
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
