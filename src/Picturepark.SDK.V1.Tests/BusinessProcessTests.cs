using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests
{
	public class BusinessProcessTests : IClassFixture<SDKClientFixture>
	{
		private readonly SDKClientFixture _fixture;
		private readonly PictureparkClient _client;

		public BusinessProcessTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "Contents")]
		public async Task ShouldSearchBusinessProcesses()
		{
			var results = await _client.BusinessProcesses.SearchAsync(new BusinessProcessSearchRequest
			{
				Start = 0,
				Limit = 20,
				Sort = new List<SortInfo>
				{
					new SortInfo
					{
						Field = "audit.creationDate",
						Direction = SortDirection.Desc
					}
				}
			});
			Assert.NotNull(results);
		}
	}
}
