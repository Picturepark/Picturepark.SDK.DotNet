using Picturepark.SDK.V1.Tests.Fixtures;
using System.Threading.Tasks;
using Xunit;

namespace Picturepark.SDK.V1.Tests
{
	public class PublicAccessTests : IClassFixture<SDKClientFixture>
	{
		private readonly SDKClientFixture _fixture;
		private readonly PictureparkClient _client;

		public PublicAccessTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = fixture.Client;
		}

		[Fact]
		[Trait("Stack", "PublicAccess")]
		public async Task ShouldGetVersion()
		{
			var version = await _client.PublicAccess.GetVersionAsync();

			Assert.NotNull(version.ContractVersion);
			Assert.NotNull(version.FileProductVersion);
			Assert.NotNull(version.FileVersion);
			Assert.NotNull(version.Release);
		}
	}
}
