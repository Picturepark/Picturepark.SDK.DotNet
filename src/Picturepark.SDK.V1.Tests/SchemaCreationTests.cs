using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Linq;
using Xunit;

namespace Picturepark.SDK.V1.Tests
{
	public class SchemaCreationTests : IClassFixture<SDKClientFixture>
	{
		private readonly SDKClientFixture _fixture;
		private readonly PictureparkClient _client;

		public SchemaCreationTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "SchemaCreation")]
		public void ShouldIgnoreJsonProperty()
		{
			/// Act
			var jsonTransformSchemas = _client.Schemas.GenerateSchemaFromPOCO(typeof(JsonTransform)); // TODO: Rename to GenerateSchemaFromType or just GenerateSchema

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
		public void ShouldUseRenamedJsonProperty()
		{
			/// Act
			var jsonTransformSchemas = _client.Schemas.GenerateSchemaFromPOCO(typeof(JsonTransform));

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
