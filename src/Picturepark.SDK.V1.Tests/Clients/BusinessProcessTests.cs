using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;
using System.Linq;
#pragma warning disable 1587

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class BusinessProcessTests : IClassFixture<ClientFixture>
    {
        private readonly IPictureparkClient _client;

        public BusinessProcessTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        [Trait("Stack", "BusinessProcesses")]
        public async Task ShouldSearchBusinessProcesses()
        {
            // Arrange
            var request = new BusinessProcessSearchRequest
            {
                Start = 0,
                Limit = 20
            };

            // Act
            var results = await _client.BusinessProcesses.SearchAsync(request).ConfigureAwait(false);

            // Assert
            Assert.NotNull(results);
            Assert.True(results.Results.Any());
        }

        [Fact]
        [Trait("Stack", "BusinessProcesses")]
        public async Task ShouldGetBusinessProcessDetails()
        {
            // Arrange

            // 1. Create or update schema
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(BusinessProcessTest)).ConfigureAwait(false);
            await _client.Schemas.CreateOrUpdateAsync(schemas.First(), false, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            // 2. Create list items
            var listItemDetail1 = await _client.ListItems.CreateAsync(new ListItemCreateRequest
            {
                Content = new BusinessProcessTest { Name = "Test1" },
                ContentSchemaId = nameof(BusinessProcessTest)
            }).ConfigureAwait(false);

            var listItemDetail2 = await _client.ListItems.CreateAsync(new ListItemCreateRequest
            {
                Content = new BusinessProcessTest { Name = "Test2" },
                ContentSchemaId = nameof(BusinessProcessTest)
            }).ConfigureAwait(false);

            // 3. Initialize change request
            var updateRequest = new ListItemFieldsBatchUpdateRequest
            {
                ListItemIds = new List<string>
                {
                    listItemDetail1.Id,
                    listItemDetail2.Id
                },
                ChangeCommands = new List<MetadataValuesChangeCommandBase>
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

            var result = await _client.ListItems.BatchUpdateFieldsByIdsAsync(updateRequest).ConfigureAwait(false);

            // Act
            var details = await _client.BusinessProcesses.GetDetailsAsync(result.BusinessProcessId).ConfigureAwait(false);

            // Assert
            Assert.True(details.LifeCycle == BusinessProcessLifeCycle.Succeeded);
        }
    }
}
