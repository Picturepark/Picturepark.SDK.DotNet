#pragma warning disable SA1201 // Elements must appear in the correct order

using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Builders;
using Picturepark.SDK.V1.Contract.Providers;
using Picturepark.SDK.V1.Providers;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Conversion
{
    public class SchemaCreationTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly PictureparkClient _client;

        public SchemaCreationTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldInvokeFilterProvider()
        {
            /// Act
            var allTypes = await _client.Schemas.GenerateSchemasAsync(typeof(ClassWithSimpleRelationAndFilterProvider));

            /// Assert
            var type = allTypes.Single(t => t.Id == nameof(ClassWithSimpleRelationAndFilterProvider));
            var field = (FieldSingleRelation)type.Fields.Single(f => f.Id == "relationField");
            var filter = (TermFilter)field.RelationTypes.First().Filter;

            Assert.Equal("contentType", filter.Field);
            Assert.Equal("Bitmap", filter.Term);
        }

        [PictureparkSchemaType(SchemaType.Content)]
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
            /// Act
            var allTypes = await _client.Schemas.GenerateSchemasAsync(typeof(ClassWithSimpleRelationAndSchemaIndexingInfoProvider));

            /// Assert
            var type = allTypes.Single(t => t.Id == nameof(ClassWithSimpleRelationAndSchemaIndexingInfoProvider));
            var field = (FieldSingleRelation)type.Fields.Single(f => f.Id == "relationField");
            var indexingInfo = field.SchemaIndexingInfo;

            Assert.Equal("relationField", indexingInfo.Fields.First().Id);
            Assert.Equal(11, indexingInfo.Fields.First().Boost);
        }

        [PictureparkSchemaType(SchemaType.Content)]
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
            /// Act
            var jsonTransformSchemas = await _client.Schemas.GenerateSchemasAsync(typeof(JsonTransform));

            /// Assert
            var jsonTransformSchema = jsonTransformSchemas.First();

            Assert.False(jsonTransformSchema.Fields.Any(i => i.Id == nameof(JsonTransform.IgnoredString)));
            var schemaSimpleRelation = jsonTransformSchemas.First(i => i.Id == nameof(SimpleRelation));

            Assert.True(schemaSimpleRelation.Fields.Any(i => i.Id == nameof(SimpleRelation.RelationInfo).ToLowerCamelCase()));
            Assert.False(schemaSimpleRelation.Fields.Any(i => i.Id == nameof(SimpleRelation.RelationId).ToLowerCamelCase()));
            Assert.False(schemaSimpleRelation.Fields.Any(i => i.Id == nameof(SimpleRelation.RelationType).ToLowerCamelCase()));
            Assert.False(schemaSimpleRelation.Fields.Any(i => i.Id == nameof(SimpleRelation.TargetContext).ToLowerCamelCase()));
            Assert.False(schemaSimpleRelation.Fields.Any(i => i.Id == nameof(SimpleRelation.TargetId).ToLowerCamelCase()));
        }

        [Fact]
        [Trait("Stack", "SchemaCreation")]
        public async Task ShouldUseRenamedJsonProperty()
        {
            /// Act
            var jsonTransformSchemas = await _client.Schemas.GenerateSchemasAsync(typeof(JsonTransform));

            /// Assert
            var jsonTransformSchema = jsonTransformSchemas.First(i => i.Id == nameof(JsonTransform));

            Assert.False(jsonTransformSchema.Fields.Any(i => i.Id == nameof(JsonTransform.OldName).ToLowerCamelCase()));
            Assert.True(jsonTransformSchema.Fields.Any(i => i.Id == "_newName"));
        }

        [PictureparkSchemaType(SchemaType.Struct)]
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
    }
}
