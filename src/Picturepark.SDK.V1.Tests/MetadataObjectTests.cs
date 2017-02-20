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
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests
{
	public class MetadataObjectTests : IClassFixture<MetadataObjectFixture>
	{
		private SDKClientFixture _fixture;
		private PictureparkClient _client;

		public MetadataObjectTests(MetadataObjectFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldAggregateObjects()
		{
			var fieldName = nameof(Country) + "." + nameof(Country.RegionCode);

			var request = new MetadataObjectAggregationRequest()
			{
				MetadataSchemaIds = new List<string> { nameof(Country) },
				Aggregators = new List<AggregatorBase>
				{
					{ new TermsAggregator { Name = fieldName, Field = fieldName, Size = 20 } }
				},
				SearchString = "*"
			};

			ObjectAggregationResult result = await _client.MetadataObjects.AggregateAsync(request);

			var aggregation = result.GetByName(fieldName);

			Assert.NotNull(aggregation);

			Assert.Equal(aggregation.AggregationResultItems.Count, 20);
		}

		[Fact]
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldCreateAndDeleteObject()
		{
			string objectName = "ThisObjectA" + new Random().Next(0, 999999).ToString();

			var createRequest = new MetadataObjectCreateRequest
			{
				MetadataSchemaId = nameof(Tag),
				Metadata = new MetadataDictionary
				{
					{ "Tag", new Tag { Name = objectName } }
				}
			};

			MetadataObjectViewItem viewItem = await _client.MetadataObjects.CreateAbcAsync(createRequest);
			Assert.False(string.IsNullOrEmpty(viewItem.Id));

			await _client.MetadataObjects.DeleteAsync(viewItem.Id);

			// TODO: Throw specific 404 exception
			await Assert.ThrowsAsync<ApiException>(async () => await _client.MetadataObjects.GetAsync(viewItem.Id));
		}

		[Fact]
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldCreateAndUpdateObject()
		{
			// ---------------------------------------------------------------------------
			// Create Object
			// ---------------------------------------------------------------------------
			string objectName = "ThisObjectD" + new Random().Next(0, 999999).ToString();

			var objects = new List<MetadataObjectCreateRequest>()
			{
				new MetadataObjectCreateRequest
				{
					MetadataSchemaId = nameof(Tag),
					Metadata = new MetadataDictionary
					{
						{ "Tag", new Tag { Name = objectName } }
					}
				}
			};

			IEnumerable<MetadataObjectViewItem> results = await _client.MetadataObjects.CreateManyAsync(objects);
			Assert.Equal(results.Count(), 1);

			var result = results.First();

			// Update object, assign MetadataSchemaIds
			var request = new MetadataObjectUpdateRequest()
			{
				Id = result.Id,
				Metadata = new MetadataDictionary
				{
					{ "Tag", new Tag { Name = objectName } }
				}
			};

			var requests = new List<MetadataObjectUpdateRequest>() { request };

			await _client.MetadataObjects.UpdateMetadataObjectAsync(request);
		}

		[Fact]
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldCreateObject()
		{
			string objectName = "ThisObjectB" + new Random().Next(0, 999999).ToString();

			var metadataObject = new MetadataObjectCreateRequest
			{
				MetadataSchemaId = nameof(Tag),
				Metadata = new MetadataDictionary
				{
					{ "Tag", new Tag { Name = objectName } }
				}
			};

			MetadataObjectDetailViewItem result = await _client.MetadataObjects.CreateAsync(metadataObject);
			Assert.False(string.IsNullOrEmpty(result.Id));
		}

		[Fact]
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldCreateObjectWithHelper()
		{
			// Using Helper method
			var createdObject = await _client.MetadataObjects.CreateFromPOCO(
				new Tag
				{
					Name = "ThisObjectB" + new Random().Next(0, 999999).ToString()
				}, nameof(Tag));
		}

		[Fact]
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldCreateComplexObjectWithHelper()
		{
			// Reusable as reference
			var dog = new Dog { Name = "Dogname1", PlaysCatch = true };

			// Using Helper method
			var soccerPlayerTree = await _client.MetadataObjects.CreateFromPOCO(
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

			var soccerTrainerTree = await _client.MetadataObjects.CreateFromPOCO(
				new SoccerTrainer
				{
					BirthDate = DateTime.Now,
					EmailAddress = "xyyyy@teyyyyyyst.com",
					Firstname = "Urxxxxs",
					LastName = "xxxxxxxx",
					TrainerSince = new DateTime(2000, 1, 1)
				}, nameof(SoccerTrainer));

			var person = await _client.MetadataObjects.CreateFromPOCO(
				new Person
				{
					BirthDate = DateTime.Now,
					EmailAddress = "xyyyy@teyyyyyyst.com",
					Firstname = "Urxxxxs",
					LastName = "xxxxxxxx"
				}, nameof(Person));
		}

		[Fact]
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldCreateObjectWithoutHelper()
		{
			// Create SoccerPlayer
			var createRequest = await _client.MetadataObjects.CreateAsync(
				new MetadataObjectCreateRequest
				{
					MetadataSchemaId = "SoccerPlayer",
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
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldGetObject()
		{
			string objectName = "ThisObjectC" + new Random().Next(0, 999999).ToString();

			var createRequest = new MetadataObjectCreateRequest
			{
				MetadataSchemaId = nameof(Tag),
				Metadata = new MetadataDictionary
				{
					{ "Tag", new Tag { Name = objectName } }
				}
			};

			MetadataObjectViewItem viewItem = await _client.MetadataObjects.CreateAbcAsync(createRequest);
			var result = await _client.MetadataObjects.GetAsync(viewItem.Id);
		}

		[Fact]
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldGetObjectResolved()
		{
			var request = new MetadataSchemaSearchRequest()
			{
				Limit = 100,
				Filter = new TermFilter()
				{
					Field = "Types",
					Term = MetadataSchemaType.MetadataContent.ToString()
				}
			};
			BaseResultOfMetadataSchemaViewItem result = _client.Schemas.Search(request);
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
			await _client.MetadataObjects.GetResolvedAsync(objectId);
		}

		[Fact]
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldSearchObjects()
		{
			// ---------------------------------------------------------------------------
			// Get a list of MetadataSchemaIds
			// ---------------------------------------------------------------------------
			var searchRequestSchema = new MetadataSchemaSearchRequest() { Start = 0, Limit = 999, Filter = new TermFilter() { Field = "Types", Term = MetadataSchemaType.MetadataContent.ToString() } };
			BaseResultOfMetadataSchemaViewItem searchResultSchema = _client.Schemas.Search(searchRequestSchema);
			Assert.True(searchResultSchema.Results.Count() > 0);

			List<string> metadataSchemaIds = searchResultSchema.Results.Select(i => i.Id).OrderBy(i => i).ToList();

			var searchRequestObject = new MetadataObjectSearchRequest() { Start = 0, Limit = 100 };
			var viewItems = new List<MetadataObjectViewItem>();
			List<string> failedMetadataSchemaIds = new List<string>();
			BaseResultOfMetadataObjectViewItem searchResultObject;

			// ---------------------------------------------------------------------------
			// Loop over all metadataSchemaIds and make a search for each metadataSchemaId
			// ---------------------------------------------------------------------------
			foreach (string metadataSchemaId in metadataSchemaIds)
			{
				searchRequestObject.MetadataSchemaIds = new List<string> { metadataSchemaId };

				try
				{
					searchResultObject = await _client.MetadataObjects.SearchAsync(searchRequestObject);

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
		[Trait("Stack", "MetadataObject")]
		public async Task ShouldUpdateObject()
		{
			// Search players
			var players = await _client.MetadataObjects.SearchAsync(new MetadataObjectSearchRequest
			{
				Limit = 20,
				SearchString = "-ivorejvioe",
				MetadataSchemaIds = new List<string> { "SoccerPlayer" }
			});

			Assert.True(players.Results.Count() > 0);
			var playerObjectId = players.Results.First().Id;

			var playerViewItem = await _client.MetadataObjects.GetResolvedAsync(playerObjectId);

			// Convert first result item to CLR
			var player = playerViewItem.ConvertToType<SoccerPlayer>(nameof(SoccerPlayer));

			// Update CLR Object
			player.Firstname = "xy jviorej ivorejvioe";

			// Update on server
			await _client.MetadataObjects.UpdateMetadataObjectAsync(playerViewItem, player, nameof(SoccerPlayer));
		}

		[Fact]
		[Trait("Stack", "Metadata")]
		public async Task ShouldImport()
		{
			string jsonFilePath = Path.GetFullPath("ExampleData/Corporate.json");
			await _client.MetadataObjects.ImportFromJsonAsync(jsonFilePath, includeObjects: false);
		}
	}
}