using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests
{
	public class ProfileTests : IClassFixture<SDKClientFixture>
	{
		private readonly SDKClientFixture _fixture;
		private readonly PictureparkClient _client;

		public ProfileTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
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
			// Act
			var profile = await _client.Profile.GetAsync();
			var firstName = profile.FirstName;

			profile.FirstName = profile.FirstName + "1";

			var updated = await _client.Profile.UpdateAsync(profile);

			// Assert
			Assert.Equal(firstName, updated.FirstName);
		}
	}
}
