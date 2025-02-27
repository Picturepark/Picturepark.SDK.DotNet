﻿using System;
using System.Collections.Generic;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "Metadata")]
    public class MetadataTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public MetadataTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        [Fact]
        public async Task ShouldGetStatus()
        {
            // Act
            var metadataStatus = await _client.Metadata.GetStatusAsync();

            // Assert
            metadataStatus.MainDocuments.ContentOrLayerSchemaIds.Should().NotBeNull();
            metadataStatus.MainDocuments.ListSchemaIds.Should().NotBeNull();
            metadataStatus.FieldIdsToCleanup.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldUpdateOutdatedMetadataWhenStatusIsOutdated()
        {
            // Arrange: create content with predefined schema
            var schemaId = $"Schema{Guid.NewGuid():N}";
            var uniqueName = $"Name_{Guid.NewGuid():N}";
            var (schemaCreateResult, content) = await CreateSchemaAndContent(schemaId, uniqueName);

            content.ContentAs<JObject>()["name"].Value<string>().Should().Be(uniqueName);

            // Remove schema fields in order to turn the metadata status to red
            schemaCreateResult.Schema.Fields.Clear();
            await _client.Schema.UpdateAsync(schemaCreateResult.Schema, false);

            var metadataStatus = await _client.Metadata.GetStatusAsync();
            metadataStatus.State.Should().Be(MetadataState.Outdated);
            metadataStatus.MainDocuments.ContentOrLayerSchemaIds.Should().Contain(schemaId);
            metadataStatus.ContentOrLayerSchemaIds.Should().Contain(schemaId);
            metadataStatus.FieldIdsToCleanup.Should().ContainKey(schemaId.ToLowerCamelCase());
            metadataStatus.FieldIdsToCleanup[schemaId.ToLowerCamelCase()].Should().Contain("name");

            // Update outdated metadata and await for its completion
            var businessProcess = await _client.Metadata.UpdateOutdatedAsync();
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            // Assert
            content = await _client.Content.GetAsync(content.Id, new[] { ContentResolveBehavior.Content });
            content.ContentAs<JObject>().Should().NotContainKey("name");

            metadataStatus = await _client.Metadata.GetStatusAsync();
            metadataStatus.State.Should().Be(MetadataState.UpToDate);
            metadataStatus.MainDocuments.ContentOrLayerSchemaIds.Should().NotContain(schemaId);
            metadataStatus.ContentOrLayerSchemaIds.Should().NotContain(schemaId);
            metadataStatus.FieldIdsToCleanup.Should().NotContainKey(schemaId.ToLower());
        }

        [Fact]
        public async Task ShouldBePossibleToReuseRemovedFieldIdOfASchemaAfterUpdatingOutdatedMetadata()
        {
            // Arrange: create content with predefined schema
            var schemaId = $"Schema{Guid.NewGuid():N}";
            var uniqueName = $"Name_{Guid.NewGuid():N}";

            var (schemaCreateResult, content) = await CreateSchemaAndContent(schemaId, uniqueName);

            // Remove schema fields in order to turn the metadata status to red
            schemaCreateResult.Schema.Fields.Clear();
            var schemaUpdateResult = await _client.Schema.UpdateAsync(schemaCreateResult.Schema, false);

            // Update outdated metadata and await for its completion
            var businessProcess = await _client.Metadata.UpdateOutdatedAsync();
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            // Act: update schema adding field with previous id but different type
            schemaUpdateResult.Schema.Fields.Add(new FieldTranslatedString { Id = "name", Names = new TranslatedStringDictionary { { _fixture.DefaultLanguage, "Name Translated" } } });
            schemaUpdateResult = await _client.Schema.UpdateAsync(schemaUpdateResult.Schema, false);

            // Assert
            schemaUpdateResult.Schema.Fields.Should().Contain(f => f.Id == "name");
        }

        private async Task<(SchemaCreateResult, ContentDetail)> CreateSchemaAndContent(string schemaId, string contentUniqueName)
        {
            var schemaResult = await _client.Schema.CreateAsync(
                    new SchemaCreateRequest
                    {
                        Types = new List<SchemaType> { SchemaType.Content },
                        ViewForAll = true,
                        Id = schemaId,
                        Fields = new List<FieldBase> { new FieldString { Id = "name", Names = new TranslatedStringDictionary { { _fixture.DefaultLanguage, "Name" } } } },
                        Names = new TranslatedStringDictionary { { _fixture.DefaultLanguage, schemaId } }
                    })
                ;

            var request = new ContentCreateRequest
            {
                Content = new { Name = contentUniqueName },
                ContentSchemaId = schemaId
            };

            var content = await _client.Content.CreateAsync(request, new[] { ContentResolveBehavior.Content });

            return (schemaResult, content);
        }
    }
}
