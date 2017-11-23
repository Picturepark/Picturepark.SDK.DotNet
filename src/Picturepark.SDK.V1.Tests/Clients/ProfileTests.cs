using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
	public class ProfileTests : IClassFixture<ClientFixture>
	{
		private readonly PictureparkClient _client;

		public ProfileTests(ClientFixture fixture)
		{
			_client = fixture.Client;
		}

		[Fact]
		[Trait("Stack", "Profile")]
		public async Task ShouldGetProfile()
		{
			// Act
			var profile = await _client.Profile.GetAsync();

			// Assert
			Assert.NotEmpty(profile.Id);
		}

		[Fact]
		[Trait("Stack", "Profile")]
		public async Task ShouldUpdateProfile()
		{
			// Arrange
			var profile = await _client.Profile.GetAsync();

			// Act
			profile.FirstName = profile.FirstName + "1";
			var updated = await _client.Profile.UpdateAsync(profile);

			// Assert
			Assert.Equal(profile.FirstName, updated.FirstName);
		}
	}
}
