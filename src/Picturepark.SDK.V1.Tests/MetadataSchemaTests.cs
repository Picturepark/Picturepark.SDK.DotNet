using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Tests
{
	public class MetadataSchemaTests : IClassFixture<SDKClientFixture>
	{
		private readonly SDKClientFixture _fixture;
		private readonly PictureparkClient _client;

		public MetadataSchemaTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldCreateAllTypesSchemaFromClass()
		{
			var schemas = new List<MetadataSchemaDetailViewItem>();

			var allTypes = _client.Schemas.GenerateSchemaFromPOCO(typeof(AllDataTypesContract), schemas, true);

			foreach (var schema in allTypes)
			{
				if (await _client.Schemas.ExistsAsync(schema.Id) == false)
				{
					await _client.Schemas.CreateAsync(schema, true);
				}
			}
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldCreateFromClass()
		{
			var schemas = new List<MetadataSchemaDetailViewItem>();

			var person = _client.Schemas.GenerateSchemaFromPOCO(typeof(Person), schemas, true);

			// Expect child schemas and referenced schemas to exist
			Assert.Equal(person.Count, 8);

			foreach (var schema in person)
			{
				if (await _client.Schemas.ExistsAsync(schema.Id) == false)
				{
					await _client.Schemas.CreateAsync(schema, true);
				}
			}

			var generatedPersonSchema = await _client.Schemas.GetAsync("Person");
			Assert.Contains(generatedPersonSchema.Types, i => i == MetadataSchemaType.MetadataContent || i == MetadataSchemaType.Struct );

			var schemasFromPersonShot = new List<MetadataSchemaDetailViewItem>();
			var personShot = _client.Schemas.GenerateSchemaFromPOCO(typeof(PersonShot), schemasFromPersonShot, true);

			// Expect child schemas and referenced schemas to exist
			Assert.Equal(personShot.Count, 9);

			foreach (var schema in personShot)
			{
				if (await _client.Schemas.ExistsAsync(schema.Id) == false)
				{
					await _client.Schemas.CreateAsync(schema, true);
				}
			}
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldDelete()
		{
			var schemas = new List<MetadataSchemaDetailViewItem>();
			var tags = _client.Schemas.GenerateSchemaFromPOCO(typeof(Tag), schemas, true);

			Assert.Equal(tags.Count, 1);

			var tag = tags.First();
			tag.Id = "SchemaToDelete" + new Random().Next(0, 999999);

			await _client.Schemas.CreateAsync(tag, false);

			string metadataSchemaId = tag.Id;

			MetadataSchemaDetailViewItem schemaDetailViewItem = await _client.Schemas.GetAsync(metadataSchemaId);
			await _client.Schemas.DeleteAsync(schemaDetailViewItem.Id);
			MetadataSchemaDetailViewItem deletedSchemaDetailViewItem = null;

			try
			{
				deletedSchemaDetailViewItem = await _client.Schemas.GetAsync(metadataSchemaId);
			}
			catch (Exception)
			{
				// ignored
			}

			Assert.True(deletedSchemaDetailViewItem == null);
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldExist()
		{
			string metadataSchemaId = _fixture.GetRandomMetadataSchemaId(20);

			bool schemaExists = await _client.Schemas.ExistsAsync(metadataSchemaId);
			Assert.True(schemaExists);

			schemaExists = await _client.Schemas.ExistsAsync("abcabcabcabc");
			Assert.False(schemaExists);
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldGet()
		{
			var request = new MetadataSchemaSearchRequest() { Start = 0, Limit = 100 };
			BaseResultOfMetadataSchemaViewItem result = _client.Schemas.SearchAsync(request).Result;
			Assert.True(result.Results.Any());

			List<string> metadataSchemaIds = result.Results.Select(i => i.Id).OrderBy(i => i).ToList();
			var metadataSchemaIdsOk = new List<string>();
			var metadataSchemaIdsNotOk = new List<string>();

			foreach (var metadataSchemaId in metadataSchemaIds)
			{
				try
				{
					var schema = await _client.Schemas.GetAsync(metadataSchemaId);
					metadataSchemaIdsOk.Add(schema.Id);
				}
				catch
				{
					metadataSchemaIdsNotOk.Add(metadataSchemaId);
				}
			}

			Assert.False(metadataSchemaIdsNotOk.Count > 0);
			Assert.True(metadataSchemaIdsOk.Count > 0);
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldGetJsonValidationSchema()
		{
			var metadataSchemaId = _fixture.GetRandomMetadataSchemaId(20);
			Assert.False(string.IsNullOrEmpty(metadataSchemaId));

			var result = await _client.JsonSchemas.GetAsync(metadataSchemaId);
			var stringResult = result.ToString(Formatting.Indented);
			Assert.NotNull(result);
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldCreateSchemaAndValidateFilter()
		{
			await ShouldCreateFromClassGeneric<SoccerPlayer>();

			var generatedSoccerPlayerSchema = await _client.Schemas.GetAsync("SoccerPlayer");

			string filterString = "{\"Kind\":\"TermFilter\",\"Field\":\"AssetType\",\"Term\":\"FC Aarau\"}";

			var jsonConvertedField = generatedSoccerPlayerSchema.Fields[0].ToJson();

			bool containsString = jsonConvertedField.Contains(filterString);

			Assert.Equal(true, containsString);
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldCreateSchemaAndValidateMultiline()
		{
			await ShouldCreateFromClassGeneric<Person>();

			string multilineString = "\"MultiLine\":true";

			var generatedSoccerPlayerSchema = await _client.Schemas.GetAsync("Person");

			var jsonConvertedField = generatedSoccerPlayerSchema.Fields[0].ToJson();

			bool containsString = jsonConvertedField.Contains(multilineString);

			Assert.Equal(true, containsString);
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldCreateSchemaAndValidateMaxRecursion()
		{
			await ShouldCreateFromClassGeneric<Person>();
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldSearch()
		{
			var searchRequest = new MetadataSchemaSearchRequest
			{
				Limit = 12,
				SearchString = "D*"
			};

			BaseResultOfMetadataSchemaViewItem result = await _client.Schemas.SearchAsync(searchRequest);
			Assert.True(result.Results.Any());
		}

		[Fact]
		[Trait("Stack", "MetadataSchema")]
		public async Task ShouldUpdate()
		{
			string metadataSchemaId = _fixture.GetRandomMetadataSchemaId(20);
			MetadataSchemaDetailViewItem schemaDetailViewItem = await _client.Schemas.GetAsync(metadataSchemaId);

			string outString;
			string language = "es";

			schemaDetailViewItem.Names.Remove(language);
			schemaDetailViewItem.Names.Add(language, metadataSchemaId);

			await _client.Schemas.UpdateAsync(schemaDetailViewItem);

			MetadataSchemaDetailViewItem updatedSchema = await _client.Schemas.GetAsync(metadataSchemaId);

			updatedSchema.Names.TryGetValue(language, out outString);
			Assert.True(outString == metadataSchemaId);
		}

		public async Task ShouldCreateFromClassGeneric<T>() where T : class
		{
			var schemas = new List<MetadataSchemaDetailViewItem>();

			var childSchemas = _client.Schemas.GenerateSchemaFromPOCO(typeof(T), schemas, true);

			foreach (var schema in childSchemas)
			{
				if (await _client.Schemas.ExistsAsync(schema.Id) == false)
				{
					await _client.Schemas.CreateAsync(schema, true);
				}
			}

			var generatedPersonSchema = await _client.Schemas.GetAsync(typeof(T).Name);
			Assert.Contains(generatedPersonSchema.Types, i => i == MetadataSchemaType.MetadataContent || i == MetadataSchemaType.Struct);
		}
	}
}