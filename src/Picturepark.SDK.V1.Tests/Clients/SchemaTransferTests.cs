using System;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class SchemaTransferTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly PictureparkClient _client;

        public SchemaTransferTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "SchemaTransfer")]
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

                await _client.SchemaTransfer.ImportAsync(request).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "SchemaTransfer")]
        public async Task ShouldImportPlanetSchema()
        {
            // Arrange
            const string schemaId = "Planet";

            var transferName = new Random().Next(1000, 9999).ToString();

            var files = new FileLocations[]
            {
                Path.Combine(_fixture.ExampleSchemaBasePath, "Planet.json")
            };

            var createTransferResult = await _client.Transfers.UploadFilesAsync(transferName, files, new UploadOptions()).ConfigureAwait(false);

            // get the only uploaded file
            var fileTransfers = await _client.Transfers.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id).ConfigureAwait(false);

            var request = new SchemaImportRequest()
            {
                FileTransferId = fileTransfers.Results.First().Id,
                AllowMissingDependencies = true,
                ImportListItems = false
            };

            // Act
            // import schema
            var transfer = await _client.SchemaTransfer.ImportAsync(request).ConfigureAwait(false);

            // wait for completion
            await _client.BusinessProcesses.WaitForCompletionAsync(transfer.BusinessProcessId).ConfigureAwait(false);

            var schema = await _client.Schemas.GetAsync(schemaId).ConfigureAwait(false);

            // Assert
            Assert.Equal(schema.Id, schemaId);

            /// Tear down
            await _client.Schemas.DeleteAsync(schema.Id).ConfigureAwait(false);
        }
    }
}