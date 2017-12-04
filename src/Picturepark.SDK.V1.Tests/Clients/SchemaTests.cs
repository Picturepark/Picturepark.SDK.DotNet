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
			Assert.Equal(schemas.Count, 8);
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
			Assert.Equal(schemas.Count, 9);
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
			Assert.Equal(schemas.Count, 1);

			// modify schema ID before submit
			var tagSchema = schemas.First();
			tagSchema.Id = "SchemaToDelete" + new Random().Next(0, 999999);

			await _client.Schemas.CreateAndWaitForCompletionAsync(tagSchema, false);
			var schemaDetail = await _client.Schemas.GetAsync(tagSchema.Id);

			/// Act
			await _client.Schemas.DeleteAndWaitForCompletionAsync(schemaDetail.Id);

			/// Assert
			await Assert.ThrowsAsync<ApiException>(async () => await _client.Schemas.GetAsync(tagSchema.Id));
		}

		[Fact]
		[Trait("Stack", "Schema")]
		public async Task ShouldExist()
		{
			string schemaId = _fixture.GetRandomSchemaId(20);

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
			var schemaId = _fixture.GetRandomSchemaId(20);

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
			await CreateFromClassGenericAsync<SoccerPlayer>();

			/// Act
			var generatedSoccerPlayerSchema = await _client.Schemas.GetAsync("SoccerPlayer");
			var jsonConvertedField = generatedSoccerPlayerSchema.Fields[0].ToJson();

			/// Assert
			Assert.True(jsonConvertedField.Contains(expectedFilterString));
		}

		[Fact]
		[Trait("Stack", "Schema")]
		public async Task ShouldCreateSchemaAndValidateMultiline()
		{
			/// Arrange
			string expectedMultilineString = "\"multiLine\":true";
			await CreateFromClassGenericAsync<Person>();

			/// Act
			var generatedSoccerPlayerSchema = await _client.Schemas.GetAsync("Person");
			var jsonConvertedField = generatedSoccerPlayerSchema.Fields[0].ToJson();

			/// Assert
			Assert.True(jsonConvertedField.Contains(expectedMultilineString));
		}

		[Fact]
		[Trait("Stack", "Schema")]
		public async Task ShouldCreateSchemaAndValidateMaxRecursion()
		{
			/// Act
			await CreateFromClassGenericAsync<Person>();
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
			string schemaId = _fixture.GetRandomSchemaId(20);
			SchemaDetail schemaDetail = await _client.Schemas.GetAsync(schemaId);

			string language = "es";

			schemaDetail.Names.Remove(language);
			schemaDetail.Names.Add(language, schemaId);

			/// Act
			await _client.Schemas.UpdateAndWaitForCompletionAsync(schemaDetail, false);

			/// Assert
			SchemaDetail updatedSchema = await _client.Schemas.GetAsync(schemaId);
			updatedSchema.Names.TryGetValue(language, out string outString);

			Assert.Equal(schemaId, outString);
		}

		private async Task CreateFromClassGenericAsync<T>()
			where T : class
		{
			var childSchemas = await _client.Schemas.GenerateSchemasAsync(typeof(T));

			foreach (var schema in childSchemas)
			{
				if (await _client.Schemas.ExistsAsync(schema.Id) == false)
				{
					await _client.Schemas.CreateAndWaitForCompletionAsync(schema, true);
				}
			}

			var schemaId = typeof(T).Name;
			var generatedPersonSchema = await _client.Schemas.GetAsync(schemaId);

			Assert.Contains(generatedPersonSchema.Types, i => i == SchemaType.List || i == SchemaType.Struct);
		}
	}
}