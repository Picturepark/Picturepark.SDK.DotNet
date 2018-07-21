using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class SchemaTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly PictureparkClient _client;

        public SchemaTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateAllTypesSchemaFromClass()
        {
            // Act
            var allTypes = await _client.Schemas.GenerateSchemasAsync(typeof(AllDataTypesContract)).ConfigureAwait(false);
            foreach (var schema in allTypes)
            {
                await _client.Schemas.CreateOrUpdateAsync(schema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            }
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCorrectlyDeserializeExceptions()
        {
            // Arrange
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(Person)).ConfigureAwait(false);

            // Act & Assert
            await Assert.ThrowsAsync<SchemaValidationException>(async () =>
            {
                foreach (var schema in schemas)
                {
                    schema.Id = "000";
                    await _client.Schemas.CreateOrUpdateAsync(schema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false); // throws exception
                }
            }).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateSchemas()
        {
            // Act
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(Person));

            // Assert
            Assert.Equal(8, schemas.Count);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateAndCreateSchemas()
        {
            // Act
            var schemaSuffix = new Random().Next(0, 999999);
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(PersonShot)).ConfigureAwait(false);
            IList<SchemaDetail> createdSchemas = new List<SchemaDetail>();
            foreach (var schema in schemas)
            {
                AppendSchemaIdSuffix(schema, schemaSuffix);
                var result = await _client.Schemas.CreateAsync(schema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                createdSchemas.Add(result.Schema);
            }

            // Assert
            createdSchemas.Should().HaveSameCount(schemas);
            createdSchemas.Select(s => s.Id).Should().BeEquivalentTo(schemas.Select(s => s.Id));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateAndCreateAndUpdateSchemas()
        {
            // Act
            var schemaSuffix = new Random().Next(0, 999999);
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(Person)).ConfigureAwait(false);

            IList<SchemaDetail> createdSchemas = new List<SchemaDetail>();
            foreach (var schema in schemas)
            {
                AppendSchemaIdSuffix(schema, schemaSuffix);
                var result = await _client.Schemas.CreateAsync(schema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                createdSchemas.Add(result.Schema);
            }

            // Add a new text field to the first created schema
            var createdSchema = createdSchemas.First();
            var fieldName = "newField" + createdSchema.Id;
            createdSchema.Fields.Add(new FieldString
            {
                Names = new TranslatedStringDictionary { { _fixture.DefaultLanguage, fieldName } },
                Id = fieldName
            });
            var updateResult = await _client.Schemas.UpdateAsync(createdSchema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            // Assert
            updateResult.Schema.Fields.Should().Contain(f => f.Id == $"newField{updateResult.Schema.Id}");
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldDelete()
        {
            // Arrange
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(Tag)).ConfigureAwait(false);
            Assert.Equal(1, schemas.Count);

            // modify schema ID before submit
            var tagSchema = schemas.First();
            tagSchema.Id = "SchemaToDelete" + new Random().Next(0, 999999);

            var result = await _client.Schemas.CreateAsync(tagSchema, false, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            // Act
            await _client.Schemas.DeleteAsync(result.Schema.Id, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            // Assert
            await Assert.ThrowsAsync<SchemaNotFoundException>(async () => await _client.Schemas.GetAsync(tagSchema.Id).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldExist()
        {
            string schemaId = await _fixture.GetRandomSchemaIdAsync(20);

            // Act
            bool schemaExists = await _client.Schemas.ExistsAsync(schemaId);

            // Assert
            Assert.True(schemaExists);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldNotExist()
        {
            // Act
            var schemaExists = await _client.Schemas.ExistsAsync("abcabcabcabc");

            // Assert
            Assert.False(schemaExists);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGet()
        {
            // Arrange
            var searchRequest = new SchemaSearchRequest { Start = 0, Limit = 2 };
            var searchResult = await _client.Schemas.SearchAsync(searchRequest);

            Assert.True(searchResult.Results.Any());

            List<string> schemaIds = searchResult.Results
                .Select(i => i.Id)
                .OrderBy(i => i)
                .ToList();

            // Act
            var schemaIdsOk = new List<string>();
            var schemaIdsNotOk = new List<string>();
            foreach (var schemaId in schemaIds)
            {
                try
                {
                    var schema = await _client.Schemas.GetAsync(schemaId);
                    schemaIdsOk.Add(schema.Id);
                }
                catch
                {
                    schemaIdsNotOk.Add(schemaId);
                }
            }

            // Assert
            Assert.False(schemaIdsNotOk.Count > 0);
            Assert.True(schemaIdsOk.Count > 0);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetJsonValidationSchema()
        {
            // Arrange
            var schemaId = await _fixture.GetRandomSchemaIdAsync(20);

            // Act
            var result = await _client.JsonSchemas.GetAsync(schemaId);

            // Assert
            Assert.NotNull(result.Property("definitions"));
            Assert.NotNull(result.ToString(Formatting.Indented));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateSchemaAndValidateFilter()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<SoccerPlayer>(_client);
            var expectedFilterString = "{\"kind\":\"TermFilter\",\"field\":\"contentType\",\"term\":\"FC Aarau\"}";

            // Act
            var generatedSoccerPlayerSchema = await _client.Schemas.GetAsync("SoccerPlayer");
            var jsonConvertedField = generatedSoccerPlayerSchema.Fields.Single(i => i.Id == "club");

            // Assert
            Assert.Contains(expectedFilterString, JsonConvert.SerializeObject(jsonConvertedField));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateSchemaAndValidateMultiline()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client);
            string expectedMultilineString = "\"multiLine\":true";

            // Act
            var generatedSoccerPlayerSchema = await _client.Schemas.GetAsync("Person");
            var jsonConvertedField = generatedSoccerPlayerSchema.Fields.ToList()[0];

            // Assert
            Assert.Contains(expectedMultilineString, JsonConvert.SerializeObject(jsonConvertedField));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateSchemaAndValidateMaxRecursion()
        {
            // Act
            var schema = await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client);

            // Assert
            Assert.Contains(schema.Types, i => i == SchemaType.List || i == SchemaType.Struct);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldSearch()
        {
            // Act
            var searchRequest = new SchemaSearchRequest
            {
                Limit = 12,
                SearchString = "D*"
            };

            var result = await _client.Schemas.SearchAsync(searchRequest);

            // Assert
            Assert.True(result.Results.Any());
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldUpdate()
        {
            // Arrange
            var schemaTranslation = "SchemaTranslation" + new Random().Next(0, 999999);
            var language = "de";

            // Create simple schema
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(Tag)).ConfigureAwait(false);
            var tagSchema = schemas.First();
            tagSchema.Id = "SchemaToUpdate" + new Random().Next(0, 999999);

            var createResult = await _client.Schemas.CreateAsync(tagSchema, false, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            var schemaDetail = createResult.Schema;
            schemaDetail.Names[language] = schemaTranslation;

            // Act
            var updateResult = await _client.Schemas.UpdateAsync(schemaDetail, false, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            // Assert
            updateResult.Schema.Names.TryGetValue(language, out string outString);

            Assert.Equal(schemaTranslation, outString);
        }

        private void AppendSchemaIdSuffix(SchemaDetail schema, int schemaSuffix)
        {
            var systemSchemaIds = new[] { "Country" };
            if (!systemSchemaIds.Contains(schema.Id))
            {
                schema.Id = schema.Id + schemaSuffix;

                foreach (var key in schema.Names.Keys.ToList())
                {
                    schema.Names[key] = schema.Names[key] + " " + schemaSuffix;
                }
            }

            if (!string.IsNullOrEmpty(schema.ParentSchemaId) && !systemSchemaIds.Contains(schema.ParentSchemaId))
            {
                schema.ParentSchemaId = schema.ParentSchemaId + schemaSuffix;
            }

            foreach (var field in schema.Fields.OfType<FieldSingleTagbox>().Where(f => !systemSchemaIds.Contains(f.SchemaId)))
            {
                field.SchemaId = field.SchemaId + schemaSuffix;
            }

            foreach (var field in schema.Fields.OfType<FieldMultiTagbox>().Where(f => !systemSchemaIds.Contains(f.SchemaId)))
            {
                field.SchemaId = field.SchemaId + schemaSuffix;
            }

            foreach (var field in schema.Fields.OfType<FieldSingleFieldset>().Where(f => !systemSchemaIds.Contains(f.SchemaId)))
            {
                field.SchemaId = field.SchemaId + schemaSuffix;
            }

            foreach (var field in schema.Fields.OfType<FieldMultiFieldset>().Where(f => !systemSchemaIds.Contains(f.SchemaId)))
            {
                field.SchemaId = field.SchemaId + schemaSuffix;
            }

            foreach (var field in schema.Fields.OfType<FieldSingleRelation>().Where(f => !systemSchemaIds.Contains(f.SchemaId)))
            {
                field.SchemaId = field.SchemaId + schemaSuffix;
            }

            foreach (var field in schema.Fields.OfType<FieldMultiRelation>().Where(f => !systemSchemaIds.Contains(f.SchemaId)))
            {
                field.SchemaId = field.SchemaId + schemaSuffix;
            }
        }
    }
}