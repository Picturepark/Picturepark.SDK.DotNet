using FluentAssertions;
using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using Picturepark.SDK.V1.Tests.FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Xunit;

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
            await _fixture.RandomizeSchemaIdsAndCreateMany(allTypes).ConfigureAwait(false);
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
            Assert.Equal(11, schemas.Count);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateAndCreateSchemas()
        {
            // Act
            var schemaSuffix = new Random().Next(0, 999999);
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(PersonShot)).ConfigureAwait(false);

            foreach (var schema in schemas)
            {
                AppendSchemaIdSuffix(schema, schemaSuffix);
            }

            var createdSchemas = await _client.Schema.CreateManyAsync(schemas, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            var detail = await createdSchemas.FetchDetail();

            // Assert
            detail.SucceededItems.Should().HaveSameCount(schemas);
            detail.SucceededItems.Select(s => s.Id).Should().BeEquivalentTo(schemas.Select(s => s.Id));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateAndCreateAndUpdateSchemas()
        {
            // Act
            var schemaSuffix = new Random().Next(0, 999999);
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Person)).ConfigureAwait(false);

            foreach (var schema in schemas)
            {
                AppendSchemaIdSuffix(schema, schemaSuffix);
            }

            var result = await _client.Schema.CreateManyAsync(schemas, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            var detail = await result.FetchDetail().ConfigureAwait(false);

            // Add a new text field to the first created schema
            var createdSchema = detail.SucceededItems.First();
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

            generatedSoccerPlayerSchema.Audit.CreatedByUser.Should().BeResolved();
            generatedSoccerPlayerSchema.Audit.ModifiedByUser.Should().BeResolved();
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
        public async Task ShouldSearchAndAggregateAllTogether()
        {
            // Act
            var searchRequest = new SchemaSearchRequest
            {
                Limit = 5,
                Aggregators = new[]
                {
                    new TermsAggregator
                        { Field = nameof(SchemaDetail.Types).ToLowerCamelCase(), Name = "schemaTypesAggregation" }
                }
            };
            var searchResult = await _client.Schema.SearchAsync(searchRequest).ConfigureAwait(false);

            // Assert
            searchResult.Results.Should().HaveCount(5);

            // one aggregation result item per schema type
            searchResult.AggregationResults.Should()
                .HaveCount(1)
                .And.Subject.First()
                .AggregationResultItems.Should()
                .HaveCount(4)
                .And.OnlyContain(i => i.Count > 0);
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

            createResult.Schema.Audit.CreatedByUser.Should().BeResolved();
            createResult.Schema.Audit.ModifiedByUser.Should().BeResolved();

            updateResult.Schema.Audit.CreatedByUser.Should().BeResolved();
            updateResult.Schema.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateAndCreateSchemasWithDateTypeFields()
        {
            // Act
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Vehicle)).ConfigureAwait(false);

            var createdSchemas = await _fixture.RandomizeSchemaIdsAndCreateMany(schemas).ConfigureAwait(false);

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
        public async Task ShouldGetAggregationFieldsFromSchema()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Vehicle)).ConfigureAwait(false);
            var createdSchemas = await _fixture.RandomizeSchemaIdsAndCreateMany(schemas).ConfigureAwait(false);
            var firstSchemaId = createdSchemas.First(x => x.Id.StartsWith("Vehicle")).Id;

            // Act
            var fieldsInfo = await _client.Schema.GetAggregationFieldsAsync(firstSchemaId).ConfigureAwait(false);

            // Assert
            fieldsInfo.Count(f => !f.Static).Should().Be(2);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetAggregationFieldsFromSchemas()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Vehicle)).ConfigureAwait(false);
            var createdSchemas = await _fixture.RandomizeSchemaIdsAndCreateMany(schemas).ConfigureAwait(false);
            var vehicleSchemaId = createdSchemas.First(x => x.Id.StartsWith(nameof(Vehicle))).Id;
            var carSchemaId = createdSchemas.First(x => x.Id.StartsWith("Automobile")).Id;

            // Act
            var fieldsInfo = await _client.Schema.GetAggregationFieldsManyAsync(new[] { vehicleSchemaId, carSchemaId }).ConfigureAwait(false);

            // Assert
            fieldsInfo.Count(f => !f.Static).Should().Be(8);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetFilterFieldsFromSchema()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Vehicle)).ConfigureAwait(false);
            var createdSchemas = await _fixture.RandomizeSchemaIdsAndCreateMany(schemas).ConfigureAwait(false);
            var firstSchemaId = createdSchemas.First(x => x.Id.StartsWith("Vehicle")).Id;

            // Act
            var fieldsInfo = await _client.Schema.GetFilterFieldsAsync(firstSchemaId).ConfigureAwait(false);

            // Assert
            fieldsInfo.Count(f => !f.Static).Should().Be(2);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetFilterFieldsFromSchemas()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Vehicle)).ConfigureAwait(false);
            var createdSchemas = await _fixture.RandomizeSchemaIdsAndCreateMany(schemas).ConfigureAwait(false);
            var vehicleSchemaId = createdSchemas.First(x => x.Id.StartsWith(nameof(Vehicle))).Id;
            var carSchemaId = createdSchemas.First(x => x.Id.StartsWith("Automobile")).Id;

            // Act
            var fieldsInfo = await _client.Schema.GetFilterFieldsManyAsync(new[] { vehicleSchemaId, carSchemaId }).ConfigureAwait(false);

            // Assert
            fieldsInfo.Count(f => !f.Static).Should().Be(8);
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

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetReferencedSchemas()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Employee)).ConfigureAwait(false);
            var result = await _fixture.RandomizeSchemaIdsAndCreateMany(schemas).ConfigureAwait(false);

            var employeeSchemaId = result.Single(x => x.Id.StartsWith("Employee")).Id;

            // act
            var referencedSchemas = await _client.Schema.GetReferencedAsync(employeeSchemaId).ConfigureAwait(false);

            // assert
            referencedSchemas.Should().HaveCount(1);
            referencedSchemas.Single().Id.Should().StartWith("Department");

            referencedSchemas.ToList().ForEach(schema =>
                {
                    schema.Audit.CreatedByUser.Should().BeResolved();
                    schema.Audit.ModifiedByUser.Should().BeResolved();
                }
            );
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetReferencedSchemasWithSourceOne()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Employee)).ConfigureAwait(false);
            var result = await _fixture.RandomizeSchemaIdsAndCreateMany(schemas).ConfigureAwait(false);
            var employeeSchemaId = result.Single(x => x.Id.StartsWith("Employee")).Id;

            // act
            var referencedSchemas = await _client.Schema.GetReferencedAsync(employeeSchemaId, true).ConfigureAwait(false);

            // assert
            referencedSchemas.Should().HaveCount(2);
            referencedSchemas.Should().ContainSingle(s => s.Id.StartsWith("Department"));
            referencedSchemas.Should().ContainSingle(s => s.Id.StartsWith("Employee"));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldNotGetAnyOtherReferencedSchemasForCyclicDependency()
        {
            // Arrange
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Employee)).ConfigureAwait(false);
            var result = await _fixture.RandomizeSchemaIdsAndCreateMany(schemas).ConfigureAwait(false);

            // act
            var referencedSchemas = await _client.Schema.GetManyReferencedAsync(result.Select(i => i.Id)).ConfigureAwait(false);

            // assert
            referencedSchemas.Should().HaveCount(0);

            referencedSchemas.ToList().ForEach(schema =>
                {
                    schema.Audit.CreatedByUser.Should().BeResolved();
                    schema.Audit.ModifiedByUser.Should().BeResolved();
                }
            );
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldTransferOwnershipMany()
        {
            // Arrange
            var schemaRequests = await _client.Schema.GenerateSchemasAsync(typeof(Employee)).ConfigureAwait(false);
            var schemas = await _fixture.RandomizeSchemaIdsAndCreateMany(schemaRequests).ConfigureAwait(false);

            var currentOwner = await _client.User.GetByOwnerTokenAsync(schemas.First().OwnerTokenId).ConfigureAwait(false);

            var newPotentialOwners = await _client.User.SearchAsync(new UserSearchRequest
            {
                Limit = 10,
                UserRightsFilter = new List<UserRight> { UserRight.ManageSchemas }
            }).ConfigureAwait(false);

            var newOwnerId = newPotentialOwners.Results.FirstOrDefault(u => u.Id != currentOwner.Id)?.Id;
            newOwnerId.Should().NotBeNull($"expected to have more users with {UserRight.ManageSchemas} user right in the tested customer to test schema ownership transfer");

            var newOwner = await _client.User.GetAsync(newOwnerId).ConfigureAwait(false);
            var schemaIds = schemas.Select(i => i.Id).ToList();
            var manyRequest = new SchemaOwnershipTransferManyRequest
            {
                SchemaIds = schemaIds,
                TransferUserId = newOwner.Id
            };

            // Act
            var bp = await _client.Schema.TransferOwnershipManyAsync(manyRequest).ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(bp.Id).ConfigureAwait(false);

            // Assert
            var transferredSchemas = await _client.Schema.GetManyAsync(schemaIds).ConfigureAwait(false);
            transferredSchemas.Select(c => c.OwnerTokenId).Should().OnlyContain(ot => ot == newOwner.OwnerTokens.First().Id);

            transferredSchemas.ToList().ForEach(schema =>
                {
                    schema.Audit.CreatedByUser.Should().BeResolved();
                    schema.Audit.ModifiedByUser.Should().BeResolved();
                }
            );
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldUpdateMany()
        {
            // Arrange
            var schemaRequests = await _client.Schema.GenerateSchemasAsync(typeof(Thing)).ConfigureAwait(false);
            var schemas = await _fixture.RandomizeSchemaIdsAndCreateMany(schemaRequests).ConfigureAwait(false);

            // Act
            foreach (var childSchema in schemas.Where(s => s.ParentSchemaId != null))
            {
                if (childSchema.Fields.Single(f => f.Id == "color") is FieldString fieldString)
                    fieldString.Boost = 50;
            }

            // assert
            var result = await _client.Schema.UpdateManyAsync(schemas.Where(s => s.ParentSchemaId != null), false).ConfigureAwait(false);
            var detail = await result.FetchDetail().ConfigureAwait(false);

            detail.SucceededItems.Should().HaveCount(2);

            foreach (var childSchema in detail.SucceededItems.Where(s => s.ParentSchemaId != null))
            {
                childSchema.Fields.Should().HaveCount(2);
                childSchema.Fields.Single(f => f.Id == "color").Should().BeOfType<FieldString>().Which.Boost.Should().Be(50);
            }
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

        [PictureparkSchema(SchemaType.List)]
        [KnownType(typeof(Table))]
        [KnownType(typeof(Chair))]
        public class Thing
        {
            public string Name { get; set; }
        }

        [PictureparkSchema(SchemaType.List)]
        public class Table : Thing
        {
            [PictureparkSearch(Boost = 100)]
            public string Color { get; set; }
        }

        [PictureparkSchema(SchemaType.List)]
        public class Chair : Thing
        {
            [PictureparkSearch(Boost = 100)]
            public string Color { get; set; }
        }
    }
}