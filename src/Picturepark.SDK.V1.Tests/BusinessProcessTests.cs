using System.Collections.Generic;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;
using System.Linq;

namespace Picturepark.SDK.V1.Tests
{
	public class BusinessProcessTests : IClassFixture<SDKClientFixture>
	{
		private readonly PictureparkClient _client;

		public BusinessProcessTests(SDKClientFixture fixture)
		{
			_client = fixture.Client;
		}

		[Fact]
		[Trait("Stack", "BusinessProcesses")]
		public async Task ShouldSearchBusinessProcesses()
		{
			/// Arrange
			var request = new BusinessProcessSearchRequest
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
			};

			/// Act
			var results = await _client.BusinessProcesses.SearchAsync(request);

			/// Assert
			Assert.NotNull(results);
			Assert.True(results.Results.Any());
		}

		[Fact]
		[Trait("Stack", "BusinessProcesses")]
		public async Task ShouldGetBusinessProcessDetails()
		{
			/// Arrange

			// 1. Create or update schema
			var schemas = _client.Schemas.GenerateSchemaFromPOCO(typeof(BusinessProcessTest));
			await _client.Schemas.CreateOrUpdateAsync(schemas.First(), false);

			// 2. Create list items
			var listItemDetail1 = await _client.ListItems.CreateAsync(new ListItemCreateRequest
			{
				Content = new BusinessProcessTest { Name = "Test1" },
				ContentSchemaId = nameof(BusinessProcessTest)
			});

			var listItemDetail2 = await _client.ListItems.CreateAsync(new ListItemCreateRequest
			{
				Content = new BusinessProcessTest { Name = "Test2" },
				ContentSchemaId = nameof(BusinessProcessTest)
			});

			// 3. Initialize change request
			var updateRequest = new ListItemFieldsUpdateRequest
			{
				ListItemIds = new List<string>
				{
					listItemDetail1.Id,
					listItemDetail2.Id
				},
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

			/// Act
			var businessProcess = await _client.ListItems.UpdateFieldsAsync(updateRequest);
			var waitResult = await _client.BusinessProcesses.WaitForStatesAsync(businessProcess.Id, "Completed", 10 * 1000);
			var details = await _client.BusinessProcesses.GetDetailsAsync(businessProcess.Id);

			/// Assert
			Assert.True(waitResult.HasStateHit);
		}
	}
}
