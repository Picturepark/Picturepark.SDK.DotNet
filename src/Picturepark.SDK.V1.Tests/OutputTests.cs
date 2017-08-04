using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests
{
	public class OutputTests : IClassFixture<SDKClientFixture>
	{
		private SDKClientFixture _fixture;
		private PictureparkClient _client;

		public OutputTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "Outputs")]
		public async Task ShouldGet()
		{
			string contentId = _fixture.GetRandomContentId("*.jpg", 20);
			Assert.False(string.IsNullOrEmpty(contentId));

			ContentDetail contentDetail = await _client.Contents.GetAsync(contentId);
			Assert.True(contentId == contentDetail.Id, "Delivery goes wrong. We never ordered such pizza.");

			Assert.True(contentDetail.Outputs.Any());
			var outputId = contentDetail.Outputs.FirstOrDefault().Id;
			Assert.False(string.IsNullOrEmpty(outputId));

			OutputDetail result = await _client.Outputs.GetAsync(outputId);
			Assert.True(result.ContentId == contentId );
		}
	}
}
