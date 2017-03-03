using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Contract;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.IO;
using Picturepark.SDK.V1.Contract.Extensions;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests
{
	public class ListItemTests : IClassFixture<ListItemFixture>
	{
		private SDKClientFixture _fixture;
		private PictureparkClient _client;

		public ListItemTests(ListItemFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldAggregateObjects()
		{
			var fieldName = nameof(Country) + "." + nameof(Country.RegionCode);

			var request = new ListItemAggregationRequest()
			{
				SchemaIds = new List<string> { nameof(Country) },
				Aggregators = new List<AggregatorBase>
				{
					{ new TermsAggregator { Name = fieldName, Field = fieldName, Size = 20 } }
				},
				SearchString = "*"
			};

			ObjectAggregationResult result = await _client.ListItems.AggregateAsync(request);

			var aggregation = result.GetByName(fieldName);

			Assert.NotNull(aggregation);

			Assert.Equal(aggregation.AggregationResultItems.Count, 20);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateAndDeleteObject()
		{
			string objectName = "ThisObjectA" + new Random().Next(0, 999999).ToString();

			var createRequest = new ListItemCreateRequest
			{
				SchemaId = nameof(Tag),
				Metadata = new MetadataDictionary
				{
					{ "Tag", new Tag { Name = objectName } }
				}
			};

			ListItemViewItem viewItem = await _client.ListItems.CreateAbcAsync(createRequest);
			Assert.False(string.IsNullOrEmpty(viewItem.Id));

			await _client.ListItems.DeleteAsync(viewItem.Id);

			// TODO: Throw specific 404 exception
			await Assert.ThrowsAsync<ApiException>(async () => await _client.ListItems.GetAsync(viewItem.Id, true));
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateAndUpdateObject()
		{
			// ---------------------------------------------------------------------------
			// Create Object
			// ---------------------------------------------------------------------------
			string objectName = "ThisObjectD" + new Random().Next(0, 999999).ToString();

			var objects = new List<ListItemCreateRequest>()
			{
				new ListItemCreateRequest
				{
					SchemaId = nameof(Tag),
					Metadata = new MetadataDictionary
					{
						{ "Tag", new Tag { Name = objectName } }
					}
				}
			};

			IEnumerable<ListItemViewItem> results = await _client.ListItems.CreateManyAsync(objects);
			Assert.Equal(results.Count(), 1);

			var result = results.First();

			// Update object, assign MetadataSchemaIds
			var request = new ListItemUpdateRequest()
			{
				Id = result.Id,
				Metadata = new MetadataDictionary
				{
					{ "Tag", new Tag { Name = objectName } }
				}
			};

			var requests = new List<ListItemUpdateRequest>() { request };

			await _client.ListItems.UpdateListItemAsync(request);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateObject()
		{
			string objectName = "ThisObjectB" + new Random().Next(0, 999999).ToString();

			var listItem = new ListItemCreateRequest
			{
				SchemaId = nameof(Tag),
				Metadata = new MetadataDictionary
				{
					{ "Tag", new Tag { Name = objectName } }
				}
			};

			ListItemDetailViewItem result = await _client.ListItems.CreateAsync(listItem);
			Assert.False(string.IsNullOrEmpty(result.Id));
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateObjectWithHelper()
		{
			// Using Helper method
			var createdObject = await _client.ListItems.CreateFromPOCO(
				new Tag
				{
					Name = "ThisObjectB" + new Random().Next(0, 999999).ToString()
				}, nameof(Tag));
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldCreateComplexObjectWithHelper()
		{
			// Reusable as reference
			var dog = new Dog { Name = "Dogname1", PlaysCatch = true };

			// Using Helper method
			var soccerPlayerTree = await _client.ListItems.CreateFromPOCO(
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
						new Cat { Name = "Catname1", ChasesLaser = true },
						dog
					}
				}, nameof(SoccerPlayer));

			var soccerTrainerTree = await _client.ListItems.CreateFromPOCO(
				new SoccerTrainer
				{
					BirthDate = DateTime.Now,
					EmailAddress = "xyyyy@teyyyyyyst.com",
					Firstname = "Urxxxxs",
					LastName = "xxxxxxxx",
					TrainerSince = new DateTime(2000, 1, 1)
				}, nameof(SoccerTrainer));

			var person = await _client.ListItems.CreateFromPOCO(
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
			var createRequest = await _client.ListItems.CreateAsync(
				new ListItemCreateRequest
				{
					SchemaId = "SoccerPlayer",
					Metadata = new MetadataDictionary
					{
						{
							"SoccerPlayer",
							new SoccerPlayer
							{
								BirthDate = DateTime.Now,
								EmailAddress = "test@test.com",
								Firstname = "Urs",
								LastName = "Brogle"
							}
						}
					}
				});
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldGetObject()
		{
			string objectName = "ThisObjectC" + new Random().Next(0, 999999).ToString();

			var createRequest = new ListItemCreateRequest
			{
				SchemaId = nameof(Tag),
				Metadata = new MetadataDictionary
				{
					{ "Tag", new Tag { Name = objectName } }
				}
			};

			ListItemViewItem viewItem = await _client.ListItems.CreateAbcAsync(createRequest);
			var result = await _client.ListItems.GetAsync(viewItem.Id, true);
		}

		[Fact]
		[Trait("Stack", "ListItem")]
		public async Task ShouldGetObjectResolved()
		{
			var request = new SchemaSearchRequest()
			{
				Limit = 100,
				Filter = new TermFilter()
				{
					Field = "Types",
					Term = SchemaType.List.ToString()
				}
			};
			BaseResultOfSchemaViewItem result = _client.Schemas.Search(request);
			Assert.True(result.Results.Count() > 0);

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
			var searchRequestSchema = new SchemaSearchRequest() { Start = 0, Limit = 999, Filter = new TermFilter() { Field = "Types", Term = SchemaType.List.ToString() } };
			BaseResultOfSchemaViewItem searchResultSchema = _client.Schemas.Search(searchRequestSchema);
			Assert.True(searchResultSchema.Results.Count() > 0);

			List<string> metadataSchemaIds = searchResultSchema.Results.Select(i => i.Id).OrderBy(i => i).ToList();

			var searchRequestObject = new ListItemSearchRequest() { Start = 0, Limit = 100 };
			var viewItems = new List<ListItemViewItem>();
			List<string> failedMetadataSchemaIds = new List<string>();
			BaseResultOfListItemViewItem searchResultObject;

			// ---------------------------------------------------------------------------
			// Loop over all metadataSchemaIds and make a search for each metadataSchemaId
			// ---------------------------------------------------------------------------
			foreach (string metadataSchemaId in metadataSchemaIds)
			{
				searchRequestObject.SchemaIds = new List<string> { metadataSchemaId };

				try
				{
					searchResultObject = await _client.ListItems.SearchAsync(searchRequestObject);

					if (searchResultObject.Results.Count() > 0)
						viewItems.AddRange(searchResultObject.Results);
				}
				catch (Exception ex)
				{
					if (ex.Message.Length == 1234)
						break;

					failedMetadataSchemaIds.Add(metadataSchemaId);
				}
			}

			Assert.True(failedMetadataSchemaIds.Count() == 0);
			Assert.True(viewItems.Count() > 0);
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

			Assert.True(players.Results.Count() > 0);
			var playerObjectId = players.Results.First().Id;

			var playerViewItem = await _client.ListItems.GetAsync(playerObjectId, true);

			// Convert first result item to CLR
			var player = playerViewItem.ConvertToType<SoccerPlayer>(nameof(SoccerPlayer));

			// Update CLR Object
			player.Firstname = "xy jviorej ivorejvioe";

			// Update on server
			await _client.ListItems.UpdateListItemAsync(playerViewItem, player, nameof(SoccerPlayer));
		}

		[Fact(Skip = "Broken")] // TODO: Fix
		[Trait("Stack", "Metadata")]
		public async Task ShouldImport()
		{
			string jsonFilePath = Path.GetFullPath(_fixture.ProjectDirectory + "ExampleData/Corporate.json");
			await _client.ListItems.ImportFromJsonAsync(jsonFilePath, includeObjects: false);
		}
	}
}