#pragma warning disable SA1201 // Elements must appear in the correct order

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Builders;
using Picturepark.SDK.V1.Contract.Attributes.Analyzer;
using Picturepark.SDK.V1.Contract.Providers;
using Picturepark.SDK.V1.Providers;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Conversion
{
    public class SchemaCreationTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public SchemaCreationTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldInvokeFilterProvider()
        {
            // Act
            var allTypes = await _client.Schema.GenerateSchemasAsync(typeof(ClassWithSimpleRelationAndFilterProvider)).ConfigureAwait(false);

            // Assert
            var type = allTypes.Single(t => t.Id == nameof(ClassWithSimpleRelationAndFilterProvider));
            var field = (FieldSingleRelation)type.Fields.Single(f => f.Id == "relationField");
            var filter = (TermFilter)field.RelationTypes.First().Filter;

            Assert.Equal("contentType", filter.Field);
            Assert.Equal("Bitmap", filter.Term);
        }

        [PictureparkSchema(SchemaType.Content)]
        public class ClassWithSimpleRelationAndFilterProvider
        {
            [PictureparkContentRelation("RelationName", typeof(RelationFieldFilterProvider))]
            public SimpleRelation RelationField { get; set; }

            public class RelationFieldFilterProvider : IFilterProvider
            {
                public FilterBase GetFilter()
                {
                    return new TermFilter { Field = "contentType", Term = "Bitmap" };
                }
            }
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldInvokeSchemaIndexingInfoProvider()
        {
            // Act
            var allTypes = await _client.Schema.GenerateSchemasAsync(typeof(ClassWithSimpleRelationAndSchemaIndexingInfoProvider)).ConfigureAwait(false);

            // Assert
            var type = allTypes.Single(t => t.Id == nameof(ClassWithSimpleRelationAndSchemaIndexingInfoProvider));
            var field = (FieldSingleRelation)type.Fields.Single(f => f.Id == "relationField");
            var indexingInfo = field.SchemaIndexingInfo;

            Assert.Equal("relationField", indexingInfo.Fields.First().Id);
            Assert.Equal(11, indexingInfo.Fields.First().Boost);
        }

        [PictureparkSchema(SchemaType.Content)]
        public class ClassWithSimpleRelationAndSchemaIndexingInfoProvider
        {
            [PictureparkContentRelation("RelationName", "{ 'kind': 'TermFilter', 'field': 'contentType', term: 'Bitmap' }")]
            [PictureparkSchemaIndexing(typeof(RelationFieldSchemaIndexingInfoProvider))]
            public SimpleRelation RelationField { get; set; }

            public class RelationFieldSchemaIndexingInfoProvider : SchemaIndexingInfoProvider<ClassWithSimpleRelationAndSchemaIndexingInfoProvider>
            {
                protected override SchemaIndexingInfoBuilder<ClassWithSimpleRelationAndSchemaIndexingInfoProvider> Setup(
                    SchemaIndexingInfoBuilder<ClassWithSimpleRelationAndSchemaIndexingInfoProvider> builder)
                {
                    return builder.AddIndexWithSimpleSearch(p => p.RelationField, 11);
                }
            }
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldIgnoreJsonProperty()
        {
            // Act
            var jsonTransformSchemas = await _client.Schema.GenerateSchemasAsync(typeof(JsonTransform)).ConfigureAwait(false);

            // Assert
            var jsonTransformSchema = jsonTransformSchemas.First();

            Assert.DoesNotContain(jsonTransformSchema.Fields, i => i.Id == nameof(JsonTransform.IgnoredString));
            var schemaSimpleRelation = jsonTransformSchemas.First(i => i.Id == nameof(SimpleRelation));

            Assert.Contains(schemaSimpleRelation.Fields, i => i.Id == nameof(SimpleRelation.RelationInfo).ToLowerCamelCase());
            Assert.DoesNotContain(schemaSimpleRelation.Fields, i => i.Id == nameof(SimpleRelation.RelationId).ToLowerCamelCase());
            Assert.DoesNotContain(schemaSimpleRelation.Fields, i => i.Id == nameof(SimpleRelation.RelationType).ToLowerCamelCase());
            Assert.DoesNotContain(schemaSimpleRelation.Fields, i => i.Id == nameof(SimpleRelation.TargetDocType).ToLowerCamelCase());
            Assert.DoesNotContain(schemaSimpleRelation.Fields, i => i.Id == nameof(SimpleRelation.TargetId).ToLowerCamelCase());
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldUseRenamedJsonProperty()
        {
            // Act
            var jsonTransformSchemas = await _client.Schema.GenerateSchemasAsync(typeof(JsonTransform)).ConfigureAwait(false);

            // Assert
            var jsonTransformSchema = jsonTransformSchemas.First(i => i.Id == nameof(JsonTransform));

            Assert.DoesNotContain(jsonTransformSchema.Fields, i => i.Id == nameof(JsonTransform.OldName).ToLowerCamelCase());
            Assert.Contains(jsonTransformSchema.Fields, i => i.Id == "_newName");
        }

        [PictureparkSchema(SchemaType.Struct)]
        public class JsonTransform
        {
            [JsonIgnore]
            public string IgnoredString { get; set; }

            [JsonProperty("_newName")]
            public string OldName { get; set; }

            [PictureparkContentRelation(
                "RelationName",
                "{ 'kind': 'TermFilter', 'field': 'contentType', term: 'Bitmap' }"
            )]
            public SimpleRelation RelationField { get; set; }
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldNotAllowRelationsMarkedAsSortable()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async() => await _client.Schema.GenerateSchemasAsync(typeof(ClassSortableRelation)).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [PictureparkSchema(SchemaType.List)]
        public class ClassSortableRelation
        {
            [PictureparkContentRelation(
                "RelationName",
                "{ 'kind': 'TermFilter', 'field': 'contentType', term: 'Bitmap' }"
            )]
            [PictureparkSort]
            public SimpleRelation Relation { get; set; }
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldNotAllowGeopointsMarkedAsSortable()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _client.Schema.GenerateSchemasAsync(typeof(ClassSortableGeopoint)).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [PictureparkSchema(SchemaType.List)]
        public class ClassSortableGeopoint
        {
            [PictureparkSort]
            public GeoPoint Location { get; set; }
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldMarkFieldAsSortableWhenMarkedWithSortAttribute()
        {
            var schema = await _client.Schema.GenerateSchemasAsync(typeof(ClassSortableString)).ConfigureAwait(false);
            Assert.True(schema.First().Fields.First().Sortable);
        }

        [PictureparkSchema(SchemaType.List)]
        public class ClassSortableString
        {
            [PictureparkSort]
            public string Title { get; set; }
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldNotAllowAnalyzerWithoutIndexOrSearch()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _client.Schema.GenerateSchemasAsync(typeof(ClassAnalyzerWithoutIndexAndSearch)));
        }

        [PictureparkSchema(SchemaType.List)]
        public class ClassAnalyzerWithoutIndexAndSearch
        {
            [PictureparkSimpleAnalyzer(Index = false, SimpleSearch = false)]
            public string Title { get; set; }
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldNotAllowMultipleDisplayPatternsOfSameTypeAndLanguage()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _client.Schema.GenerateSchemasAsync(typeof(ClassWithMultipleDisplayPatternsForEnglishName)).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [PictureparkSchema(SchemaType.List)]
        [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{pattern}}", "en")]
        [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{pattern}}", "en")]
        public class ClassWithMultipleDisplayPatternsForEnglishName
        {
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldSetViewForAllForStructs()
        {
            var schema = await _client.Schema.GenerateSchemasAsync(typeof(ClassStruct)).ConfigureAwait(false);
            schema.Single().ViewForAll.Should().BeTrue();
        }

        [PictureparkSchema(SchemaType.Struct)]
        public class ClassStruct
        {
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldSetViewForAllForContents()
        {
            var schema = await _client.Schema.GenerateSchemasAsync(typeof(ClassContent)).ConfigureAwait(false);
            schema.Single().ViewForAll.Should().BeTrue();
        }

        [PictureparkSchema(SchemaType.Content)]
        public class ClassContent
        {
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldSupportCyclicDependencies()
        {
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Employee)).ConfigureAwait(false);
            schemas.Count.Should().Be(2);

            var employeeSchema = schemas.Single(x => x.Id == nameof(Employee));
            var departmentReference = employeeSchema.Fields.Single(x => x.Id.ToLower() == nameof(Employee.MemberOf).ToLower());

            departmentReference.Should().BeOfType<FieldSingleTagbox>();
        }

        [PictureparkSchema(SchemaType.List)]
        [PictureparkReference]
        public class Employee
        {
            public Department MemberOf { get; set; }
        }

        [PictureparkSchema(SchemaType.List)]
        [PictureparkReference]
        public class Department
        {
            public Employee Supervisor { get; set; }
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldSupportCyclicDependenciesWithIndirectCycle()
        {
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(Country)).ConfigureAwait(false);
            schemas.Count.Should().Be(3);

            var countrySchema = schemas.Single(x => x.Id == nameof(Country));
            countrySchema.Fields.Should().HaveCount(2);
            countrySchema.Fields.OfType<FieldSingleTagbox>().Single().SchemaId.Should().Be(nameof(City));

            var stateSchema = schemas.Single(x => x.Id == nameof(State));
            stateSchema.Fields.Should().HaveCount(2);
            stateSchema.Fields.OfType<FieldSingleTagbox>().Single().SchemaId.Should().Be(nameof(Country));

            var citySchema = schemas.Single(x => x.Id == nameof(City));
            citySchema.Fields.Should().HaveCount(2);
            citySchema.Fields.OfType<FieldSingleTagbox>().Single().SchemaId.Should().Be(nameof(State));
        }

        [PictureparkSchema(SchemaType.List)]
        [PictureparkReference]
        public class Country
        {
            public string Name { get; set; }

            public City Capital { get; set; }
        }

        [PictureparkSchema(SchemaType.List)]
        [PictureparkReference]
        public class State
        {
            public string Name { get; set; }

            public Country Country { get; set; }
        }

        [PictureparkSchema(SchemaType.List)]
        [PictureparkReference]
        public class City
        {
            public string Name { get; set; }

            public State State { get; set; }
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldGenerateTriggerField()
        {
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(ListWithTrigger)).ConfigureAwait(false);
            schemas.Should().HaveCount(1);

            var schema = schemas.Single();
            schema.Fields.Should().HaveCount(1);

            var triggerField = schema.Fields.Single();
            triggerField.Should().BeOfType<FieldTrigger>();
            triggerField.Index.Should().BeFalse();
            triggerField.SimpleSearch.Should().BeTrue();
            ((FieldTrigger)triggerField).Boost.Should().Be(1.3);
        }

        [PictureparkSchema(SchemaType.List)]
        public class ListWithTrigger
        {
            [PictureparkSearch(Boost = 1.3, Index = false, SimpleSearch = true)]
            public TriggerObject Trigger { get; set; }
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldGenerateFormattedStringFields()
        {
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(SchemaWithFormattedStrings)).ConfigureAwait(false);
            var generatedSchema = schemas.Should().ContainSingle().Which;

            void Assert(SchemaDetail schema)
            {
                var nonTranslatedFields = schema.Fields.OfType<FieldString>().ToList();
                nonTranslatedFields.Should().HaveCount(2);

                var fieldPlainSimple = nonTranslatedFields.Should().ContainSingle(f => f.Id == nameof(SchemaWithFormattedStrings.PlainSimple).ToLowerCamelCase()).Which;
                fieldPlainSimple.RenderingType.Should().Be(StringRenderingType.Default);

                var fieldFormattedSimple = nonTranslatedFields.Should().ContainSingle(f => f.Id == nameof(SchemaWithFormattedStrings.FormattedSimple).ToLowerCamelCase()).Which;
                fieldFormattedSimple.RenderingType.Should().Be(StringRenderingType.Markdown);

                var translatedFields = schema.Fields.OfType<FieldTranslatedString>().ToList();
                translatedFields.Should().HaveCount(2);

                var fieldTranslatedSimple = translatedFields.Should().ContainSingle(f => f.Id == nameof(SchemaWithFormattedStrings.PlainTranslated).ToLowerCamelCase()).Which;
                fieldTranslatedSimple.RenderingType.Should().Be(StringRenderingType.Default);

                var fieldTranslatedFormatted = translatedFields.Should().ContainSingle(f => f.Id == nameof(SchemaWithFormattedStrings.FormattedTranslated).ToLowerCamelCase()).Which;
                fieldTranslatedFormatted.RenderingType.Should().Be(StringRenderingType.Markdown);
            }

            Assert(generatedSchema);

            var createResult = await _client.Schema.CreateAsync(schemas.Single()).ConfigureAwait(false);
            try
            {
                Assert(createResult.Schema);
            }
            finally
            {
                await _client.Schema.DeleteAsync(createResult.Schema.Id).ConfigureAwait(false);
            }
        }

        [PictureparkSchema(SchemaType.List)]
        public class SchemaWithFormattedStrings
        {
            public string PlainSimple { get; set; }

            public TranslatedStringDictionary PlainTranslated { get; set; }

            [PictureparkString(RenderingType = StringRenderingType.Markdown)]
            public string FormattedSimple { get; set; }

            [PictureparkTranslatedString(RenderingType = StringRenderingType.Markdown)]
            public TranslatedStringDictionary FormattedTranslated { get; set; }
        }
    }
}
