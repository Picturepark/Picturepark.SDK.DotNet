using Picturepark.SDK.V1.Contract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Contract.Extensions;
using Picturepark.SDK.V1.Tests.Fixtures;
using System;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class DocumentHistoryTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly PictureparkClient _client;

        public DocumentHistoryTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldSearch()
        {
            /// Arrange
            var request = new DocumentHistorySearchRequest
            {
                Start = 0,
                Limit = 10,
            };

            /// Act
            var result = await _client.DocumentHistory.SearchAsync(request);

            /// Assert
            Assert.True(result.Results.Any());
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldGet()
        {
            /// Arrange
            var documentId = await _fixture.GetRandomContentIdAsync(".jpg", 20);

            /// Act
            var result = await _client.DocumentHistory.GetAsync(documentId);

            /// Assert
            Assert.True(result.DocumentId == documentId);
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldGetVersion()
        {
            /// Arrange
            string documentId = await _fixture.GetRandomContentIdAsync(".jpg", 20);
            string versionId = "1";

            /// Act
            var result = await _client.DocumentHistory.GetVersionAsync(documentId, versionId);

            /// Assert
            Assert.Equal(versionId, result.DocumentVersion.ToString());
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldGetDifferenceOfContentChange()
        {
            /// Arrange
            string location = "testlocation" + new Random().Next(0, 999999);
            string contentId = await _fixture.GetRandomContentIdAsync(".jpg", 20);
            var content = await _client.Contents.GetAsync(contentId);
            var history = await _client.DocumentHistory.GetAsync(contentId);

            var updateRequest = new ContentFieldsUpdateRequest
            {
                ContentIds = new List<string> { content.Id },
                ChangeCommands = new List<MetadataValuesChangeCommandBase>
                {
                    new MetadataValuesSchemaUpsertCommand
                    {
                        SchemaId = "Drive",
                        Value = new DataDictionary
                        {
                            { "Location", location }
                        }
                    }
                }
            };

            // TODO: Create ContentHelper to update and wait with one call => UpdateMetadataManyAndWaitForCompletionAsync?
            var businessProcess = await _client.Contents.BatchUpdateFieldsByIdsAsync(updateRequest);
            var waitResult = await _client.BusinessProcesses.WaitForCompletionAsync(businessProcess.Id);

            // Refetch content and compare versions
            var updatedHistory = await _client.DocumentHistory.GetAsync(contentId);

            /// Act
            var difference = await _client.DocumentHistory.GetDifferenceAsync(contentId, history.DocumentVersion, updatedHistory.DocumentVersion);

            /// Assert
            Assert.True(waitResult.HasLifeCycleHit);
            Assert.NotEqual(0, updatedHistory.DocumentVersion);
            Assert.Contains(@"""location"": """ + location + @"""", difference.NewValues.ToString());
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldGetDifferenceWithLatestVersion()
        {
            /// Arrange
            string documentId = await _fixture.GetRandomContentIdAsync(".jpg", 20);
            long oldVersionId = 1;

            /// Act
            var difference = await _client.DocumentHistory.GetDifferenceLatestAsync(documentId, oldVersionId);

            /// Assert
            Assert.True(difference.OldDocumentVersion <= difference.NewDocumentVersion);
        }
    }
}
