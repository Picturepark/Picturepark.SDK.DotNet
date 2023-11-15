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
                    new TermsAggregator { Name = fieldName, Field = fieldName, Size = 20, Sort = new SortInfo { Direction = SortDirection.Asc } }
                }
            };

            // Act
            ObjectAggregationResult result = await _client.ListItem.AggregateAsync(request);
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
            var result = await _client.ListItem.SearchAsync(request);

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

            var results = await _client.ListItem.CreateManyAsync(createManyRequest);
            var detail = await results.FetchDetail();

            var itemId = detail.SucceededIds.First();

            // Act
            var request = new ListItemUpdateRequest
            {
                Content = new Tag { Name = "Foo" }
            };
            await _client.ListItem.UpdateAsync(itemId, request);

            // Assert
            var newItem = await _client.ListItem.GetAsync(itemId, new[] { ListItemResolveBehavior.Content });
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
            ListItemDetail result = await _client.ListItem.CreateAsync(listItem);

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
            ListItemDetail listItemDetail = await _client.ListItem.CreateAsync(listItem);

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
            await _client.ListItem.BatchUpdateFieldsByFilterAsync(updateRequest);
            ListItemDetail result = await _client.ListItem.GetAsync(listItemDetail.Id, new[] { ListItemResolveBehavior.Content });

            // Assert
            Assert.Equal("Foo", result.ConvertTo<Tag>().Name);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateWithHelperContainingBaseClassOfInheritance()
        {
            // Arrange
            var pet = new Pet { Name = "Pet" + new Random().Next(0, 999999) };

            // Act
            var petCreateResult = await _client.ListItem.CreateFromObjectAsync(pet);
            var petCreateDetail = await petCreateResult.FetchDetail(new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems });
            var petListItem = petCreateDetail.SucceededItems.First().Item;

            // Assert
            var petObject = petListItem.ConvertTo<Pet>();
            petObject.Name.Should().Be(pet.Name);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateWithHelperContainingInheritedClass1()
        {
            // Arrange
            var dog = new Dog { Name = "Dog" + new Random().Next(0, 999999), PlaysCatch = true };

            // Act
            var dogCreateResult = await _client.ListItem.CreateFromObjectAsync(dog);
            var dogCreateDetail = await dogCreateResult.FetchDetail(new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems });
            var dogListItem = dogCreateDetail.SucceededItems.First().Item;

            // Assert
            var dogObject = dogListItem.ConvertTo<Dog>();
            dogObject.PlaysCatch.Should().BeTrue();
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateWithHelperContainingInheritedClass2()
        {
            // Arrange
            var cat = new Cat { Name = "Cat" + new Random().Next(0, 999999), ChasesLaser = true };

            // Act
            var catCreateResult = await _client.ListItem.CreateFromObjectAsync(cat);
            var catCreateDetail = await catCreateResult.FetchDetail(new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems });
            var catListItem = catCreateDetail.SucceededItems.First().Item;

            // Assert
            var catObject = catListItem.ConvertTo<Cat>();
            catObject.ChasesLaser.Should().BeTrue();
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateObjectWithHelperContainingBaseClassOfInheritance()
        {
            // Act
            var personResult = await _client.ListItem.CreateFromObjectAsync(
                new Person
                {
                    BirthDate = DateTime.Now,
                    EmailAddress = "xyyyy@teyyyyyyst.com",
                    Firstname = "Urxxxxs",
                    LastName = "xxxxxxxx"
                });

            var personDetail = await personResult.FetchDetail(new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems });

            // Assert
            personDetail.SucceededItems.Should().NotBeEmpty();
            var person = personDetail.SucceededItems.First().Item.ConvertTo<Person>();
            person.EmailAddress.Should().Be("xyyyy@teyyyyyyst.com");
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateComplexObjectWithHelperContainingInheritance()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client);

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
                    AddressesPlus = new List<AddressesPlus>
                    {
                        new AddressesPlus
                        {
                            Name = "Lenzburg",
                            SecurityPet = dog,
                            Number = 2
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
                });

            var soccerPlayerDetail = await soccerPlayerResult.FetchDetail(new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems });

            // Assert
            soccerPlayerDetail.SucceededItems.Should().NotBeEmpty();
            foreach (var succeededItem in soccerPlayerDetail.SucceededItems)
            {
                var listItemDetail = succeededItem.Item;
                if (listItemDetail.ContentSchemaId == "SoccerPlayer")
                {
                    var soccerPlayer = listItemDetail.ConvertTo<SoccerPlayer>();
                    soccerPlayer.OwnsPets.First(p => p.Name == "Catname1").Should().BeOfType<Cat>().Subject.ChasesLaser.Should().BeTrue();
                    soccerPlayer.OwnsPets.First(p => p.Name == "Dogname1").Should().BeOfType<Dog>().Subject.PlaysCatch.Should().BeTrue();
                    soccerPlayer.Addresses.First().SecurityPet.Should().BeOfType<Dog>().Subject.PlaysCatch.Should().BeTrue();
                    soccerPlayer.AddressesPlus.First().SecurityPet.Should().BeOfType<Dog>().Subject.PlaysCatch.Should().BeTrue();
                }

                if (listItemDetail.ContentSchemaId == "Cat")
                {
                    var cat = listItemDetail.ConvertTo<Cat>();
                    cat.ChasesLaser.Should().BeTrue();

                    var pet = listItemDetail.ConvertTo<Pet>();
                    pet.Name.Should().Be("Catname1");
                    ((Cat)pet).ChasesLaser.Should().BeTrue();
                }

                if (listItemDetail.ContentSchemaId == "Dog")
                {
                    var dogObject = listItemDetail.ConvertTo<Dog>();
                    dogObject.PlaysCatch.Should().BeTrue();

                    var pet = listItemDetail.ConvertTo<Pet>();
                    pet.Name.Should().Be(dog.Name);
                    ((Dog)pet).PlaysCatch.Should().BeTrue();
                }
            }
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateComplexObjectWithHelperContainingInheritanceWithSchemaIdsDifferentFromStrongTypes()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client);

            // Act
            var club = new Club { Country = "Country1", Name = "Name1" };
            var leagueClub = new LeagueClub { Country = "Country2", Name = "Club2", League = "League2" };
            var schoolClub = new SchoolClub { Country = "Country3", Name = "Name3", School = "School3" };

            var soccerTrainerResult = await _client.ListItem.CreateFromObjectAsync(
                new SoccerTrainer
                {
                    BirthDate = DateTime.Now,
                    EmailAddress = "xyyyy@teyyyyyyst.com",
                    Firstname = "Urxxxxs",
                    LastName = "xxxxxxxx",
                    TrainerSince = new DateTime(2000, 1, 1),
                    PreviousClubs = new List<Club> { club, leagueClub, schoolClub }
                });

            var soccerTrainerDetail = await soccerTrainerResult.FetchDetail(new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems });

            // Assert
            soccerTrainerDetail.SucceededItems.Should().NotBeEmpty();
            foreach (var succeededItem in soccerTrainerDetail.SucceededItems)
            {
                var listItemDetail = succeededItem.Item;
                switch (listItemDetail.ContentSchemaId)
                {
                    case "SoccerTrainer":
                    {
                        var soccerTrainerObject = listItemDetail.ConvertTo<SoccerTrainer>();
                        soccerTrainerObject.TrainerSince.Should().Be(new DateTime(2000, 1, 1));
                        soccerTrainerObject.PreviousClubs.First(p => p.Name == club.Name).Should().BeOfType<Club>().Subject.Country.Should().Be(club.Country);
                        soccerTrainerObject.PreviousClubs.First(p => p.Name == leagueClub.Name).Should().BeOfType<LeagueClub>().Subject.League.Should().Be(leagueClub.League);
                        soccerTrainerObject.PreviousClubs.First(p => p.Name == schoolClub.Name).Should().BeOfType<SchoolClub>().Subject.School.Should().Be(schoolClub.School);
                        break;
                    }

                    case "ClubId":
                    {
                        var clubObject = listItemDetail.ConvertTo<Club>();
                        clubObject.Name.Should().Be(club.Name);
                        break;
                    }

                    case "LeagueClubId":
                    {
                        var leagueClubObject = listItemDetail.ConvertTo<LeagueClub>();
                        leagueClubObject.League.Should().Be(leagueClub.League);

                        var clubObject = listItemDetail.ConvertTo<Club>();
                        clubObject.Name.Should().Be(leagueClub.Name);
                        ((LeagueClub)clubObject).League.Should().Be(leagueClub.League);
                        break;
                    }

                    case "SchoolClubId":
                    {
                        var schoolClubObject = listItemDetail.ConvertTo<SchoolClub>();
                        schoolClubObject.School.Should().Be(schoolClub.School);

                        var clubObject = listItemDetail.ConvertTo<Club>();
                        clubObject.Name.Should().Be(schoolClub.Name);
                        ((SchoolClub)clubObject).School.Should().Be(schoolClub.School);
                        break;
                    }
                }
            }
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldCreateObjectWithoutHelper()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client);

            var petsCreateRequest = new ListItemCreateManyRequest
            {
                Items = new List<ListItemCreateRequest>
                {
                    new ListItemCreateRequest { ContentSchemaId = nameof(Cat), Content = new Cat { ChasesLaser = true, Name = "Cat1" } },
                    new ListItemCreateRequest { ContentSchemaId = nameof(Dog), Content = new Dog { PlaysCatch = true, Name = "Dog1" } },
                    new ListItemCreateRequest { ContentSchemaId = nameof(Pet), Content = new Pet { Name = "Pet1" } }
                }
            };

            var result = await _client.ListItem.CreateManyAsync(petsCreateRequest);
            var succeededItems = (await result.FetchDetail(new[] { ListItemResolveBehavior.Content })).SucceededItems.ToArray();
            succeededItems.Should().HaveCount(3);

            Cat cat = null;
            Dog dog = null;
            Pet pet = null;

            foreach (var item in succeededItems)
            {
                switch (item.Item.ContentSchemaId)
                {
                    case nameof(Pet):
                        pet = item.Item.ConvertTo<Pet>();
                        pet.Name.Should().Be("Pet1");
                        break;
                    case nameof(Cat):
                        cat = item.Item.ConvertTo<Cat>();
                        cat.ChasesLaser.Should().BeTrue();
                        break;
                    case nameof(Dog):
                        dog = item.Item.ConvertTo<Dog>();
                        dog.PlaysCatch.Should().BeTrue();
                        break;
                }
            }

            var originalPlayer = new SoccerPlayer
            {
                BirthDate = DateTime.Now,
                EmailAddress = "test@test.com",
                Firstname = "Urs",
                LastName = "Brogle",
                Addresses = new List<Addresses>
                {
                    new Addresses
                    {
                        Name = "Aarau",
                        SecurityPet = cat
                    }
                },
                AddressesPlus = new List<AddressesPlus>
                {
                    new AddressesPlus
                    {
                        Name = "Lenzburg",
                        Number = 2,
                        SecurityPet = dog
                    }
                },
                OwnsPets = new List<Pet> { cat, dog, pet }
            };

            // Act
            var createRequest = new ListItemCreateRequest
            {
                ContentSchemaId = nameof(SoccerPlayer),
                Content = originalPlayer
            };

            var playerItem = await _client.ListItem.CreateAsync(createRequest, new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems });

            // Assert
            Assert.NotNull(playerItem);

            var createdPlayer = playerItem.ConvertTo<SoccerPlayer>();
            Assert.Equal("Urs", createdPlayer.Firstname);
            createdPlayer.Addresses.First().Name.Should().Be("Aarau");
            createdPlayer.Addresses.First().SecurityPet.Should().BeOfType<Cat>();
            createdPlayer.AddressesPlus.First().Number.Should().Be(2);
            createdPlayer.AddressesPlus.First().SecurityPet.Should().BeOfType<Dog>();
            createdPlayer.OwnsPets.Should().HaveCount(3);
            createdPlayer.OwnsPets.First(p => p.Name == "Pet1").Should().BeOfType<Pet>();
            createdPlayer.OwnsPets.First(p => p.Name == "Cat1").Should().BeOfType<Cat>();
            createdPlayer.OwnsPets.First(p => p.Name == "Dog1").Should().BeOfType<Dog>();
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

            var listItem = await _client.ListItem.CreateAsync(createRequest);
            var result = await _client.ListItem.GetAsync(listItem.Id);

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
            });

            chSearch.Results.Should().NotBeEmpty("Switzerland should exist");

            // Act
            var listItem = await _client.ListItem.GetAsync(chSearch.Results.First().Id, new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems });

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

            var createResult = await _client.ListItem.CreateManyAsync(createRequest);
            var createDetail = await createResult.FetchDetail();
            var createdListItemIds = createDetail.SucceededIds;

            // Act
            var resultListItems = await _client.ListItem.GetManyAsync(createdListItemIds, new List<ListItemResolveBehavior> { ListItemResolveBehavior.Content });

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

            var createResult = await _client.ListItem.CreateManyAsync(createRequest);
            var createDetail = await createResult.FetchDetail();

            // Act
            var resultListItems = await _client.ListItem.GetManyAndConvertToAsync<Tag>(createDetail.SucceededIds, nameof(Tag));

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

            var searchResultSchema = await _client.Schema.SearchAsync(searchRequestSchema);
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
                        searchResultObject = await _client.ListItem.SearchAsync(searchRequestObject);
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

            items.First().LifeCycle.Should().Be(LifeCycle.Active);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldUpdate()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(_client);

            // Create object
            var objectName = "ObjectToUpdate" + new Random().Next(0, 999999);
            var listItem = new ListItemCreateRequest
            {
                ContentSchemaId = nameof(SoccerPlayer),
                Content = new SoccerPlayer { Firstname = objectName, LastName = "Foo", EmailAddress = "abc@def.ch" }
            };
            var playerItem = await _client.ListItem.CreateAsync(listItem, new[] { ListItemResolveBehavior.Content });

            // Act
            var player = playerItem.ConvertTo<SoccerPlayer>();
            player.Firstname = "xy jviorej ivorejvioe";

            var updateResult = await _client.ListItem.UpdateAsync(playerItem.Id, player);
            var updatedPlayer = await _client.ListItem.GetAndConvertToAsync<SoccerPlayer>(playerItem.Id, nameof(SoccerPlayer));

            // Assert
            Assert.Equal(player.Firstname, updatedPlayer.Firstname);

            updateResult.Audit.CreatedByUser.Should().BeResolved();
            updateResult.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldUpdateWithContentReplaceIfUpdateOptionSetToReplace()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Club>(_client);

            // Create object
            var objectName = "Superclub" + new Random().Next(0, 999999);
            var listItem = new ListItemCreateRequest
            {
                ContentSchemaId = "ClubId",
                Content = new Club { Name = objectName, Country = "Switzerland" }
            };
            var clubItem = await _client.ListItem.CreateAsync(listItem, new[] { ListItemResolveBehavior.Content });

            var club = clubItem.ConvertTo<Club>();
            club.Country.Should().Be("Switzerland");

            // Act
            club.Country = null;

            await _client.ListItem.UpdateAsync(clubItem.Id, club, UpdateOption.Replace);
            var updateClub = await _client.ListItem.GetAndConvertToAsync<Club>(clubItem.Id, nameof(Club));

            // Assert
            updateClub.Name.Should().Be(objectName);
            updateClub.Country.Should().BeNull();
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldUpdateMany()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<SoccerPlayer>(_client);
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
            });

            var updatedPlayer = await _client.ListItem.GetAndConvertToAsync<SoccerPlayer>(playerItem.Id, nameof(SoccerPlayer));

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
            });
            var listItemId = listItem.Id;

            Assert.False(string.IsNullOrEmpty(listItemId));

            // Act
            // Deactivate
            await _client.ListItem.DeleteAsync(listItemId, timeout: new TimeSpan(0, 2, 0));
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItemId));

            // Reactivate
            await _client.ListItem.RestoreAsync(listItemId, timeout: new TimeSpan(0, 2, 0));

            // Assert
            Assert.NotNull(await _client.ListItem.GetAsync(listItemId));
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
            });

            var listItem2 = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                ContentSchemaId = nameof(Tag),
                Content = new Tag { Name = "ShouldDeleteAndRestoreListItemMany2" }
            });

            // Act
            // Deactivate
            var deactivateRequest = new ListItemDeleteManyRequest()
            {
                ListItemIds = new List<string> { listItem1.Id, listItem2.Id }
            };

            var businessProcess = await _client.ListItem.DeleteManyAsync(deactivateRequest);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem1.Id));
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem2.Id));

            // Reactivate
            var reactivateRequest = new ListItemRestoreManyRequest()
            {
                ListItemIds = new List<string> { listItem1.Id, listItem2.Id }
            };

            businessProcess = await _client.ListItem.RestoreManyAsync(reactivateRequest);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            // Assert
            Assert.NotNull(await _client.ListItem.GetAsync(listItem1.Id));
            Assert.NotNull(await _client.ListItem.GetAsync(listItem2.Id));
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
            });

            var listItem2 = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                ContentSchemaId = nameof(Tag),
                Content = new Tag { Name = $"{uniqueValue}_2" }
            });

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

            var businessProcess = await _client.ListItem.DeleteManyByFilterAsync(deactivateRequest);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem1.Id));
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem2.Id));
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldUseDisplayLanguageToResolveDisplayValues()
        {
            // Arange
            var schema = await SchemaHelper.CreateSchemasIfNotExistentAsync<DisplayLanguageTestItems>(_client);

            var listItem1 = new DisplayLanguageTestItems()
            {
                Value1 = "value1",
                Value2 = "value2"
            };

            var detail = await _client.ListItem.CreateAsync(new ListItemCreateRequest() { ContentSchemaId = schema.Id, Content = listItem1 });

            // Act
            var englishClient = _fixture.GetLocalizedPictureparkService("en");
            var receivedItem1 = await englishClient.ListItem.GetAsync(detail.Id, new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.OuterDisplayValueName });

            var germanClient = _fixture.GetLocalizedPictureparkService("de");
            var receivedItem2 = await germanClient.ListItem.GetAsync(detail.Id, new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.OuterDisplayValueName });

            // Assert
            receivedItem1.DisplayValues[DisplayPatternType.Name.ToString().ToLowerCamelCase()].Should().Be("value2");
            receivedItem2.DisplayValues[DisplayPatternType.Name.ToString().ToLowerCamelCase()].Should().Be("value1");
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldUseLocalDateForDisplayValue()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<LocalDateTestItem>(_client);

            var date = new DateTime(2012, 12, 12, 1, 1, 1, DateTimeKind.Utc);

            var listItem1 = new LocalDateTestItem
            {
                DateTimeField = date,
                Child = new LocalDateTestItem
                {
                    DateTimeField = new DateTime(2010, 1, 1, 12, 1, 1, DateTimeKind.Local)
                }
            };

            var detail = await _client.ListItem.CreateFromObjectAsync(listItem1);

            var details = await detail.FetchDetail(new[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.InnerDisplayValueName, ListItemResolveBehavior.OuterDisplayValueName });
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
        [Trait("Stack", "ListItem")]
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
                });

            // Act
            var detail = await result.FetchDetail(new[] { ListItemResolveBehavior.Content });

            // Assert
            detail.SucceededItems.Should().HaveCount(201);
            detail.SucceededItems.Select(i => ((dynamic)i.Item.Content).name).ToArray().Distinct().Should().HaveCount(201);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldProperlyResolveSchemaName()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<Vehicle>(_client);

            var carResult = await _client.ListItem.CreateFromObjectAsync(
                new Car
                {
                    NumberOfWheels = 4,
                    HorsePower = 142,
                    BootSize = 490,
                    Model = "Civic",
                    Introduced = new DateTime(2013, 1, 1)
                });

            var carResultDetail = await carResult.FetchDetail();

            carResultDetail.FailedItems.Should().BeEmpty();
            carResultDetail.SucceededItems.Should().NotBeEmpty()
                .And.Subject.Single(i => i.RequestId == ListItemClient.RootObjectRequestId).Item.ContentSchemaId.Should().Be("Automobile");
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldGenerateSchemaFireTriggerAndRetrieveInfo()
        {
            // Arrange
            var myself = await _client.Profile.GetAsync();

            await SchemaHelper.CreateSchemasIfNotExistentAsync<TriggerList>(_client);

            var listItemCreationResult = await _client.ListItem.CreateFromObjectAsync(
                new TriggerList
                {
                    Name = "name"
                });

            var listItemId = (await listItemCreationResult.FetchDetail()).SucceededIds.Single();

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
                resolveBehaviors: new[] { ListItemResolveBehavior.Content });

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
            result = await _client.ListItem.UpdateAsync(listItemId, updated, resolveBehaviors: new[] { ListItemResolveBehavior.Content });
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
                });

            var detail = await result.FetchDetail(new[] { ListItemResolveBehavior.Content });

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
                });

            var detail = await result.FetchDetail(new[] { ListItemResolveBehavior.Content });

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
                ;

            // Update
            var expectedName = $"{Guid.NewGuid():N}";
            listItem = await _client.ListItem.UpdateAsync(listItem.Id, new ListItemUpdateRequest { Content = new Tag { Name = expectedName } }, waitSearchDocCreation: false)
                ;

            // Delete
            await _client.ListItem.DeleteAsync(listItem.Id, waitSearchDocCreation: false);

            // Assert
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem.Id));

            // Restore
            await _client.ListItem.RestoreAsync(listItem.Id, waitSearchDocCreation: false);

            listItem = await _client.ListItem.GetAsync(listItem.Id, new[] { ListItemResolveBehavior.Content });

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
                ;

            var createdItems = (await result.FetchDetail()).SucceededItems.ToArray();
            var listItem1Id = createdItems.Single(i => i.RequestId == listItem1CreateRequest.RequestId).Item.Id;
            var listItem2Id = createdItems.Single(i => i.RequestId == listItem2CreateRequest.RequestId).Item.Id;
            var listItemIds = new[] { listItem1Id, listItem2Id };

            // Update
            var expectedName1 = $"{Guid.NewGuid():N}";
            var expectedName2 = $"{Guid.NewGuid():N}";
            var listItem1UpdateItem = new ListItemUpdateItem { Id = listItem1Id, Content = new Tag { Name = expectedName1 } };
            var listItem2UpdateItem = new ListItemUpdateItem { Id = listItem2Id, Content = new Tag { Name = expectedName2 } };

            await _client.ListItem.UpdateManyAsync(new ListItemUpdateManyRequest { Items = new[] { listItem1UpdateItem, listItem2UpdateItem } }, waitSearchDocCreation: false)
                ;

            // Delete
            var businessProcess = await _client.ListItem.DeleteManyAsync(new ListItemDeleteManyRequest { ListItemIds = listItemIds });
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id, waitForContinuationCompletion: false);

            // Assert
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem1Id));
            await Assert.ThrowsAsync<ListItemNotFoundException>(async () => await _client.ListItem.GetAsync(listItem2Id));

            // Restore
            businessProcess = await _client.ListItem.RestoreManyAsync(new ListItemRestoreManyRequest { ListItemIds = listItemIds });
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id, waitForContinuationCompletion: false);

            var listItems = await _client.ListItem.GetManyAsync(listItemIds, new[] { ListItemResolveBehavior.Content });

            // Assert
            listItems.Should().HaveCount(2).And.Subject.Select(c => c.ConvertTo<Tag>().Name).Should().BeEquivalentTo(expectedName1, expectedName2);
        }

        [Fact]
        [Trait("Stack", "ListItem")]
        public async Task ShouldBatchUpdateWithoutWithoutWaitingSearchDocs()
        {
            // Arrange
            var listItem1CreateRequest = new ListItemCreateRequest { RequestId = $"{Guid.NewGuid():N}", ContentSchemaId = nameof(Tag), Content = new Tag { Name = $"{Guid.NewGuid():N}" } };
            var listItem2CreateRequest = new ListItemCreateRequest { RequestId = $"{Guid.NewGuid():N}", ContentSchemaId = nameof(Tag), Content = new Tag { Name = $"{Guid.NewGuid():N}" } };

            var result = await _client.ListItem.CreateManyAsync(new ListItemCreateManyRequest { Items = new[] { listItem1CreateRequest, listItem2CreateRequest } }, waitSearchDocCreation: false)
                ;

            var listItemIds = (await result.FetchDetail()).SucceededIds;

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

            await _client.ListItem.BatchUpdateFieldsByIdsAsync(updateRequest, waitSearchDocCreation: false);

            updateRequest.ChangeCommands.First().As<MetadataValuesSchemaUpdateCommand>().Value = new DataDictionary { { "name", expectedName } };
            await _client.ListItem.BatchUpdateFieldsByIdsAsync(updateRequest, waitSearchDocCreation: false);

            var listItems = await _client.ListItem.GetManyAsync(listItemIds, new[] { ListItemResolveBehavior.Content });

            // Assert
            listItems.Should().HaveCount(2).And.Subject.Select(c => c.ConvertTo<Tag>().Name).Should().OnlyContain(s => s == expectedName);
        }

        private async Task<ListItemDetail> CreateSoccerPlayer()
        {
            var originalPlayer = new SoccerPlayer { BirthDate = DateTime.Now, EmailAddress = "test@test.com", Firstname = "Test", LastName = "Soccerplayer" };

            var createRequest = new ListItemCreateRequest { ContentSchemaId = nameof(SoccerPlayer), Content = originalPlayer };

            var playerItem = await _client.ListItem.CreateAsync(createRequest, new[] { ListItemResolveBehavior.Content });
            return playerItem;
        }
    }
}
