using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            /// Act
            var allTypes = await _client.Schemas.GenerateSchemasAsync(typeof(AllDataTypesContract));
            foreach (var schema in allTypes)
            {
                await _client.Schemas.CreateOrUpdateAndWaitForCompletionAsync(schema, true);
            }
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateSchemas()
        {
            /// Act
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(Person));

            /// Assert
            Assert.Equal(8, schemas.Count);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateAndCreateSchemas()
        {
            /// Act
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(PersonShot));
            foreach (var schema in schemas)
            {
                if (await _client.Schemas.ExistsAsync(schema.Id) == false)
                {
                    await _client.Schemas.CreateAndWaitForCompletionAsync(schema, true);
                }
            }

            /// Assert
            Assert.True(await _client.Schemas.ExistsAsync(schemas.First().Id));
            Assert.Equal(9, schemas.Count);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCorrectlyDeserializeExceptions()
        {
            /// Arrange
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(Person));

            /// Act & Assert
            await Assert.ThrowsAsync<SchemaValidationException>(async () =>
            {
                foreach (var schema in schemas)
                {
                    schema.Id = "000";
                    await _client.Schemas.CreateOrUpdateAndWaitForCompletionAsync(schema, true); // throws exception
                }
            });
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGenerateAndCreateOrUpdateSchemas()
        {
            /// Act
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(Person));
            foreach (var schema in schemas)
            {
                await _client.Schemas.CreateOrUpdateAndWaitForCompletionAsync(schema, true);
            }

            var newSchemas = await _client.Schemas.GetAsync(nameof(Person));

            /// Assert
            Assert.Contains(newSchemas.Types, i => i == SchemaType.List || i == SchemaType.Struct);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldDelete()
        {
            /// Arrange
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(Tag));
            Assert.Equal(1, schemas.Count);

            // modify schema ID before submit
            var tagSchema = schemas.First();
            tagSchema.Id = "SchemaToDelete" + new Random().Next(0, 999999);

            await _client.Schemas.CreateAndWaitForCompletionAsync(tagSchema, false);
            var schemaDetail = await _client.Schemas.GetAsync(tagSchema.Id);

            /// Act
            await _client.Schemas.DeleteAndWaitForCompletionAsync(schemaDetail.Id);

            /// Assert
            await Assert.ThrowsAsync<SchemaNotFoundException>(async () => await _client.Schemas.GetAsync(tagSchema.Id));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldExist()
        {
            string schemaId = await _fixture.GetRandomSchemaIdAsync(20);

            /// Act
            bool schemaExists = await _client.Schemas.ExistsAsync(schemaId);

            /// Assert
            Assert.True(schemaExists);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldNotExist()
        {
            /// Act
            var schemaExists = await _client.Schemas.ExistsAsync("abcabcabcabc");

            /// Assert
            Assert.False(schemaExists);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGet()
        {
            /// Arrange
            var searchRequest = new SchemaSearchRequest { Start = 0, Limit = 100 };
            var searchResult = await _client.Schemas.SearchAsync(searchRequest);

            Assert.True(searchResult.Results.Any());

            List<string> schemaIds = searchResult.Results
                .Select(i => i.Id)
                .OrderBy(i => i)
                .ToList();

            /// Act
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

            /// Assert
            Assert.False(schemaIdsNotOk.Count > 0);
            Assert.True(schemaIdsOk.Count > 0);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetJsonValidationSchema()
        {
            /// Arrange
            var schemaId = await _fixture.GetRandomSchemaIdAsync(20);

            /// Act
            var result = await _client.JsonSchemas.GetAsync(schemaId);

            /// Assert
            Assert.NotNull(result.Property("definitions"));
            Assert.NotNull(result.ToString(Formatting.Indented));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateSchemaAndValidateFilter()
        {
            /// Arrange
            var expectedFilterString = "{\"kind\":\"TermFilter\",\"field\":\"contentType\",\"term\":\"FC Aarau\"}";
            await CreateSchemasAsync<SoccerPlayer>();

            /// Act
            var generatedSoccerPlayerSchema = await _client.Schemas.GetAsync("SoccerPlayer");
            var jsonConvertedField = generatedSoccerPlayerSchema.Fields.Single(i => i.Id == "club");

            /// Assert
            Assert.Contains(expectedFilterString, JsonConvert.SerializeObject(jsonConvertedField));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateSchemaAndValidateMultiline()
        {
            /// Arrange
            string expectedMultilineString = "\"multiLine\":true";
            await CreateSchemasAsync<Person>();

            /// Act
            var generatedSoccerPlayerSchema = await _client.Schemas.GetAsync("Person");
            var jsonConvertedField = generatedSoccerPlayerSchema.Fields.ToList()[0];

            /// Assert
            Assert.Contains(expectedMultilineString, JsonConvert.SerializeObject(jsonConvertedField));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldCreateSchemaAndValidateMaxRecursion()
        {
            /// Act
            await CreateSchemasAsync<Person>();
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldSearch()
        {
            /// Act
            var searchRequest = new SchemaSearchRequest
            {
                Limit = 12,
                SearchString = "D*"
            };

            var result = await _client.Schemas.SearchAsync(searchRequest);

            /// Assert
            Assert.True(result.Results.Any());
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldUpdate()
        {
            /// Arrange
            string schemaId = await _fixture.GetRandomSchemaIdAsync(20);
            SchemaDetail schemaDetail = await _client.Schemas.GetAsync(schemaId);

            string language = "de";

            schemaDetail.Names.Remove(language);
            schemaDetail.Names.Add(language, schemaId);

            /// Act
            await _client.Schemas.UpdateAndWaitForCompletionAsync(schemaDetail, false);

            /// Assert
            SchemaDetail updatedSchema = await _client.Schemas.GetAsync(schemaId);
            updatedSchema.Names.TryGetValue(language, out string outString);

            Assert.Equal(schemaId, outString);
        }

        [Fact(Skip = "MigrationOnly")]
        [Trait("Stack", "Schema")]
        public async Task ShouldWrapFiltersInNestedFilter()
        {
            var schemas = (await _client.Schemas.SearchAsync(new SchemaSearchRequest
            {
                Limit = 1000
            })).Results.Where(i => !i.System).ToList();

            var indexFields = await _client.Schemas.GetIndexFieldsAsync(new GetIndexFieldsRequest
            {
                SchemaIds = schemas.Select(i => i.Id).ToList()
            });
            foreach (var layer in schemas)
            {
                var details = await _client.Schemas.GetAsync(layer.Id);

                foreach (var field in details.Fields)
                {
                    if (field is FieldMultiTagbox multiTagbox)
                    {
                        if (multiTagbox.Filter != null)
                        {
                            multiTagbox.Filter = WrapFilter(multiTagbox.Filter, indexFields);
                        }
                    }

                    if (field is FieldSingleTagbox singleTagbox)
                    {
                        if (singleTagbox.Filter != null)
                        {
                            singleTagbox.Filter = WrapFilter(singleTagbox.Filter, indexFields);
                        }
                    }
                }

                foreach (var field in details.FieldsOverwrite)
                {
                    if (field is FieldOverwriteMultiTagbox multiTagbox)
                    {
                        if (multiTagbox.Filter != null)
                        {
                            multiTagbox.Filter = WrapFilter(multiTagbox.Filter, indexFields);
                        }
                    }

                    if (field is FieldOverwriteSingleTagbox singleTagbox)
                    {
                        if (singleTagbox.Filter != null)
                        {
                            singleTagbox.Filter = WrapFilter(singleTagbox.Filter, indexFields);
                        }
                    }
                }

                await _client.Schemas.UpdateAndWaitForCompletionAsync(details, false);
            }
        }

        private FilterBase WrapFilter(FilterBase filter, ICollection<IndexField> indexFields)
        {
            switch (filter)
            {
                case AndFilter andFilter:
                    var andFilters = andFilter.Filters.ToList();

                    for (var i = 0; i < andFilters.Count; i++)
                    {
                        andFilters[i] = WrapFilter(andFilters[i], indexFields);
                    }

                    andFilter.Filters = andFilters;
                    return andFilter;
                case OrFilter orFilter:
                    var orFilters = orFilter.Filters.ToList();

                    for (var i = 0; i < orFilters.Count; i++)
                    {
                        orFilters[i] = WrapFilter(orFilters[i], indexFields);
                    }

                    orFilter.Filters = orFilters;
                    return orFilter;
                case NotFilter notFilter:
                    return WrapFilter(notFilter.Filter, indexFields);
                case TermFilter termFilter:
                    return GetWrappedFilter(termFilter.Field, termFilter, indexFields);
                case TermsFilter termsFilter:
                    return GetWrappedFilter(termsFilter.Field, termsFilter, indexFields);
                case PrefixFilter prefixFilter:
                    return GetWrappedFilter(prefixFilter.Field, prefixFilter, indexFields);
                case NestedFilter nestedFilter:
                    return nestedFilter;
                default:
                    throw new NotImplementedException();
            }
        }

        private FilterBase GetWrappedFilter(string field, FilterBase filter, ICollection<IndexField> indexFields)
        {
            var indexField = indexFields.SingleOrDefault(i => i.Id == field.Replace(".x-default", string.Empty));
            if (!string.IsNullOrEmpty(indexField?.NestedPath))
            {
                return new NestedFilter
                {
                    Path = indexField.NestedPath,
                    Filter = filter
                };
            }

            return filter;
        }

        private async Task CreateSchemasAsync<T>()
            where T : class
        {
            var childSchemas = await _client.Schemas.GenerateSchemasAsync(typeof(T));

            foreach (var schema in childSchemas)
            {
                if (await _client.Schemas.ExistsAsync(schema.Id) == false)
                {
                    try
                    {
                        await _client.Schemas.CreateAndWaitForCompletionAsync(schema, true);
                    }
                    catch (DuplicateSchemaException)
                    {
                        // ignore DuplicateSchemaException exceptions
                    }
                }
            }

            var schemaId = typeof(T).Name;
            var generatedPersonSchema = await _client.Schemas.GetAsync(schemaId);

            Assert.Contains(generatedPersonSchema.Types, i => i == SchemaType.List || i == SchemaType.Struct);
        }
    }
}