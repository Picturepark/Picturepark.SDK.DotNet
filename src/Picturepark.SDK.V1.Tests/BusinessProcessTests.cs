using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests
{
	public class BusinessProcessTests : IClassFixture<BusinessProcessFixture>
	{
		private readonly PictureparkClient _client;

		public BusinessProcessTests(BusinessProcessFixture fixture)
		{
			_client = fixture.Client;
		}

		[Fact]
		[Trait("Stack", "BusinessProcesses")]
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

		[Fact(Skip = "TODO: Finalize")]
		[Trait("Stack", "BusinessProcesses")]
		public async Task ShouldGetBusinessProcessDetails()
		{
			var listItemDetail1 = await _client.ListItems.CreateAsync(new ListItemCreateRequest { Content = new BusinessProcessTest { Name = "Test1" }, ContentSchemaId = nameof(BusinessProcessTest) });
			var listItemDetail2 = await _client.ListItems.CreateAsync(new ListItemCreateRequest { Content = new BusinessProcessTest { Name = "Test2" }, ContentSchemaId = nameof(BusinessProcessTest) });

			var updateRequest = new ListItemFieldsUpdateRequest
			{
				ListItemIds = new List<string> { listItemDetail1.Id, listItemDetail2.Id },
				ChangeCommands = new List<MetadataValuesSchemaUpdateCommand>
				{
					new MetadataValuesSchemaUpdateCommand
					{
						SchemaId = nameof(BusinessProcessTest),
						Value = new DataDictionary
						{
							{ "Description", "TestDescription" }
						}
					}
				}
			};
			var businessProcess = await _client.ListItems.UpdateFieldsAsync(updateRequest);

			var waitResult = await _client.BusinessProcesses.WaitForStatesAsync(businessProcess.Id, "Completed", 10 * 1000);

			Assert.True(waitResult.HasStateHit);

			var details = await _client.BusinessProcesses.GetDetailsAsync(businessProcess.Id);
		}
	}
}
