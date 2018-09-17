using Picturepark.SDK.V1.Contract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;
using System;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class DocumentHistoryTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public DocumentHistoryTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldSearch()
        {
            // Arrange
            var request = new DocumentHistorySearchRequest
            {
                Limit = 10,
            };

            // Act
            var result = await _client.DocumentHistory.SearchAsync(request).ConfigureAwait(false);

            // Assert
            Assert.True(result.Results.Any());
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldGet()
        {
            // Arrange
            var documentId = await _fixture.GetRandomContentIdAsync(".jpg", 20).ConfigureAwait(false);

            // Act
            var result = await _client.DocumentHistory.GetAsync(documentId).ConfigureAwait(false);

            // Assert
            Assert.True(result.DocumentId == documentId);
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldGetVersion()
        {
            // Arrange
            string documentId = await _fixture.GetRandomContentIdAsync(".jpg", 20).ConfigureAwait(false);
            string versionId = "1";

            // Act
            var result = await _client.DocumentHistory.GetVersionAsync(documentId, versionId).ConfigureAwait(false);

            // Assert
            Assert.Equal(versionId, result.DocumentVersion.ToString());
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldGetDifferenceOfContentChange()
        {
            // Arrange
            string location = "testlocation" + new Random().Next(0, 999999);
            string contentId = await _fixture.GetRandomContentIdAsync(".jpg", 20).ConfigureAwait(false);

            var schema = await CreateTestSchemaAsync().ConfigureAwait(false);
            var content = await _client.Content.GetAsync(contentId).ConfigureAwait(false);
            var history = await _client.DocumentHistory.GetAsync(contentId).ConfigureAwait(false);

            var updateRequest = new ContentFieldsBatchUpdateRequest
            {
                ContentIds = new List<string> { content.Id },
                ChangeCommands = new List<MetadataValuesChangeCommandBase>
                {
                    new MetadataValuesSchemaUpsertCommand
                    {
                        SchemaId = schema.Id,
                        Value = new DataDictionary
                        {
                            { "name", location }
                        }
                    }
                }
            };

            var result = await _client.Content.BatchUpdateFieldsByIdsAsync(updateRequest).ConfigureAwait(false);

            // Refetch content and compare versions
            var updatedHistory = await _client.DocumentHistory.GetAsync(contentId).ConfigureAwait(false);

            // Act
            var difference = await _client.DocumentHistory.GetDifferenceAsync(contentId, history.DocumentVersion, updatedHistory.DocumentVersion).ConfigureAwait(false);

            // Assert
            Assert.True(result.LifeCycle == BusinessProcessLifeCycle.Succeeded);
            Assert.NotEqual(0, updatedHistory.DocumentVersion);
            Assert.Contains(@"""name"": """ + location + @"""", difference.NewValues.ToString());
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldGetDifferenceWithLatestVersion()
        {
            // Arrange
            string documentId = await _fixture.GetRandomContentIdAsync(".jpg", 20).ConfigureAwait(false);
            long oldVersionId = 1;

            // Act
            var difference = await _client.DocumentHistory.GetDifferenceLatestAsync(documentId, oldVersionId).ConfigureAwait(false);

            // Assert
            Assert.True(difference.OldDocumentVersion <= difference.NewDocumentVersion);
        }

        private async Task<SchemaDetail> CreateTestSchemaAsync()
        {
            var schemaId = "Schema" + new Random().Next(0, 999999);
            var config = await _client.Info.GetInfoAsync().ConfigureAwait(false);
            var schemaItem = new SchemaDetail
            {
                Id = schemaId,
                ReferencedInContentSchemaIds = new List<string>
                {
                    "ImageMetadata"
                },
                Fields = new List<FieldBase>
                {
                    new FieldString
                    {
                        Id = "name",
                        Names = new TranslatedStringDictionary { { config.LanguageConfiguration.DefaultLanguage, "Name" } },
                    }
                },
                FieldsOverwrite = new List<FieldOverwriteBase>(),
                Names = new TranslatedStringDictionary { { config.LanguageConfiguration.DefaultLanguage, schemaId } },
                Descriptions = new TranslatedStringDictionary(),
                Types = new List<SchemaType>
                {
                    SchemaType.Layer
                },
                DisplayPatterns = new List<DisplayPattern>()
            };

            var result = await _client.Schema.CreateAsync(schemaItem, false, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            return result.Schema;
        }
    }
}
