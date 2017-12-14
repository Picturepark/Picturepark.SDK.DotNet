using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Extensions;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
	public class ListItemTests : IClassFixture<ListItemFixture>
	{
		private readonly ClientFixture _fixture;
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
			Assert.Equal("Foo", newItem.ConvertTo<Tag>(nameof(Tag)).Name);
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
		[Trait("Stack", "ListItem")]
		public async Task ShouldBatchUpdateFieldsByFilter()
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
				ListItemFilterRequest = new ListItemFilterRequest // TODO: ListItemFieldsFilterUpdateRequest.ListItemFilterRequest: Rename property to FilterRequest?
				{
					Filter = new TermFilter { Field = "id", Term = listItemDetail.Id }
				},
				ChangeCommands = new List<MetadataValuesChangeCommandBase>
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
			var businessProcess = await _client.ListItems.BatchUpdateFieldsByFilterAsync(updateRequest);
			var waitResult = await _client.BusinessProcesses.WaitForCompletionAsync(businessProcess.Id, TimeSpan.FromSeconds(10));

			ListItemDetail result = await _client.ListItems.GetAsync(listItemDetail.Id, true);

			/// Assert
			Assert.Equal("Foo", result.ConvertTo<Tag>(nameof(Tag)).Name);
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
			var createdObjects = await _client.ListItems.CreateFromObjectAsync(tag, nameof(Tag));

			/// Assert
			Assert.Equal(1, createdObjects.Count());
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateComplexObjectWithHelper()
		{
			/// Arrange
			// Reusable as reference
			var dog = new Dog
			{
				Name = "Dogname1",
				PlaysCatch = true
			};

			/// Act
			// Using Helper method
			var soccerPlayerTree = await _client.ListItems.CreateFromObjectAsync(
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
				}, nameof(SoccerPlayer)); // TODO: ListItemClient.CreateFromPOCOAsync: We should add an attribute to the class with its schema name instead of passing it as parameter

			var soccerTrainerTree = await _client.ListItems.CreateFromObjectAsync(
				new SoccerTrainer
				{
					BirthDate = DateTime.Now,
					EmailAddress = "xyyyy@teyyyyyyst.com",
					Firstname = "Urxxxxs",
					LastName = "xxxxxxxx",
					TrainerSince = new DateTime(2000, 1, 1)
				}, nameof(SoccerTrainer));

			var personTree = await _client.ListItems.CreateFromObjectAsync(
				new Person
				{
					BirthDate = DateTime.Now,
					EmailAddress = "xyyyy@teyyyyyyst.com",
					Firstname = "Urxxxxs",
					LastName = "xxxxxxxx"
				}, nameof(Person));

			/// Assert
			Assert.True(soccerPlayerTree.Any());
			Assert.True(soccerTrainerTree.Any());
			Assert.True(personTree.Any());
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateObjectWithoutHelper()
		{
			/// Arrange
			var originalPlayer = new SoccerPlayer
			{
				BirthDate = DateTime.Now,
				EmailAddress = "test@test.com",
				Firstname = "Urs",
				LastName = "Brogle"
			};

			/// Act
			var createRequest = new ListItemCreateRequest
			{
				ContentSchemaId = "SoccerPlayer",
				Content = originalPlayer
			};

			var playerItem = await _client.ListItems.CreateAsync(createRequest);

			/// Assert
			Assert.NotNull(playerItem);

			var createdPlayer = playerItem.ConvertTo<SoccerPlayer>("SoccerPlayer");
			Assert.Equal("Urs", createdPlayer.Firstname);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldGetObject()
		{
			/// Arrange
			var objectName = "ThisObjectC" + new Random().Next(0, 999999);

			/// Act
			var createRequest = new ListItemCreateRequest
			{
				ContentSchemaId = nameof(Tag),
				Content = new Tag { Name = objectName }
			};

			var listItem = await _client.ListItems.CreateAsync(createRequest);
			var result = await _client.ListItems.GetAsync(listItem.Id, true);

			/// Assert
			Assert.NotNull(result);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldGetObjectResolved()
		{
			/// Arrange
			var request = new SchemaSearchRequest
			{
				Limit = 100,
				Filter = new TermFilter
				{
					Field = "types",
					Term = SchemaType.List.ToString()
				}
			};

			var result = await _client.Schemas.SearchAsync(request);
			Assert.True(result.Results.Any());

			// For debugging: .Where(i => i.Id == "Esselabore1"))
			var tuple = result.Results
				.Select(i => new { schemaId = i.Id, objectId = _fixture.GetRandomObjectId(i.Id, 20) })
				.First(i => !string.IsNullOrEmpty(i.objectId));

			/// Act
			var listItem = await _client.ListItems.GetAsync(tuple.objectId, true);

			/// Assert
			Assert.Equal(tuple.schemaId, listItem.ContentSchemaId);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldSearchObjects()
		{
			/// Arrange
			// ---------------------------------------------------------------------------
			// Get a list of MetadataSchemaIds
			// ---------------------------------------------------------------------------
			var searchRequestSchema = new SchemaSearchRequest
			{
				Start = 0,
				Limit = 999,
				Filter = new TermFilter
				{
					Field = "types",
					Term = SchemaType.List.ToString()
				}
			};

			var searchResultSchema = await _client.Schemas.SearchAsync(searchRequestSchema);
			Assert.True(searchResultSchema.Results.Any());

			List<string> metadataSchemaIds = searchResultSchema.Results
				.Select(i => i.Id)
				.OrderBy(i => i)
				.ToList();

			var searchRequestObject = new ListItemSearchRequest() { Start = 0, Limit = 100 };
			var items = new List<ListItem>();
			List<string> failedMetadataSchemaIds = new List<string>();

			/// Act
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
					{
						items.AddRange(searchResultObject.Results);
					}
				}
				catch (Exception)
				{
					failedMetadataSchemaIds.Add(metadataSchemaId);
				}
			}

			/// Assert
			Assert.True(!failedMetadataSchemaIds.Any());
			Assert.True(items.Any());
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldUpdate()
		{
			/// Arrange
			var players = await _client.ListItems.SearchAsync(new ListItemSearchRequest
			{
				Limit = 20,
				SearchString = "-ivorejvioe",
				SchemaIds = new List<string> { "SoccerPlayer" }
			});

			var playerObjectId = players.Results.First().Id;
			var playerItem = await _client.ListItems.GetAsync(playerObjectId, true);

			/// Act
			var player = playerItem.ConvertTo<SoccerPlayer>(nameof(SoccerPlayer));
			player.Firstname = "xy jviorej ivorejvioe";

			await _client.ListItems.UpdateAsync(playerItem.Id, player);
			var updatedPlayer = await _client.ListItems.GetAndConvertToAsync<SoccerPlayer>(playerItem.Id, nameof(SoccerPlayer));

			/// Assert
			Assert.Equal(player.Firstname, updatedPlayer.Firstname);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldUpdateMany()
		{
			/// Arrange
			var originalPlayer = new SoccerPlayer
			{
				BirthDate = DateTime.Now,
				EmailAddress = "test@test.com",
				Firstname = "Test",
				LastName = "Soccerplayer"
			};

			var createRequest = new ListItemCreateRequest
			{
				ContentSchemaId = "SoccerPlayer",
				Content = originalPlayer
			};

			var playerItem = await _client.ListItems.CreateAsync(createRequest);

			/// Act
			var player = playerItem.ConvertTo<SoccerPlayer>(nameof(SoccerPlayer));
			player.Firstname = "xy jviorej ivorejvioe";

			var businessProcess = await _client.ListItems.UpdateManyAsync(new[]
			{
				new ListItemUpdateRequest { Id = playerItem.Id, Content = player }
			});

			await _client.BusinessProcesses.WaitForCompletionAsync(businessProcess.Id);
			var updatedPlayer = await _client.ListItems.GetAndConvertToAsync<SoccerPlayer>(playerItem.Id, nameof(SoccerPlayer));

			/// Assert
			Assert.Equal(player.Firstname, updatedPlayer.Firstname);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldTrashAndUntrashListItem()
		{
			/// Arrange
			var listItem = await _client.ListItems.CreateAsync(new ListItemCreateRequest
			{
				ContentSchemaId = nameof(Tag),
				Content = new Tag { Name = "ShouldTrashAndUntrashRandomListItem" }
			});
			var listItemId = listItem.Id;

			Assert.False(string.IsNullOrEmpty(listItemId));
			var listItemDetail = await _client.ListItems.GetAsync(listItemId, true);

			/// Act
			// Deactivate
			await _client.ListItems.DeactivateAsync(listItemId, new TimeSpan(0, 2, 0));
			await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItems.GetAsync(listItemId, false));

			// Reactivate
			var reactivatedListItem = await _client.ListItems.ReactivateAsync(listItemId, new TimeSpan(0, 2, 0));

			/// Assert
			Assert.True(reactivatedListItem != null);
			Assert.NotNull(await _client.ListItems.GetAsync(listItemId, true));
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldTrashAndUntrashListItemMany()
		{
			/// Arrange
			var listItem1 = await _client.ListItems.CreateAsync(new ListItemCreateRequest
			{
				ContentSchemaId = nameof(Tag),
				Content = new Tag { Name = "ShouldTrashAndUntrashRandomListItemMany1" }
			});

			var listItem2 = await _client.ListItems.CreateAsync(new ListItemCreateRequest
			{
				ContentSchemaId = nameof(Tag),
				Content = new Tag { Name = "ShouldTrashAndUntrashRandomListItemMany2" }
			});

			var listItemDetail1 = await _client.ListItems.GetAsync(listItem1.Id, true);

			var listItemDetail2 = await _client.ListItems.GetAsync(listItem2.Id, true);

			/// Act
			// Deactivate
			var deactivateRequest = new ListItemDeactivateRequest
			{
				ListItemIds = new List<string> { listItem1.Id, listItem2.Id }
			};

			var businessProcess = await _client.ListItems.DeactivateManyAsync(deactivateRequest);
			await _client.BusinessProcesses.WaitForCompletionAsync(businessProcess.Id);

			await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItems.GetAsync(listItem1.Id, false));
			await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItems.GetAsync(listItem2.Id, false));

			// Reactivate
			var reactivateRequest = new ListItemReactivateRequest
			{
				ListItemIds = new List<string> { listItem1.Id, listItem2.Id }
			};

			businessProcess = await _client.ListItems.ReactivateManyAsync(reactivateRequest);
			await _client.BusinessProcesses.WaitForCompletionAsync(businessProcess.Id);

			/// Assert
			Assert.NotNull(await _client.ListItems.GetAsync(listItem1.Id, true));
			Assert.NotNull(await _client.ListItems.GetAsync(listItem2.Id, true));
		}
	}
}