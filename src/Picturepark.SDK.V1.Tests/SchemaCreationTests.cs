using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			var jsonTransform = _client.Schemas.GenerateSchemaFromPOCO(typeof(JsonTransform));
			var schema = jsonTransform.First();

			Assert.False(schema.Fields.Any(i => i.Id == nameof(JsonTransform.IgnoredString)));

			var schemaSimpleRelation = jsonTransform.First(i => i.Id == nameof(SimpleRelation));

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
			var jsonTransform = _client.Schemas.GenerateSchemaFromPOCO(typeof(JsonTransform));
			var schemaJsonTransform = jsonTransform.First(i => i.Id == nameof(JsonTransform));

			Assert.False(schemaJsonTransform.Fields.Any(i => i.Id == nameof(JsonTransform.OldName).ToLowerCamelCase()));
			Assert.True(schemaJsonTransform.Fields.Any(i => i.Id == "_newName"));
		}
	}
}
