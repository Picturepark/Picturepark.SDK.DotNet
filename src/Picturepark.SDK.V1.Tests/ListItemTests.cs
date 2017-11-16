using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Extensions;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests
{
	public class ListItemTests : IClassFixture<ListItemFixture>
	{
		private readonly SDKClientFixture _fixture;
		private readonly PictureparkClient _client;

		public ListItemTests(ListItemFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldAggregateObjects()
		{
			/// Arrange
			var fieldName = nameof(Country).ToLowerCamelCase() + "." + nameof(Country.RegionCode).ToLowerCamelCase();
			var request = new ListItemAggregationRequest
			{
				SearchString = "*",
				SchemaIds = new List<string> { nameof(Country) },
				Aggregators = new List<AggregatorBase>
				{
					new TermsAggregator { Name = fieldName, Field = fieldName, Size = 20 }
				}
			};

			/// Act
			ObjectAggregationResult result = await _client.ListItems.AggregateAsync(request);
			AggregationResult aggregation = result.GetByName(fieldName);

			/// Assert
			Assert.NotNull(aggregation);
			Assert.Equal(aggregation.AggregationResultItems.Count, 20);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldDelete()
		{
			/// Arrange
			var objectName = "ThisObjectA" + new Random().Next(0, 999999);
			var createRequest = new ListItemCreateRequest
			{
				ContentSchemaId = nameof(Tag),
				Content = new Tag { Name = objectName }
			};

			ListItemDetail listItem = await _client.ListItems.CreateAsync(createRequest);
			Assert.False(string.IsNullOrEmpty(listItem.Id));

			/// Act
			await _client.ListItems.DeleteAsync(listItem.Id);

			/// Assert
			// TODO: ListItemClient.GetAsync: Throw specific 404 exception
			await Assert.ThrowsAsync<ApiException>(async () => await _client.ListItems.GetAsync(listItem.Id, true));
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldDeleteMany()
		{
			/// Arrange
			var objectName = "ThisObjectA" + new Random().Next(0, 999999);
			var createRequest = new ListItemCreateRequest
			{
				ContentSchemaId = nameof(Tag),
				Content = new Tag { Name = objectName }
			};

			ListItemDetail listItem1 = await _client.ListItems.CreateAsync(createRequest);
			ListItemDetail listItem2 = await _client.ListItems.CreateAsync(createRequest);

			/// Act
			var businessProcess = await _client.ListItems.DeleteManyAsync(new List<string> { listItem1.Id, listItem2.Id });
			await _client.BusinessProcesses.WaitForCompletionAsync(businessProcess.Id);

			/// Assert
			await Assert.ThrowsAsync<ApiException>(async () => await _client.ListItems.GetAsync(listItem1.Id, true));
			await Assert.ThrowsAsync<ApiException>(async () => await _client.ListItems.GetAsync(listItem2.Id, true));
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateAndUpdateObject()
		{
			/// Arrange
			var objectName = "ThisObjectD" + new Random().Next(0, 999999);
			var objects = new List<ListItemCreateRequest>
			{
				new ListItemCreateRequest
				{
					ContentSchemaId = nameof(Tag),
					Content = new Tag { Name = objectName }
				}
			};

			var results = await _client.ListItems.CreateManyAsync(objects);
			var result = results.First();

			/// Act
			var request = new ListItemUpdateRequest
			{
				Id = result.Id,
				Content = new Tag { Name = "Foo" }
			};
			await _client.ListItems.UpdateAsync(request);

			/// Assert
			var newItem = await _client.ListItems.GetAsync(result.Id, true);
			Assert.Equal("Foo", newItem.ConvertToType<Tag>(nameof(Tag)).Name);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreate()
		{
			/// Arrange
			var objectName = "ThisObjectB" + new Random().Next(0, 999999);
			var listItem = new ListItemCreateRequest
			{
				ContentSchemaId = nameof(Tag),
				Content = new Tag { Name = objectName }
			};

			/// Act
			ListItemDetail result = await _client.ListItems.CreateAsync(listItem);

			/// Assert
			Assert.False(string.IsNullOrEmpty(result.Id));
		}

		[Fact]
		[Trait("Stack", "BusinessProcesses")]
		public async Task ShouldUpdateFieldsByFilter()
		{
			/// Arrange
			var objectName = "ThisObjectB" + new Random().Next(0, 999999);
			var listItem = new ListItemCreateRequest
			{
				ContentSchemaId = nameof(Tag),
				Content = new Tag { Name = objectName }
			};
			ListItemDetail listItemDetail = await _client.ListItems.CreateAsync(listItem);

			/// Act
			var updateRequest = new ListItemFieldsFilterUpdateRequest
			{
				ListItemFilterRequest = new ListItemFilterRequest // TODO: ListItemFieldsFilterUpdateRequest.ListItemFilterRequest: Rename to FilterRequest
				{
					Filter = new TermFilter { Field = "id", Term = listItemDetail.Id }
				},
				ChangeCommands = new List<MetadataValuesSchemaUpdateCommand>
				{
					new MetadataValuesSchemaUpdateCommand
					{
						SchemaId = nameof(Tag),
						Value = new DataDictionary
						{
							{ "name", "Foo" }
						}
					}
				}
			};

			/// Act
			var businessProcess = await _client.ListItems.UpdateFieldsByFilterAsync(updateRequest);
			var waitResult = await _client.BusinessProcesses.WaitForCompletionAsync(businessProcess.Id, 10 * 1000);

			ListItemDetail result = await _client.ListItems.GetAsync(listItemDetail.Id, true);

			/// Assert
			Assert.Equal("Foo", result.ConvertToType<Tag>(nameof(Tag)).Name);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateWithHelper()
		{
			/// Arrange
			var tag = new Tag
			{
				Name = "ThisObjectB" + new Random().Next(0, 999999)
			};

			/// Act
			var createdObjects = await _client.ListItems.CreateFromPOCOAsync(tag, nameof(Tag));

			/// Assert
			Assert.Equal(1, createdObjects.Count());
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateComplexObjectWithHelper()
		{
			// Reusable as reference
			var dog = new Dog
			{
				Name = "Dogname1",
				PlaysCatch = true
			};

			// Using Helper method
			var soccerPlayerTree = await _client.ListItems.CreateFromPOCOAsync(
				new SoccerPlayer
				{
					BirthDate = DateTime.Now,
					EmailAddress = "xyyyy@teyyyyyyst.com",
					Firstname = "Urxxxxs",
					LastName = "xxxxxxxx",
					Addresses = new List<Addresses>
					{
						new Addresses
						{
							Name = "Aarau",
							SecurityPet = dog
						}
					},
					OwnsPets = new List<Pet>
					{
						new Cat
						{
							Name = "Catname1",
							ChasesLaser = true
						},
						dog
					}
				}, nameof(SoccerPlayer)); // TODO: We should add an attribute to the class with its schema name instead of passing it as parameter

			var soccerTrainerTree = await _client.ListItems.CreateFromPOCOAsync(
				new SoccerTrainer
				{
					BirthDate = DateTime.Now,
					EmailAddress = "xyyyy@teyyyyyyst.com",
					Firstname = "Urxxxxs",
					LastName = "xxxxxxxx",
					TrainerSince = new DateTime(2000, 1, 1)
				}, nameof(SoccerTrainer));

			var person = await _client.ListItems.CreateFromPOCOAsync(
				new Person
				{
					BirthDate = DateTime.Now,
					EmailAddress = "xyyyy@teyyyyyyst.com",
					Firstname = "Urxxxxs",
					LastName = "xxxxxxxx"
				}, nameof(Person));
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateObjectWithoutHelper()
		{
			// Create SoccerPlayer
			var player = await _client.ListItems.CreateAsync(
				new ListItemCreateRequest
				{
					ContentSchemaId = "SoccerPlayer",
					Content = new SoccerPlayer
					{
						BirthDate = DateTime.Now,
						EmailAddress = "test@test.com",
						Firstname = "Urs",
						LastName = "Brogle"
					}
				});

			Assert.NotNull(player);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldGetObject()
		{
			string objectName = "ThisObjectC" + new Random().Next(0, 999999);

			var createRequest = new ListItemCreateRequest
			{
				ContentSchemaId = nameof(Tag),
				Content = new Tag { Name = objectName }
			};

			ListItemDetail listItem = await _client.ListItems.CreateAsync(createRequest);
			var result = await _client.ListItems.GetAsync(listItem.Id, true);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldGetObjectResolved()
		{
			var request = new SchemaSearchRequest
			{
				Limit = 100,
				Filter = new TermFilter
				{
					Field = "types",
					Term = SchemaType.List.ToString()
				}
			};
			var result = _client.Schemas.Search(request);
			Assert.True(result.Results.Any());

			string objectId = null;

			// Loop over all metadataSchemaIds until an object is found
			// For Debugging: foreach (var item in result.Results.Where(i => i.Id == "Esselabore1"))
			foreach (var item in result.Results)
			{
				objectId = _fixture.GetRandomObjectId(item.Id, 20);

				if (!string.IsNullOrEmpty(objectId))
					break;
			}

			Assert.False(string.IsNullOrEmpty(objectId));
			await _client.ListItems.GetAsync(objectId, true);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldSearchObjects()
		{
			// ---------------------------------------------------------------------------
			// Get a list of MetadataSchemaIds
			// ---------------------------------------------------------------------------
			var searchRequestSchema = new SchemaSearchRequest { Start = 0, Limit = 999, Filter = new TermFilter { Field = "types", Term = SchemaType.List.ToString() } };
			var searchResultSchema = _client.Schemas.Search(searchRequestSchema);
			Assert.True(searchResultSchema.Results.Any());

			List<string> metadataSchemaIds = searchResultSchema.Results.Select(i => i.Id).OrderBy(i => i).ToList();

			var searchRequestObject = new ListItemSearchRequest() { Start = 0, Limit = 100 };
			var items = new List<ListItem>();
			List<string> failedMetadataSchemaIds = new List<string>();

			// ---------------------------------------------------------------------------
			// Loop over all metadataSchemaIds and make a search for each metadataSchemaId
			// ---------------------------------------------------------------------------
			foreach (var metadataSchemaId in metadataSchemaIds)
			{
				searchRequestObject.SchemaIds = new List<string> { metadataSchemaId };

				try
				{
					var searchResultObject = await _client.ListItems.SearchAsync(searchRequestObject);

					if (searchResultObject.Results.Any())
						items.AddRange(searchResultObject.Results);
				}
				catch (Exception)
				{
					failedMetadataSchemaIds.Add(metadataSchemaId);
				}
			}

			Assert.True(!failedMetadataSchemaIds.Any());
			Assert.True(items.Any());
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldUpdateObject()
		{
			// Search players
			var players = await _client.ListItems.SearchAsync(new ListItemSearchRequest
			{
				Limit = 20,
				SearchString = "-ivorejvioe",
				SchemaIds = new List<string> { "SoccerPlayer" }
			});

			Assert.True(players.Results.Any());
			var playerObjectId = players.Results.First().Id;

			var playerItem = await _client.ListItems.GetAsync(playerObjectId, true);

			// Convert first result item to CLR
			var player = playerItem.ConvertToType<SoccerPlayer>(nameof(SoccerPlayer));

			// Update CLR Object
			player.Firstname = "xy jviorej ivorejvioe";

			// Update on server
			await _client.ListItems.UpdateAsync(playerItem, player, nameof(SoccerPlayer));
		}
	}
}