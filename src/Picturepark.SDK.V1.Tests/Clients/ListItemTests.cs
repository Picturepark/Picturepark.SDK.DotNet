using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Localization;
using Picturepark.SDK.V1.Tests.Fixtures;
using Picturepark.SDK.V1.Tests.FluentAssertions;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class ListItemTests : IClassFixture<ListItemFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public ListItemTests(ListItemFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldAggregateObjects()
        {
            // Arrange
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

            // Act
            ObjectAggregationResult result = await _client.ListItem.AggregateAsync(request).ConfigureAwait(false);
            AggregationResult aggregation = result.GetByName(fieldName);

            // Assert
            Assert.NotNull(aggregation);
            Assert.Equal(20, aggregation.AggregationResultItems.Count);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldSearchAndAggregateObjectsAllTogether()
        {
            // Arrange
            var fieldName = nameof(Country).ToLowerCamelCase() + "." + nameof(Country.RegionCode).ToLowerCamelCase();
            var request = new ListItemSearchRequest
            {
                SearchString = "*",
                SchemaIds = new List<string> { nameof(Country) },
                Aggregators = new List<AggregatorBase>
                {
                    new TermsAggregator { Name = fieldName, Field = fieldName, Size = 20 }
                }
            };

            // Act
            var result = await _client.ListItem.SearchAsync(request).ConfigureAwait(false);

            // Assert
            result.Results.Should().HaveCountGreaterThan(0).And.Subject.Should().OnlyContain(li => li.ContentSchemaId == nameof(Country));

            AggregationResult aggregationResult = result.AggregationResults.SingleOrDefault(i => i.Name == fieldName);
            aggregationResult.Should().NotBeNull();
            aggregationResult.AggregationResultItems.Should().HaveCount(20);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateAndUpdateObject()
        {
            // Arrange
            var objectName = "ThisObjectD" + new Random().Next(0, 999999);
            var createManyRequest = new ListItemCreateManyRequest()
            {
                AllowMissingDependencies = false,
                Items = new List<ListItemCreateRequest>
                {
                    new ListItemCreateRequest
                    {
                        ContentSchemaId = nameof(Tag),
                        Content = new Tag { Name = objectName }
                    }
                }
            };

            var results = await _client.ListItem.CreateManyAsync(createManyRequest).ConfigureAwait(false);
            var detail = await results.FetchDetail().ConfigureAwait(false);

            var itemId = detail.SucceededIds.First();

            // Act
            var request = new ListItemUpdateRequest
            {
                Content = new Tag { Name = "Foo" }
            };
            await _client.ListItem.UpdateAsync(itemId, request).ConfigureAwait(false);

            // Assert
            var newItem = await _client.ListItem.GetAsync(itemId, new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);
            Assert.Equal("Foo", newItem.ConvertTo<Tag>().Name);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreate()
        {
            // Arrange
            var objectName = "ThisObjectB" + new Random().Next(0, 999999);
            var listItem = new ListItemCreateRequest
            {
                ContentSchemaId = nameof(Tag),
                Content = new Tag { Name = objectName }
            };

            // Act
            ListItemDetail result = await _client.ListItem.CreateAsync(listItem).ConfigureAwait(false);

            // Assert
            Assert.False(string.IsNullOrEmpty(result.Id));

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldBatchUpdateFieldsByFilter()
        {
            // Arrange
            var objectName = "ThisObjectB" + new Random().Next(0, 999999);
            var listItem = new ListItemCreateRequest
            {
                ContentSchemaId = nameof(Tag),
                Content = new Tag { Name = objectName }
            };
            ListItemDetail listItemDetail = await _client.ListItem.CreateAsync(listItem).ConfigureAwait(false);

            // Act
            var updateRequest = new ListItemFieldsBatchUpdateFilterRequest
            {
                FilterRequest = new ListItemFilterRequest
                {
                    Filter = new TermFilter { Field = "id", Term = listItemDetail.Id }
                },
                ChangeCommands = new List<MetadataValuesChangeCommandBase>
                {
                    new MetadataValuesSchemaUpdateCommand
                    {
                        SchemaId = nameof(Tag),
                        Value = new
                        {
                            name = "Foo"
                        }
                    }
                }
            };

            // Act
            await _client.ListItem.BatchUpdateFieldsByFilterAsync(updateRequest).ConfigureAwait(false);
            ListItemDetail result = await _client.ListItem.GetAsync(listItemDetail.Id, new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            // Assert
            Assert.Equal("Foo", result.ConvertTo<Tag>().Name);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateWithHelper()
        {
            // Arrange
            var tag = new Tag
            {
                Name = "ThisObjectB" + new Random().Next(0, 999999)
            };

            // Act
            var createResult = await _client.ListItem.CreateFromObjectAsync(tag).ConfigureAwait(false);
            var createDetail = await createResult.FetchDetail().ConfigureAwait(false);

            // Assert
            Assert.Single(createDetail.SucceededItems);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateComplexObjectWithHelper()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client).ConfigureAwait(false);

            // Reusable as reference
            var dog = new Dog
            {
                Name = "Dogname1",
                PlaysCatch = true
            };

            // Act
            // Using Helper method
            var soccerPlayerResult = await _client.ListItem.CreateFromObjectAsync(
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
                }).ConfigureAwait(false);

            var soccerPlayerDetail = await soccerPlayerResult.FetchDetail().ConfigureAwait(false);

            var soccerTrainerResult = await _client.ListItem.CreateFromObjectAsync(
                new SoccerTrainer
                {
                    BirthDate = DateTime.Now,
                    EmailAddress = "xyyyy@teyyyyyyst.com",
                    Firstname = "Urxxxxs",
                    LastName = "xxxxxxxx",
                    TrainerSince = new DateTime(2000, 1, 1)
                }).ConfigureAwait(false);

            var soccerTrainerDetail = await soccerTrainerResult.FetchDetail().ConfigureAwait(false);

            var personResult = await _client.ListItem.CreateFromObjectAsync(
                new Person
                {
                    BirthDate = DateTime.Now,
                    EmailAddress = "xyyyy@teyyyyyyst.com",
                    Firstname = "Urxxxxs",
                    LastName = "xxxxxxxx"
                }).ConfigureAwait(false);

            var personDetail = await personResult.FetchDetail().ConfigureAwait(false);

            // Assert
            soccerPlayerDetail.SucceededItems.Should().NotBeEmpty();
            soccerTrainerDetail.SucceededItems.Should().NotBeEmpty();
            personDetail.SucceededItems.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateObjectWithoutHelper()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client).ConfigureAwait(false);

            var originalPlayer = new SoccerPlayer
            {
                BirthDate = DateTime.Now,
                EmailAddress = "test@test.com",
                Firstname = "Urs",
                LastName = "Brogle"
            };

            // Act
            var createRequest = new ListItemCreateRequest
            {
                ContentSchemaId = nameof(SoccerPlayer),
                Content = originalPlayer
            };

            var playerItem = await _client.ListItem.CreateAsync(createRequest, new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            // Assert
            Assert.NotNull(playerItem);

            var createdPlayer = playerItem.ConvertTo<SoccerPlayer>();
            Assert.Equal("Urs", createdPlayer.Firstname);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldGetObject()
        {
            // Arrange
            var objectName = "ThisObjectC" + new Random().Next(0, 999999);

            // Act
            var createRequest = new ListItemCreateRequest
            {
                ContentSchemaId = nameof(Tag),
                Content = new Tag { Name = objectName }
            };

            var listItem = await _client.ListItem.CreateAsync(createRequest).ConfigureAwait(false);
            var result = await _client.ListItem.GetAsync(listItem.Id).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldGetObjectResolved()
        {
            // Arrange
            var countrySchemaId = SchemaFixture.CountrySchemaId;

            var chSearch = await _client.ListItem.SearchAsync(new ListItemSearchRequest
            {
                Filter = FilterBase.FromExpression<Country>(c => c.Name, "Switzerland"),
                SchemaIds = new[] { countrySchemaId }
            }).ConfigureAwait(false);

            chSearch.Results.Should().NotBeEmpty("Switzerland should exist");

            // Act
            var listItem = await _client.ListItem.GetAsync(chSearch.Results.First().Id, new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems }).ConfigureAwait(false);

            // Assert
            listItem.ContentSchemaId.Should().Be(countrySchemaId);

            var region = ((JObject)listItem.Content)["regions"].First();
            region["name"].Should().NotBeNull("the regions should be resolved");
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldGetManyObjects()
        {
            // Arrange
            var objectName1 = "ThisObjectC" + new Random().Next(0, 999999);
            var objectName2 = "ThisObjectC" + new Random().Next(0, 999999);

            var createRequest = new ListItemCreateManyRequest()
            {
                Items = new List<ListItemCreateRequest>
                {
                    new ListItemCreateRequest
                    {
                        ContentSchemaId = nameof(Tag),
                        Content = new Tag { Name = objectName1 }
                    },
                    new ListItemCreateRequest
                    {
                        ContentSchemaId = nameof(Tag),
                        Content = new Tag { Name = objectName2 }
                    }
                }
            };

            var createResult = await _client.ListItem.CreateManyAsync(createRequest).ConfigureAwait(false);
            var createDetail = await createResult.FetchDetail().ConfigureAwait(false);
            var createdListItemIds = createDetail.SucceededIds;

            // Act
            var resultListItems = await _client.ListItem.GetManyAsync(createdListItemIds, new List<ListItemResolveBehavior> { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            // Assert
            resultListItems.Should().NotBeNull().And.HaveCount(2);
            resultListItems.Select(li => li.Id).Should().BeEquivalentTo(createdListItemIds.ElementAt(0), createdListItemIds.ElementAt(1));
            resultListItems.Select(li => li.Content.As<JObject>()["name"].ToString()).Should().BeEquivalentTo(objectName1, objectName2);

            resultListItems.ToList().ForEach(listItem =>
                {
                    listItem.Audit.CreatedByUser.Should().BeResolved();
                    listItem.Audit.ModifiedByUser.Should().BeResolved();
                }
            );
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldGetManyObjectsConverted()
        {
            // Arrange
            var objectName1 = "ThisObjectC" + new Random().Next(0, 999999);
            var objectName2 = "ThisObjectC" + new Random().Next(0, 999999);

            var createRequest = new ListItemCreateManyRequest()
            {
                Items = new List<ListItemCreateRequest>
                {
                    new ListItemCreateRequest
                    {
                        ContentSchemaId = nameof(Tag),
                        Content = new Tag { Name = objectName1 }
                    },
                    new ListItemCreateRequest
                    {
                        ContentSchemaId = nameof(Tag),
                        Content = new Tag { Name = objectName2 }
                    }
                }
            };

            var createResult = await _client.ListItem.CreateManyAsync(createRequest).ConfigureAwait(false);
            var createDetail = await createResult.FetchDetail().ConfigureAwait(false);

            // Act
            var resultListItems = await _client.ListItem.GetManyAndConvertToAsync<Tag>(createDetail.SucceededIds, nameof(Tag)).ConfigureAwait(false);

            // Assert
            resultListItems.Should().NotBeNull().And.HaveCount(2);
            resultListItems.Select(li => li.Name).Should().BeEquivalentTo(objectName1, objectName2);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldSearchListItemsAndScrollThroughResults()
        {
            // Arrange
            // ---------------------------------------------------------------------------
            // Get a list of MetadataSchemaIds
            // ---------------------------------------------------------------------------
            var searchRequestSchema = new SchemaSearchRequest
            {
                Limit = 2,
                Filter = FilterBase.FromExpression<Schema>(i => i.Types, SchemaType.List.ToString())
            };

            var searchResultSchema = await _client.Schema.SearchAsync(searchRequestSchema).ConfigureAwait(false);
            Assert.True(searchResultSchema.Results.Any());

            List<string> metadataSchemaIds = searchResultSchema.Results
                .Select(i => i.Id)
                .OrderBy(i => i)
                .ToList();

            var searchRequestObject = new ListItemSearchRequest() { Limit = 100 };
            var items = new List<ListItem>();
            List<string> failedMetadataSchemaIds = new List<string>();

            // Act
            // ---------------------------------------------------------------------------
            // Loop over all metadataSchemaIds and make a search for each metadataSchemaId
            // ---------------------------------------------------------------------------
            foreach (var metadataSchemaId in metadataSchemaIds)
            {
                searchRequestObject.SchemaIds = new List<string> { metadataSchemaId };
                searchRequestObject.PageToken = null;

                try
                {
                    int i = 0;
                    ListItemSearchResult searchResultObject;

                    do
                    {
                        searchResultObject = await _client.ListItem.SearchAsync(searchRequestObject).ConfigureAwait(false);
                        if (searchResultObject.Results.Any())
                        {
                            items.AddRange(searchResultObject.Results);
                        }
                    }
                    while (++i < 3 && ((searchRequestObject.PageToken = searchResultObject.PageToken) != null));
                }
                catch (Exception)
                {
                    failedMetadataSchemaIds.Add(metadataSchemaId);
                }
            }

            // Assert
            Assert.True(!failedMetadataSchemaIds.Any());
            Assert.True(items.Any());
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldUpdate()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client).ConfigureAwait(false);

            // Create object
            var objectName = "ObjectToUpdate" + new Random().Next(0, 999999);
            var listItem = new ListItemCreateRequest
            {
                ContentSchemaId = nameof(SoccerPlayer),
                Content = new SoccerPlayer { Firstname = objectName, LastName = "Foo", EmailAddress = "abc@def.ch" }
            };
            var playerItem = await _client.ListItem.CreateAsync(listItem, new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            // Act
            var player = playerItem.ConvertTo<SoccerPlayer>();
            player.Firstname = "xy jviorej ivorejvioe";

            var updateResult = await _client.ListItem.UpdateAsync(playerItem.Id, player).ConfigureAwait(false);
            var updatedPlayer = await _client.ListItem.GetAndConvertToAsync<SoccerPlayer>(playerItem.Id, nameof(SoccerPlayer)).ConfigureAwait(false);

            // Assert
            Assert.Equal(player.Firstname, updatedPlayer.Firstname);

            updateResult.Audit.CreatedByUser.Should().BeResolved();
            updateResult.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldUpdateMany()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<SoccerPlayer>(_client).ConfigureAwait(false);
            var playerItem = await CreateSoccerPlayer();

            // Act
            var player = playerItem.ConvertTo<SoccerPlayer>();
            player.Firstname = "xy jviorej ivorejvioe";

            await _client.ListItem.UpdateManyAsync(new ListItemUpdateManyRequest
            {
                AllowMissingDependencies = false,
                Items = new[]
                {
                    new ListItemUpdateItem() { Id = playerItem.Id, Content = player }
                }
            }).ConfigureAwait(false);

            var updatedPlayer = await _client.ListItem.GetAndConvertToAsync<SoccerPlayer>(playerItem.Id, nameof(SoccerPlayer)).ConfigureAwait(false);

            // Assert
            Assert.Equal(player.Firstname, updatedPlayer.Firstname);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldDeleteAndRestoreListItem()
        {
            // Arrange
            var listItem = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                ContentSchemaId = nameof(Tag),
                Content = new Tag { Name = "ShouldDeleteAndRestoreListItem" }
            }).ConfigureAwait(false);
            var listItemId = listItem.Id;

            Assert.False(string.IsNullOrEmpty(listItemId));

            // Act
            // Deactivate
            await _client.ListItem.DeleteAsync(listItemId, timeout: new TimeSpan(0, 2, 0)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItemId).ConfigureAwait(false)).ConfigureAwait(false);

            // Reactivate
            await _client.ListItem.RestoreAsync(listItemId, timeout: new TimeSpan(0, 2, 0)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(await _client.ListItem.GetAsync(listItemId).ConfigureAwait(false));
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldDeleteAndRestoreListItemMany()
        {
            // Arrange
            var listItem1 = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                ContentSchemaId = nameof(Tag),
                Content = new Tag { Name = "ShouldDeleteAndRestoreListItemMany1" }
            }).ConfigureAwait(false);

            var listItem2 = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                ContentSchemaId = nameof(Tag),
                Content = new Tag { Name = "ShouldDeleteAndRestoreListItemMany2" }
            }).ConfigureAwait(false);

            // Act
            // Deactivate
            var deactivateRequest = new ListItemDeleteManyRequest()
            {
                ListItemIds = new List<string> { listItem1.Id, listItem2.Id }
            };

            var businessProcess = await _client.ListItem.DeleteManyAsync(deactivateRequest).ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id).ConfigureAwait(false);

            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem1.Id).ConfigureAwait(false)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem2.Id).ConfigureAwait(false)).ConfigureAwait(false);

            // Reactivate
            var reactivateRequest = new ListItemRestoreManyRequest()
            {
                ListItemIds = new List<string> { listItem1.Id, listItem2.Id }
            };

            businessProcess = await _client.ListItem.RestoreManyAsync(reactivateRequest).ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id).ConfigureAwait(false);

            // Assert
            Assert.NotNull(await _client.ListItem.GetAsync(listItem1.Id).ConfigureAwait(false));
            Assert.NotNull(await _client.ListItem.GetAsync(listItem2.Id).ConfigureAwait(false));
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldDeleteListItemManyByFilter()
        {
            // Arrange
            string uniqueValue = $"{Guid.NewGuid():N}";

            var listItem1 = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                ContentSchemaId = nameof(Tag),
                Content = new Tag { Name = $"{uniqueValue}_1" }
            }).ConfigureAwait(false);

            var listItem2 = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                ContentSchemaId = nameof(Tag),
                Content = new Tag { Name = $"{uniqueValue}_2" }
            }).ConfigureAwait(false);

            // Act
            // Deactivate
            var deactivateRequest = new ListItemDeleteManyFilterRequest()
            {
                FilterRequest = new ListItemFilterRequest
                {
                    SchemaIds = new[] { nameof(Tag) },
                    SearchString = $"{uniqueValue}*"
                }
            };

            var businessProcess = await _client.ListItem.DeleteManyByFilterAsync(deactivateRequest).ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id).ConfigureAwait(false);

            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem1.Id).ConfigureAwait(false)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem2.Id).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldUseDisplayLanguageToResolveDisplayValues()
        {
            // Arange
            var schema = await SchemaHelper.CreateSchemasIfNotExistentAsync<DisplayLanguageTestItems>(_client).ConfigureAwait(false);

            var listItem1 = new DisplayLanguageTestItems()
            {
                Value1 = "value1",
                Value2 = "value2"
            };

            var detail = await _client.ListItem.CreateAsync(new ListItemCreateRequest() { ContentSchemaId = schema.Id, Content = listItem1 }).ConfigureAwait(false);

            // Act
            var englishClient = _fixture.GetLocalizedPictureparkService("en");
            var receivedItem1 = await englishClient.ListItem.GetAsync(detail.Id, new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.OuterDisplayValueName }).ConfigureAwait(false);

            var germanClient = _fixture.GetLocalizedPictureparkService("de");
            var receivedItem2 = await germanClient.ListItem.GetAsync(detail.Id, new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.OuterDisplayValueName }).ConfigureAwait(false);

            // Assert
            receivedItem1.DisplayValues[DisplayPatternType.Name.ToString().ToLowerCamelCase()].Should().Be("value2");
            receivedItem2.DisplayValues[DisplayPatternType.Name.ToString().ToLowerCamelCase()].Should().Be("value1");
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldUseLocalDateForDisplayValue()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<LocalDateTestItem>(_client).ConfigureAwait(false);

            var date = new DateTime(2012, 12, 12, 1, 1, 1, DateTimeKind.Utc);

            var listItem1 = new LocalDateTestItem
            {
                DateTimeField = date,
                Child = new LocalDateTestItem
                {
                    DateTimeField = new DateTime(2010, 1, 1, 12, 1, 1, DateTimeKind.Local)
                }
            };

            var detail = await _client.ListItem.CreateFromObjectAsync(listItem1).ConfigureAwait(false);

            var details = await detail.FetchDetail(new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.InnerDisplayValueName, ListItemResolveBehavior.OuterDisplayValueName }).ConfigureAwait(false);
            var items = details.SucceededItems;

            // Act
            var item = items.Single(x => x.RequestId == ListItemClient.RootObjectRequestId).Item;
            var dateValue = item.ConvertTo<LocalDateTestItem>().DateTimeField;

            const string quote = "\"";
            var shouldBeValue = $"{{{{ {quote}{dateValue:O}{quote} | date: {quote}%d.%m.%Y %H:%M:%S{quote} }}}}";

            // Assert
            item.DisplayValues[DisplayPatternType.Name.ToString().ToLowerCamelCase()]
                .Should().Be(shouldBeValue);

            var renderedDisplayValue = LocalizationService.GetDateTimeLocalizedDisplayValue(shouldBeValue);
            var formattedLocalDate = date.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
            var formattedChildLocalDate = listItem1.Child.DateTimeField.ToString("dd.MM.yyyy HH:mm:ss");
            renderedDisplayValue.Should().Be(formattedLocalDate);

            // Apply local time to object tree
            LocalizationService.ReplaceDateTimeLocalizedDisplayValueInObject(item);

            item.DisplayValues["name"].Should().Be(formattedLocalDate);
            ((JObject)item.Content)
                .GetValue("child")
                .Value<JToken>("_displayValues")
                .Value<string>("name").Should()
                .Be(formattedChildLocalDate);
        }

        [Fact]
        [Trait("Stack", "Contents")]
        public async Task ShouldFetchResultFromCreateMany()
        {
            // Arrange
            var requests = Enumerable.Range(0, 201).Select(
                x => new ListItemCreateRequest
                {
                    Content = new
                    {
                        name = $"ListItem #{x}"
                    },
                    ContentSchemaId = nameof(Tag)
                }).ToList();

            var result = await _client.ListItem.CreateManyAsync(
                new ListItemCreateManyRequest
                {
                    Items = requests
                }).ConfigureAwait(false);

            // Act
            var detail = await result.FetchDetail(new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            // Assert
            detail.SucceededItems.Should().HaveCount(201);
            detail.SucceededItems.Select(i => ((dynamic)i.Item.Content).name).ToArray().Distinct().Should().HaveCount(201);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldProperlyResolveSchemaName()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Vehicle>(_client).ConfigureAwait(false);

            var carResult = await _client.ListItem.CreateFromObjectAsync(
                new Car
                {
                    NumberOfWheels = 4,
                    HorsePower = 142,
                    BootSize = 490,
                    Model = "Civic",
                    Introduced = new DateTime(2013, 1, 1)
                }).ConfigureAwait(false);

            var carResultDetail = await carResult.FetchDetail().ConfigureAwait(false);

            carResultDetail.FailedItems.Should().BeEmpty();
            carResultDetail.SucceededItems.Should().NotBeEmpty()
                .And.Subject.Single(i => i.RequestId == ListItemClient.RootObjectRequestId).Item.ContentSchemaId.Should().Be("Automobile");
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldGenerateSchemaFireTriggerAndRetrieveInfo()
        {
            // Arrange
            var myself = await _client.Profile.GetAsync().ConfigureAwait(false);

            await SchemaHelper.CreateSchemasIfNotExistentAsync<TriggerList>(_client).ConfigureAwait(false);

            var listItemCreationResult = await _client.ListItem.CreateFromObjectAsync(
                new TriggerList
                {
                    Name = "name"
                }).ConfigureAwait(false);

            var listItemId = (await listItemCreationResult.FetchDetail().ConfigureAwait(false)).SucceededIds.Single();

            // act
            var result = await _client.ListItem.UpdateAsync(
                listItemId,
                new TriggerList
                {
                    Name = "name",
                    Trigger = new TriggerObject
                    {
                        Trigger = true
                    }
                },
                new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            var updated = result.ConvertTo<TriggerList>();

            // assert
            updated.Trigger.Trigger.Should().BeFalse();
            updated.Trigger.TriggeredOn.Should().NotBeNull();
            updated.Trigger.TriggeredBy.Id.Should().Be(myself.Id);
            updated.Trigger.TriggeredBy.EmailAddress.Should().Be(myself.EmailAddress);
            updated.Trigger.TriggeredBy.FirstName.Should().Be(myself.FirstName);
            updated.Trigger.TriggeredBy.LastName.Should().Be(myself.LastName);

            // update again using same object
            updated.Name = "name (updated)";
            var previousTriggerDate = updated.Trigger.TriggeredOn;

            // assert
            result = await _client.ListItem.UpdateAsync(listItemId, updated, new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);
            updated = result.ConvertTo<TriggerList>();

            updated.Trigger.TriggeredOn.Should().Be(previousTriggerDate);
            updated.Trigger.TriggeredBy.Id.Should().Be(myself.Id);
            updated.Name.Should().Be("name (updated)");
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateListItemsAndReturnRequestIdInDetail()
        {
            var prefix = $"{Guid.NewGuid():N}";

            var requests = Enumerable.Range(0, 5).Select(n => $"{Guid.NewGuid():N}").Select(
                (requestId, n) =>
                    (requestId, new ListItemCreateRequest
                    {
                        RequestId = requestId,
                        ContentSchemaId = nameof(Tag),
                        Content = new Tag { Name = $"{prefix}_{n}" }
                    })).ToArray();

            var result = await _client.ListItem.CreateManyAsync(
                new ListItemCreateManyRequest
                {
                    Items = requests.Select(i => i.Item2).ToArray()
                }).ConfigureAwait(false);

            var detail = await result.FetchDetail(new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            foreach (var row in detail.SucceededItems)
            {
                var associatedRequest = requests.Single(x => x.requestId == row.RequestId);
                ((Tag)associatedRequest.Item2.Content).Name.Should().Be(row.Item.ConvertTo<Tag>().Name);
            }
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateListItemsAndEnumerateResultRowsWithoutRequestId()
        {
            var prefix = $"{Guid.NewGuid():N}";

            var requests = Enumerable.Range(0, 5).Select(n => $"{Guid.NewGuid():N}").Select(
                (requestId, n) =>
                    (requestId, new ListItemCreateRequest
                    {
                        ContentSchemaId = nameof(Tag),
                        Content = new Tag { Name = $"{prefix}_{n}" }
                    })).ToArray();

            var result = await _client.ListItem.CreateManyAsync(
                new ListItemCreateManyRequest
                {
                    Items = requests.Select(i => i.Item2).ToArray()
                }).ConfigureAwait(false);

            var detail = await result.FetchDetail(new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            var enumeratedRows = detail.SucceededItems.ToArray();
            enumeratedRows.Should().OnlyContain(r => r.RequestId == null).And.HaveCount(requests.Length);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateUpdatedDeleteAndRestoreListItemWithoutWaitingSearchDocs()
        {
            // Act
            // Create
            var listItem = await _client.ListItem.CreateAsync(
                    new ListItemCreateRequest { ContentSchemaId = nameof(Tag), Content = new Tag { Name = $"{Guid.NewGuid():N}" } },
                    waitSearchDocCreation: false)
                .ConfigureAwait(false);

            // Update
            var expectedName = $"{Guid.NewGuid():N}";
            listItem = await _client.ListItem.UpdateAsync(listItem.Id, new ListItemUpdateRequest { Content = new Tag { Name = expectedName } }, waitSearchDocCreation: false)
                .ConfigureAwait(false);

            // Delete
            await _client.ListItem.DeleteAsync(listItem.Id, waitSearchDocCreation: false).ConfigureAwait(false);

            // Assert
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem.Id).ConfigureAwait(false)).ConfigureAwait(false);

            // Restore
            await _client.ListItem.RestoreAsync(listItem.Id, waitSearchDocCreation: false).ConfigureAwait(false);

            listItem = await _client.ListItem.GetAsync(listItem.Id, new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            // Assert
            listItem.Should().NotBeNull();
            listItem.ConvertTo<Tag>().Name.Should().Be(expectedName);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateUpdatedDeleteAndRestoreManyListItemsWithoutWaitingSearchDocs()
        {
            // Act
            // Create
            var listItem1CreateRequest = new ListItemCreateRequest { RequestId = $"{Guid.NewGuid():N}", ContentSchemaId = nameof(Tag), Content = new Tag { Name = $"{Guid.NewGuid():N}" } };
            var listItem2CreateRequest = new ListItemCreateRequest { RequestId = $"{Guid.NewGuid():N}", ContentSchemaId = nameof(Tag), Content = new Tag { Name = $"{Guid.NewGuid():N}" } };

            var result = await _client.ListItem.CreateManyAsync(new ListItemCreateManyRequest { Items = new[] { listItem1CreateRequest, listItem2CreateRequest } }, waitSearchDocCreation: false)
                .ConfigureAwait(false);

            var createdItems = (await result.FetchDetail().ConfigureAwait(false)).SucceededItems.ToArray();
            var listItem1Id = createdItems.Single(i => i.RequestId == listItem1CreateRequest.RequestId).Item.Id;
            var listItem2Id = createdItems.Single(i => i.RequestId == listItem2CreateRequest.RequestId).Item.Id;
            var listItemIds = new[] { listItem1Id, listItem2Id };

            // Update
            var expectedName1 = $"{Guid.NewGuid():N}";
            var expectedName2 = $"{Guid.NewGuid():N}";
            var listItem1UpdateItem = new ListItemUpdateItem { Id = listItem1Id, Content = new Tag { Name = expectedName1 } };
            var listItem2UpdateItem = new ListItemUpdateItem { Id = listItem2Id, Content = new Tag { Name = expectedName2 } };

            await _client.ListItem.UpdateManyAsync(new ListItemUpdateManyRequest { Items = new[] { listItem1UpdateItem, listItem2UpdateItem } }, waitSearchDocCreation: false)
                .ConfigureAwait(false);

            // Delete
            var businessProcess = await _client.ListItem.DeleteManyAsync(new ListItemDeleteManyRequest { ListItemIds = listItemIds }).ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id, waitForContinuationCompletion: false).ConfigureAwait(false);

            // Assert
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem1Id).ConfigureAwait(false)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem2Id).ConfigureAwait(false)).ConfigureAwait(false);

            // Restore
            businessProcess = await _client.ListItem.RestoreManyAsync(new ListItemRestoreManyRequest { ListItemIds = listItemIds }).ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id, waitForContinuationCompletion: false).ConfigureAwait(false);

            var listItems = await _client.ListItem.GetManyAsync(listItemIds, new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            // Assert
            listItems.Should().HaveCount(2).And.Subject.Select(c => c.ConvertTo<Tag>().Name).Should().BeEquivalentTo(expectedName1, expectedName2);
        }

        [Fact]
        [Trait("Stack", "Contents")]
        public async Task ShouldBatchUpdateWithoutWithoutWaitingSearchDocs()
        {
            // Arrange
            var listItem1CreateRequest = new ListItemCreateRequest { RequestId = $"{Guid.NewGuid():N}", ContentSchemaId = nameof(Tag), Content = new Tag { Name = $"{Guid.NewGuid():N}" } };
            var listItem2CreateRequest = new ListItemCreateRequest { RequestId = $"{Guid.NewGuid():N}", ContentSchemaId = nameof(Tag), Content = new Tag { Name = $"{Guid.NewGuid():N}" } };

            var result = await _client.ListItem.CreateManyAsync(new ListItemCreateManyRequest { Items = new[] { listItem1CreateRequest, listItem2CreateRequest } }, waitSearchDocCreation: false)
                .ConfigureAwait(false);

            var listItemIds = (await result.FetchDetail().ConfigureAwait(false)).SucceededIds;

            // Act
            var temporaryName = $"{Guid.NewGuid():N}";
            var expectedName = $"{Guid.NewGuid():N}";

            var updateRequest = new ListItemFieldsBatchUpdateRequest
            {
                ListItemIds = listItemIds.ToArray(),
                ChangeCommands = new List<MetadataValuesChangeCommandBase>
                {
                    new MetadataValuesSchemaUpdateCommand { SchemaId = nameof(Tag), Value = new DataDictionary { { "name", temporaryName } } }
                }
            };

            await _client.ListItem.BatchUpdateFieldsByIdsAsync(updateRequest, waitSearchDocCreation: false).ConfigureAwait(false);

            updateRequest.ChangeCommands.First().As<MetadataValuesSchemaUpdateCommand>().Value = new DataDictionary { { "name", expectedName } };
            await _client.ListItem.BatchUpdateFieldsByIdsAsync(updateRequest, waitSearchDocCreation: false).ConfigureAwait(false);

            var listItems = await _client.ListItem.GetManyAsync(listItemIds, new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);

            // Assert
            listItems.Should().HaveCount(2).And.Subject.Select(c => c.ConvertTo<Tag>().Name).Should().OnlyContain(s => s == expectedName);
        }

        private async Task<ListItemDetail> CreateSoccerPlayer()
        {
            var originalPlayer = new SoccerPlayer { BirthDate = DateTime.Now, EmailAddress = "test@test.com", Firstname = "Test", LastName = "Soccerplayer" };

            var createRequest = new ListItemCreateRequest { ContentSchemaId = nameof(SoccerPlayer), Content = originalPlayer };

            var playerItem = await _client.ListItem.CreateAsync(createRequest, new[] { ListItemResolveBehavior.Content }).ConfigureAwait(false);
            return playerItem;
        }
    }
}