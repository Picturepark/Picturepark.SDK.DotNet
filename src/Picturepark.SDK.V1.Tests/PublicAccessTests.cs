using System;
using System.Collections.Generic;
using System.Linq;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
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

		[Fact]
		[Trait("Stack", "PublicAccess")]
		public async Task ShouldGetShareByToken()
		{
			var outputFormatIds = new List<string> { "Original", "Preview" };

			var shareContentItems = new List<ShareContent>
			{
				new ShareContent { ContentId = _fixture.GetRandomContentId(string.Empty, 30), OutputFormatIds = outputFormatIds },
				new ShareContent { ContentId = _fixture.GetRandomContentId(string.Empty, 30), OutputFormatIds = outputFormatIds }
			};

			var request = new ShareEmbedCreateRequest
			{
				Contents = shareContentItems,
				Description = "Description of Embed share",
				ExpirationDate = new DateTime(2020, 12, 31),
				Name = "Embed share"
			};

			var createResult = await _client.Shares.CreateAsync(request);

			var embedDetail = await _client.Shares.GetAsync(createResult.ShareId);

			var result = await _client.PublicAccess.GetShareAsync(((ShareDataEmbed)embedDetail.Data).Token);

			Assert.NotNull(result);
		}
	}
}
