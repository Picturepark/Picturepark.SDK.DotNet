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
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class SchemaTests : IClassFixture<SchemaFixture>
    {
        private readonly SchemaFixture _fixture;
        private readonly IPictureparkService _client;

        public SchemaTests(SchemaFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateAllTypesSchemaFromClass()
        {
            // Act
            var allTypes = await _client.Schema.GenerateSchemasAsync(typeof(AllDataTypesContract)).ConfigureAwait(false);
            foreach (var schema in allTypes)
            {
                await _client.Schema.CreateOrUpdateAsync(schema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            }
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCorrectlyDeserializeExceptions()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Person)).ConfigureAwait(false);

            // Act & Assert
            await Assert.ThrowsAsync<SchemaValidationException>(async () =>
            {
                foreach (var schema in schemas)
                {
                    schema.LayerSchemaIds = new[] { "notExistingLayerId" };
                    await _client.Schema.CreateOrUpdateAsync(schema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false); // throws exception
                }
            }).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateSchemas()
        {
            // Act
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Person)).ConfigureAwait(false);

            // Assert
            Assert.Equal(8, schemas.Count);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateAndCreateSchemas()
        {
            // Act
            var schemaSuffix = new Random().Next(0, 999999);
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(PersonShot)).ConfigureAwait(false);
            IList<SchemaDetail> createdSchemas = new List<SchemaDetail>();
            foreach (var schema in schemas)
            {
                AppendSchemaIdSuffix(schema, schemaSuffix);
                var result = await _client.Schema.CreateAsync(schema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
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
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Person)).ConfigureAwait(false);

            IList<SchemaDetail> createdSchemas = new List<SchemaDetail>();
            foreach (var schema in schemas)
            {
                AppendSchemaIdSuffix(schema, schemaSuffix);
                var result = await _client.Schema.CreateAsync(schema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
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
            var updateResult = await _client.Schema.UpdateAsync(createdSchema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            // Assert
            updateResult.Schema.Fields.Should().Contain(f => f.Id == $"newField{updateResult.Schema.Id}");
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateSchemasWithCorrectInheritance()
        {
            // Act
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Person)).ConfigureAwait(false);

            // Assert
            var pet = schemas.Single(s => s.Id == "Pet");
            var cat = schemas.Single(s => s.Id == "Cat");
            var dog = schemas.Single(s => s.Id == "Dog");

            Assert.Equal(string.Empty, pet.ParentSchemaId);
            Assert.Equal(pet.Id, cat.ParentSchemaId);
            Assert.Equal(pet.Id, dog.ParentSchemaId);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldDelete()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Tag)).ConfigureAwait(false);
            Assert.Equal(1, schemas.Count);

            // modify schema ID before submit
            var tagSchema = schemas.First();
            tagSchema.Id = "SchemaToDelete" + new Random().Next(0, 999999);

            var result = await _client.Schema.CreateAsync(tagSchema, false, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            // Act
            await _client.Schema.DeleteAsync(result.Schema.Id, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            // Assert
            await Assert.ThrowsAsync<SchemaNotFoundException>(async () => await _client.Schema.GetAsync(tagSchema.Id).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldExist()
        {
            var schemaId = SchemaFixture.SystemSchemaIds.First();

            // Act
            var schemaExists = await _client.Schema.ExistsAsync(schemaId).ConfigureAwait(false);

            // Assert
            schemaExists.Should().BeTrue();
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldNotExist()
        {
            // Act
            var schemaExists = await _client.Schema.ExistsAsync("abcabcabcabc").ConfigureAwait(false);

            // Assert
            Assert.False(schemaExists);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGet()
        {
            // Arrange
            var searchRequest = new SchemaSearchRequest { Limit = 2 };
            var searchResult = await _client.Schema.SearchAsync(searchRequest).ConfigureAwait(false);

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
                    var schema = await _client.Schema.GetAsync(schemaId).ConfigureAwait(false);
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
            var schemaId = SchemaFixture.SystemSchemaIds.First();

            // Act
            var result = await _client.JsonSchema.GetAsync(schemaId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result.Property("definitions"));
            Assert.NotNull(result.ToString(Formatting.Indented));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateSchemaAndValidateFilter()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<SoccerPlayer>(_client).ConfigureAwait(false);
            var expectedFilterString = "{\"kind\":\"TermFilter\",\"field\":\"contentType\",\"term\":\"FC Aarau\"}";

            // Act
            var generatedSoccerPlayerSchema = await _client.Schema.GetAsync("SoccerPlayer").ConfigureAwait(false);
            var jsonConvertedField = generatedSoccerPlayerSchema.Fields.Single(i => i.Id == "club");

            // Assert
            Assert.Contains(expectedFilterString, JsonConvert.SerializeObject(jsonConvertedField));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateSchemaAndValidateMultiline()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client).ConfigureAwait(false);
            string expectedMultilineString = "\"multiLine\":true";

            // Act
            var generatedSoccerPlayerSchema = await _client.Schema.GetAsync("Person").ConfigureAwait(false);
            var jsonConvertedField = generatedSoccerPlayerSchema.Fields.ToList()[0];

            // Assert
            Assert.Contains(expectedMultilineString, JsonConvert.SerializeObject(jsonConvertedField));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateSchemaAndValidateMaxRecursion()
        {
            // Act
            var schema = await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client).ConfigureAwait(false);

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

            var result = await _client.Schema.SearchAsync(searchRequest).ConfigureAwait(false);

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
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Tag)).ConfigureAwait(false);
            var tagSchema = schemas.First();
            tagSchema.Id = "SchemaToUpdate" + new Random().Next(0, 999999);

            var createResult = await _client.Schema.CreateAsync(tagSchema, false, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            var schemaDetail = createResult.Schema;
            schemaDetail.Names[language] = schemaTranslation;

            // Act
            var updateResult = await _client.Schema.UpdateAsync(schemaDetail, false, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            // Assert
            updateResult.Schema.Names.TryGetValue(language, out string outString);

            Assert.Equal(schemaTranslation, outString);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateAndCreateSchemasWithDateTypeFields()
        {
            // Act
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Vehicle)).ConfigureAwait(false);

            var createdSchemas = await _fixture.RandomizeSchemaIdsAndCreate(schemas).ConfigureAwait(false);

            // Assert
            createdSchemas.Should().HaveSameCount(schemas);
            createdSchemas.Select(s => s.Id).Should().BeEquivalentTo(schemas.Select(s => s.Id));

            var autoSchema = createdSchemas.Should().ContainSingle(s => s.Id.StartsWith("Automobile")).Subject;
            autoSchema.Fields.Should().ContainSingle(f => f.Id == nameof(Car.Introduced).ToLowerCamelCase())
                .Which.Should().BeOfType<FieldDate>();
            autoSchema.Fields.Should().ContainSingle(f => f.Id == nameof(Car.FirstPieceManufactured).ToLowerCamelCase())
                .Which.Should().BeOfType<FieldDateTime>()
                .Which.Format.Should().Be("YYYY-MM-DD hh:mm:ss");
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetIndexedFields()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Vehicle)).ConfigureAwait(false);
            var createdSchemas = await _fixture.RandomizeSchemaIdsAndCreate(schemas).ConfigureAwait(false);
            var firstSchemaId = createdSchemas.First(x => x.Id.StartsWith("Vehicle")).Id;

            // Act
            var indexedFields = await _client.Schema.GetIndexFieldsAsync(
                new IndexFieldsSearchBySchemaIdsRequest
                {
                    SchemaIds = new[] { firstSchemaId },
                    SearchMode = IndexFieldsSearchMode.SchemaAndParentFieldsOnly
                }).ConfigureAwait(false);

            // Assert
            indexedFields.Count.Should().Be(2);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateSimpleCyclicDependency()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Employee)).ConfigureAwait(false);

            // act
            var result = await _fixture.RandomizeSchemaIdsAndCreateMany(schemas).ConfigureAwait(false);

            // assert
            result.Should().HaveCount(2);
        }

        private void AppendSchemaIdSuffix(SchemaDetail schema, int schemaSuffix)
            => _fixture.AppendSchemaIdSuffix(schema, schemaSuffix);

        [PictureparkReference]
        [PictureparkSchema(SchemaType.List)]
        public class Employee
        {
            public string Name { get; set; }

            public Department MemberOf { get; set; }
        }

        [PictureparkReference]
        [PictureparkSchema(SchemaType.List)]
        public class Department
        {
            public string Name { get; set; }

            public IList<Employee> Supervisors { get; set; }
        }
    }
}