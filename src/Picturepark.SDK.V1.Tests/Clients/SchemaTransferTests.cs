using System;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "SchemaTransfer")]
    public class SchemaTransferTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public SchemaTransferTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        public async Task ShouldThrowFileTransferNotFoundException()
        {
            // Assert
            await Assert.ThrowsAsync<FileTransferNotFoundException>(async () =>
            {
                // Act
                var request = new SchemaImportRequest()
                {
                    FileTransferId = Guid.NewGuid().ToString(),
                    AllowMissingDependencies = true,
                    ImportListItems = true
                };

                await _client.SchemaTransfer.ImportAsync(request);
            });
        }

        [Fact]
        public async Task ShouldImportPlanetSchema()
        {
            // Arrange
            const string schemaId = "Planet";

            var version = await _client.Info.GetVersionAsync();

            var transferName = new Random().Next(1000, 9999).ToString();

            var tempPlanetFile = Path.GetTempFileName();

            var planetJson = File.ReadAllText(Path.Combine(_fixture.ExampleSchemaBasePath, "Planet.json"))
                .Replace("{{contractVersion}}", version.ContractVersion);

            File.WriteAllText(tempPlanetFile, planetJson);

            var files = new FileLocations[]
            {
                tempPlanetFile
            };

            var createTransferResult = await _client.Transfer.UploadFilesAsync(transferName, files, new UploadOptions());

            // get the only uploaded file
            var fileTransfers = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id);

            var request = new SchemaImportRequest()
            {
                FileTransferId = fileTransfers.First().Id,
                AllowMissingDependencies = true,
                ImportListItems = false
            };

            // Act
            // import schema
            var transfer = await _client.SchemaTransfer.ImportAsync(request);

            // wait for completion
            await _client.BusinessProcess.WaitForCompletionAsync(transfer.BusinessProcessId, new TimeSpan(0, 2, 0));

            var schema = await _client.Schema.GetAsync(schemaId);

            // Assert
            Assert.Equal(schemaId, schema.Id);

            // Tear down
            await _client.Schema.DeleteAsync(schema.Id);
        }

        [Fact]
        public async Task ShouldImportPlanetSchemaUsingJsonUpload()
        {
            // Arrange
            const string schemaId = "Planet";
            var version = await _client.Info.GetVersionAsync();

            var planetJson = (await File.ReadAllTextAsync(Path.Combine(_fixture.ExampleSchemaBasePath, "Planet.json")))
                .Replace("{{contractVersion}}", version.ContractVersion);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(planetJson));

            // Act
            var businessProcess = await _client.SchemaTransfer.ImportJsonAsync(stream, allowMissingDependencies: true, importListItems: true);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id, TimeSpan.FromMinutes(2));

            // Assert
            var schema = await _client.Schema.GetAsync(schemaId);
            schema.Id.Should().Be(schemaId);

            // Tear down
            var listItemDeletionBusinessProcess = await _client.ListItem.DeleteManyByFilterAsync(
                new ListItemDeleteManyFilterRequest
                {
                    FilterRequest = new ListItemFilterRequest
                    {
                        SchemaIds = new[] { schemaId }
                    }
                });

            await _client.BusinessProcess.WaitForCompletionAsync(listItemDeletionBusinessProcess.Id, TimeSpan.FromMinutes(2));
            await _client.Schema.DeleteAsync(schemaId);
        }
    }
}
