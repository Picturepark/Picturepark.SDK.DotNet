﻿using Picturepark.SDK.V1.Contract;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;
using System;
using FluentAssertions;

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
            var result = await _client.DocumentHistory.SearchAsync(request);

            // Assert
            result.Results.Should().NotBeEmpty().And.OnlyHaveUniqueItems(d => $"{d.DocumentType}_{d.DocumentId}_{d.DocumentVersion}");
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldGetCurrent()
        {
            // Arrange
            var documentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);

            // Act
            var result = await _client.DocumentHistory.GetCurrentAsync(typeof(Content).Name, documentId);

            // Assert
            Assert.True(result.DocumentId == documentId);
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldGetVersion()
        {
            // Arrange
            string documentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            long versionId = 1;

            // Act
            var result = await _client.DocumentHistory.GetVersionAsync(typeof(Content).Name, documentId, versionId);

            // Assert
            Assert.Equal(versionId, result.DocumentVersion);
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldCompareWithVersion()
        {
            // Arrange
            string location = "testlocation" + new Random().Next(0, 999999);
            string contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);

            var schema = await CreateTestSchemaAsync();
            var content = await _client.Content.GetAsync(contentId);
            var history = await _client.DocumentHistory.GetCurrentAsync(typeof(Content).Name, contentId);

            var updateRequest = new ContentFieldsBatchUpdateRequest
            {
                ContentIds = new List<string> { content.Id },
                ChangeCommands = new List<MetadataValuesChangeCommandBase>
                {
                    new MetadataValuesSchemaUpsertCommand
                    {
                        SchemaId = schema.Id,
                        Value = new
                        {
                            name = location
                        }
                    }
                }
            };

            var result = await _client.Content.BatchUpdateFieldsByIdsAsync(updateRequest);

            // Refetch content and compare versions
            var updatedHistory = await _client.DocumentHistory.GetCurrentAsync(typeof(Content).Name, contentId);

            // Act
            var difference = await _client.DocumentHistory.CompareWithVersionAsync(typeof(Content).Name, contentId, updatedHistory.DocumentVersion, history.DocumentVersion);

            // Assert
            Assert.True(result.LifeCycle == BusinessProcessLifeCycle.Succeeded);
            Assert.NotEqual(0, updatedHistory.DocumentVersion);
            Assert.Contains(@"""name"": """ + location + @"""", difference.Patch.ToString());
        }

        [Fact]
        [Trait("Stack", "DocumentHistory")]
        public async Task ShouldCompareWithCurrent()
        {
            // Arrange
            string documentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            long oldVersionId = 1;

            // Act
            var difference = await _client.DocumentHistory.CompareWithCurrentAsync(typeof(Content).Name, documentId, oldVersionId);

            // Assert
            Assert.True(difference.OldDocumentVersion <= difference.NewDocumentVersion);
        }

        private async Task<SchemaDetail> CreateTestSchemaAsync()
        {
            var schemaId = "Schema" + new Random().Next(0, 999999);
            var config = await _client.Info.GetInfoAsync();
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

            var result = await _client.Schema.CreateAsync(schemaItem, false, TimeSpan.FromMinutes(1));
            return result.Schema;
        }
    }
}
