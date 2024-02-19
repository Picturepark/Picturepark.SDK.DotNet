#pragma warning disable SA1201 // Elements must appear in the correct order

using FluentAssertions;
using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using Picturepark.SDK.V1.Tests.FluentAssertions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Picturepark.SDK.V1.AzureBlob;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Providers;
using Picturepark.SDK.V1.Contract.SystemTypes;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "Contents")]
    public class ContentTests : IClassFixture<ContentFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public ContentTests(ContentFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        public async Task ShouldTransferOwnership()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 50);

            // Act
            var previousContent = await _client.Content.GetAsync(contentId);
            var previousOwner = await _client.User.GetByOwnerTokenAsync(previousContent.OwnerTokenId);
            var searchResult = await _client.User.SearchAsync(new UserSearchRequest { Limit = 10, UserRightsFilter = new List<UserRight> { UserRight.ManageContent } });

            var newUser = searchResult.Results.First(u => u.Id != previousOwner.Id);
            var request = new ContentOwnershipTransferRequest
            {
                TransferUserId = newUser.Id
            };
            await _client.Content.TransferOwnershipAsync(contentId, request);

            var newContent = await _client.Content.GetAsync(contentId);
            var newOwner = await _client.User.GetByOwnerTokenAsync(newContent.OwnerTokenId);

            // Assert
            Assert.Equal(previousContent.Id, newContent.Id);
            Assert.NotEqual(previousContent.OwnerTokenId, newContent.OwnerTokenId);
            Assert.NotEqual(previousOwner.Id, newOwner.Id);
            Assert.Equal(newUser.Id, newOwner.Id);
        }

        [Fact]
        public async Task ShouldGetMany()
        {
            // Arrange
            var randomContents = await _fixture.GetRandomContentsAsync(".jpg", 2);
            var contentIds = randomContents.Results.Select(i => i.Id).ToList();

            // Act
            var contents = await _client.Content.GetManyAsync(contentIds, new[] { ContentResolveBehavior.Owner, ContentResolveBehavior.Permissions });

            // Assert
            Assert.Equal(2, contents.Count);
            Assert.Equal(contentIds[0], contents.ToList()[0].Id);
            Assert.Equal(contentIds[1], contents.ToList()[1].Id);

            contents.Should().NotContainNulls(i => i.Owner);
            contents.Should().NotContainNulls(i => i.ContentRights);

            contents.ToList().ForEach(content =>
                {
                    content.Audit.CreatedByUser.Should().BeResolved();
                    content.Audit.ModifiedByUser.Should().BeResolved();
                }
            );
        }

        [Fact]
        public async Task ShouldTransferOwnershipMany()
        {
            // Arrange
            var randomContents = await _fixture.GetRandomContentsAsync(".jpg", 2);
            var contentIds = randomContents.Results.Select(i => i.Id).ToList();

            // Act
            var previousContents = await _client.Content.GetManyAsync(contentIds);
            var previousOwner = await _client.User.GetByOwnerTokenAsync(previousContents.ToList()[0].OwnerTokenId);

            // Search user with ManageContent UserRight
            var searchResult = await _client.User.SearchAsync(new UserSearchRequest
            {
                Limit = 10,
                UserRightsFilter = new List<UserRight> { UserRight.ManageContent }
            });

            var newUser = searchResult.Results.FirstOrDefault(u => u.Id != previousOwner.Id);
            newUser.Should().NotBeNull($"expected to have more users with {UserRight.ManageContent} user right in the tested customer to test content ownership transfer");
            var manyRequest = new ContentOwnershipTransferManyRequest
            {
                Items = contentIds.Select(id => new ContentOwnershipTransferItem
                {
                    ContentId = id,
                    TransferUserId = newUser.Id
                }).ToList()
            };

            await _client.Content.TransferOwnershipManyAsync(manyRequest);

            var newContents = await _client.Content.GetManyAsync(contentIds);
            var newOwner1 = await _client.User.GetByOwnerTokenAsync(newContents.ToList()[0].OwnerTokenId);
            var newOwner2 = await _client.User.GetByOwnerTokenAsync(newContents.ToList()[1].OwnerTokenId);

            // Assert
            Assert.Equal(previousContents.ToList()[0].Id, newContents.ToList()[0].Id);
            Assert.Equal(previousContents.ToList()[1].Id, newContents.ToList()[1].Id);

            Assert.Equal(newOwner1.Id, newOwner2.Id);
            Assert.Equal(newUser.Id, newOwner1.Id);
        }

        [Fact]
        public async Task ShouldAggregateWithAggregators()
        {
            var request = new ContentAggregationRequest
            {
                SearchString = string.Empty,
                Aggregators = new List<AggregatorBase>
                {
                    new TermsAggregator { Name = "Aggregator1", Field = "contentType", Size = 10 }
                }
            };

            // Second Aggregator
            var ranges = new List<NumericRangeForAggregator>
            {
                new NumericRangeForAggregator { From = null, To = 499, Names = new TranslatedStringDictionary { { "en", "Aggregator2a" } } },
                new NumericRangeForAggregator { From = 500, To = 5000, Names = new TranslatedStringDictionary { { "en", "Aggregator2b" } } }
            };

            var numRangeAggregator = new NumericRangeAggregator()
            {
                Name = "NumberAggregator",
                Field = "Original.Width",
                Ranges = ranges
            };

            request.Aggregators.Add(numRangeAggregator);
            await _client.Content.AggregateAsync(request);
        }

        [Fact]
        public async Task ShouldSearchAndAggregateAllTogether()
        {
            var request = new ContentSearchRequest
            {
                SearchString = string.Empty,
                Aggregators = new List<AggregatorBase>
                {
                    new TermsAggregator { Name = "Aggregator1", Field = "contentType", Size = 10 }
                }
            };

            // Second Aggregator
            var ranges = new List<NumericRangeForAggregator>
            {
                new NumericRangeForAggregator { From = null, To = 499, Names = new TranslatedStringDictionary { { "en", "Aggregator2a" } } },
                new NumericRangeForAggregator { From = 500, To = 5000, Names = new TranslatedStringDictionary { { "en", "Aggregator2b" } } }
            };

            var numRangeAggregator = new NumericRangeAggregator()
            {
                Name = "NumberAggregator",
                Field = "Original.Width",
                Ranges = ranges
            };

            request.Aggregators.Add(numRangeAggregator);
            var result = await _client.Content.SearchAsync(request);
            result.Results.Should().HaveCountGreaterThan(0);
            result.AggregationResults.Should().HaveCount(2);
            foreach (var resultAggregationResult in result.AggregationResults)
            {
                resultAggregationResult.AggregationResultItems.Should().HaveCountGreaterThan(1);
            }
        }

        [Fact]
        public async Task ShouldAggregateByChannel()
        {
            // Arrange
            var channelId = "rootChannel";
            var request = new ContentAggregationRequest
            {
                ChannelId = channelId,
                SearchString = string.Empty
            };

            // Act
            var result = await _client.Content.AggregateOnChannelAsync(request);

            // Assert
            var originalWidthResults = result.AggregationResults
                .SingleOrDefault(i => i.Name == "Original Width");

            Assert.NotNull(originalWidthResults);
            Assert.True(originalWidthResults.AggregationResultItems.Count > 0);
        }

        [Fact]
        public async Task ShouldAggregateOnChannelWithTermsAggregator()
        {
            // Arrange
            var channelId = "rootChannel";
            var request = new ContentAggregationRequest
            {
                ChannelId = channelId,
                SearchString = string.Empty,
                Aggregators = new List<AggregatorBase>
                {
                    new TermsAggregator { Name = "Permissions", Field = "permissionSetIds", Size = 10 }
                }
            };

            // Act
            var result = await _client.Content.AggregateAsync(request);

            // Assert
            var permissionSetResults = result.AggregationResults
                .SingleOrDefault(i => i.Name == "Permissions");

            Assert.NotNull(permissionSetResults);
            Assert.True(permissionSetResults.AggregationResultItems.Count > 0);
        }

        [Fact]
        public async Task ShouldSortAggregationResults()
        {
            // Arrange
            var contentSchemaResult = await _client.Schema.CreateAsync(new SchemaCreateRequest
            {
                Id = $"ContentSchema{Guid.NewGuid():N}",
                Names = new TranslatedStringDictionary { { "en", "Test content schema" } },
                Types = new[] { SchemaType.Content },
                ViewForAll = true,
                LayerSchemaIds = new List<string> { nameof(PersonShot) },
                Fields = new[]
                {
                    new FieldString
                    {
                        Id = "name",
                        Index = true,
                        Names = new TranslatedStringDictionary { { "en", "Name" } }
                    }
                }
            });

            var contentSchema = contentSchemaResult.Schema;

            var peopleCreation = await _client.ListItem.CreateManyAsync(new ListItemCreateManyRequest
            {
                Items = new List<ListItemCreateRequest>
                {
                    new ListItemCreateRequest { ContentSchemaId = nameof(Person), Content = new Person { Firstname = "John", LastName = "Doe", EmailAddress = "john.doe@whatever.com" } },
                    new ListItemCreateRequest { ContentSchemaId = nameof(Person), Content = new Person { Firstname = "Max", LastName = "Frisch", EmailAddress = "max.frisch@whatever.com" } },
                    new ListItemCreateRequest { ContentSchemaId = nameof(Person), Content = new Person { Firstname = "Roger", LastName = "Federer", EmailAddress = "roger.federer@example.com" } },
                    new ListItemCreateRequest { ContentSchemaId = nameof(Person), Content = new Person { Firstname = "Carl", LastName = "Jung", EmailAddress = "carl.jung@example.com" } },
                }
            });

            var peopleCreationResult = await peopleCreation.FetchDetail();
            var people = peopleCreationResult.SucceededIds.Select(id => new Person { RefId = id }).ToArray();

            var content1 = await _client.Content.CreateAsync(
                new ContentCreateRequest
                {
                    ContentSchemaId = contentSchema.Id,
                    Content = new
                    {
                        name = "Content 1"
                    },
                    LayerSchemaIds = new List<string> { nameof(PersonShot) },
                    Metadata = Metadata.From(new PersonShot { Description = "test description", Persons = new[] { people[0], people[1] } })
                },
                waitSearchDocCreation: true);

            var content2 = await _client.Content.CreateAsync(
                new ContentCreateRequest
                {
                    ContentSchemaId = contentSchema.Id,
                    Content = new
                    {
                        name = "Content 2"
                    },
                    LayerSchemaIds = new List<string> { nameof(PersonShot) },
                    Metadata = Metadata.From(new PersonShot { Description = "test description", Persons = new[] { people[1], people[2], people[3] } })
                },
                waitSearchDocCreation: true);

            var request = new ContentAggregationRequest
            {
                Filter = new TermFilter { Field = nameof(Content.ContentSchemaId).ToLowerCamelCase(), Term = contentSchema.Id },
                Aggregators = new List<AggregatorBase>
                {
                    new NestedAggregator
                    {
                        Name = "PeopleAggregation",
                        Path = $"{nameof(PersonShot).ToLowerCamelCase()}.persons",
                        Aggregators = new List<AggregatorBase>
                        {
                            new TermsRelationAggregator
                            {
                                DocumentType = TermsRelationAggregatorDocumentType.ListItem,
                                Name = "PeopleAggregationInner",
                                Field = $"{nameof(PersonShot).ToLowerCamelCase()}.persons._refId",
                                Size = 10,
                                Sort = new SortInfo
                                {
                                    Direction = SortDirection.Asc,
                                    Field = $"{nameof(PersonShot).ToLowerCamelCase()}.persons._displayValues.name",
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var result = await _client.Content.AggregateAsync(request);

            // Assert
            try
            {
                result.AggregationResults.First().AggregationResultItems.First().AggregationResults.First()
                    .AggregationResultItems.Select(ari => ari.Name).Should().Equal(
                        "Carl Jung",
                        "John Doe",
                        "Max Frisch",
                        "Roger Federer"
                    );
            }
            finally
            {
                await _client.Content.DeleteManyAsync(new ContentDeleteManyRequest
                {
                    ContentIds = new[] { content1.Id, content2.Id }
                });

                await _client.ListItem.DeleteManyAsync(new ListItemDeleteManyRequest
                {
                    ListItemIds = peopleCreationResult.SucceededIds.ToArray()
                });
            }
        }

        [Fact]
        public async Task ShouldSearchAndAggregateOnChannelWithTermsAggregator()
        {
            // Arrange
            var channelId = "rootChannel";
            var request = new ContentSearchRequest
            {
                ChannelId = channelId,
                SearchString = string.Empty,
                Aggregators = new List<AggregatorBase>
                {
                    new TermsAggregator { Name = "Permissions", Field = "permissionSetIds", Size = 10 }
                }
            };

            // Act
            var result = await _client.Content.SearchAsync(request);

            // Assert
            result.Results.Should().HaveCountGreaterThan(0);
            var permissionSetAggregationResults = result.AggregationResults.SingleOrDefault(i => i.Name == "Permissions");
            permissionSetAggregationResults.Should().NotBeNull();
            permissionSetAggregationResults.AggregationResultItems.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task ShouldCreateDownloadLinkForSingleFile()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 50);
            var createDownloadLinkRequest = new ContentDownloadLinkCreateRequest
            {
                Contents = new List<ContentDownloadRequestItem>
                {
                    new ContentDownloadRequestItem { ContentId = contentId, OutputFormatId = "Original" }
                }
            };

            // Act
            var result = await _client.Content.CreateAndAwaitDownloadLinkAsync(createDownloadLinkRequest);
            Assert.NotNull(result.DownloadUrl);

            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync(result.DownloadUrl))
            {
                response.EnsureSuccessStatusCode();

                var fileName = response.Content.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                Assert.EndsWith(".jpg", fileName);

                var filePath = Path.Combine(_fixture.TempDirectory, fileName);

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = File.Create(filePath))
                {
                    await stream.CopyToAsync(fileStream);

                    // Assert
                    Assert.True(stream.Length > 10);
                }
            }
        }

        [Fact]
        [Trait("Stack", "Contents")]
        public async Task ShouldCreateDownloadLinkForMultipeFiles()
        {
            // Arrange
            var randomContents = await _fixture.GetRandomContentsAsync(".jpg", 2);
            var contentIds = randomContents.Results.Select(i => i.Id).ToList();

            var createDownloadLinkRequest = new ContentDownloadLinkCreateRequest
            {
                Contents = new List<ContentDownloadRequestItem>
                {
                    new ContentDownloadRequestItem { ContentId = contentIds[0], OutputFormatId = "Original" },
                    new ContentDownloadRequestItem { ContentId = contentIds[1], OutputFormatId = "Original" }
                }
            };

            // Act
            var result = await _client.Content.CreateAndAwaitDownloadLinkAsync(createDownloadLinkRequest);
            Assert.NotNull(result.DownloadUrl);

            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync(result.DownloadUrl))
            {
                response.EnsureSuccessStatusCode();

                var fileName = response.Content.Headers.ContentDisposition.FileName;
                Assert.EndsWith(".zip", fileName);

                var filePath = Path.Combine(_fixture.TempDirectory, fileName);

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = File.Create(filePath))
                {
                    await stream.CopyToAsync(fileStream);

                    // Assert
                    Assert.True(stream.Length > 10);
                }
            }
        }

        [Fact]
        public async Task ShouldCreateContent()
        {
            // Arrange
            var request = new ContentCreateRequest
            {
                Content = JsonConvert.DeserializeObject(@"{ ""name"": ""foo"" }"),
                ContentSchemaId = "ContentItem"
            };

            // Act
            var result = await _client.Content.CreateAsync(request);

            // Assert
            Assert.NotNull(result);

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        public async Task ShouldCreateContentWithRequestId()
        {
            // Arrange
            var request = new ContentCreateRequest
            {
                RequestId = "request",
                Content = JsonConvert.DeserializeObject(@"{ ""name"": ""foo"" }"),
                ContentSchemaId = "ContentItem"
            };

            // Act
            var result = await _client.Content.CreateManyAsync(
                new ContentCreateManyRequest
                {
                    Items = new List<ContentCreateRequest> { request },
                    AllowMissingDependencies = false
                });

            // Assert
            Assert.NotNull(result);

            var detail = await result.FetchDetail();
            var item = detail.SucceededItems.Should().ContainSingle(r => r.RequestId == "request").Which;
            item.Item.ContentSchemaId.Should().Be("ContentItem");
        }

        [Fact]
        public async Task ShouldCreateContents()
        {
            // Arrange
            var request1 = new ContentCreateRequest
            {
                Content = JsonConvert.DeserializeObject(@"{ ""name"": ""foo"" }"),
                ContentSchemaId = "ContentItem"
            };

            var request2 = new ContentCreateRequest
            {
                Content = JsonConvert.DeserializeObject(@"{ ""name"": ""bar"" }"),
                ContentSchemaId = "ContentItem"
            };

            // Act
            var result = await _client.Content.CreateManyAsync(new ContentCreateManyRequest
            {
                AllowMissingDependencies = false,
                Items = new List<ContentCreateRequest> { request1, request2 }
            });

            var detail = await result.FetchDetail();

            // Assert
            detail.FailedItems.Should().BeNullOrEmpty();
            detail.SucceededItems.Should().HaveCount(2);
        }

        [Fact]
        public async Task ShouldAllowMultiTagboxExtraction()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 1);
            var listItems = await _fixture.Client.ListItem.SearchAsync(new ListItemSearchRequest()
            {
                Filter = FilterBase.FromExpression<ListItem>(s => s.ContentSchemaId, nameof(SimpleReferenceObject)),
                Limit = 1
            });
            var listItemId = listItems.Results.First().Id;

            var assignLayerWithTagbox = new ContentMetadataUpdateRequest
            {
                LayerSchemaIds = new List<string>() { nameof(AllDataTypesContract) },
                Metadata = Metadata.From(
                    new AllDataTypesContract
                    {
                        MultiTagboxField = new List<SimpleReferenceObject> { new SimpleReferenceObject { RefId = listItemId } }
                    }),
                LayerFieldsUpdateOptions = UpdateOption.Replace
            };

            // Act
            var withTagboxLayer = await _client.Content
                .UpdateMetadataAsync(contentId, assignLayerWithTagbox, new[] { ContentResolveBehavior.Metadata })
                ;

            var listOfRefObjects = withTagboxLayer.Layer<AllDataTypesContract>().MultiTagboxField;

            // Assert
            listOfRefObjects.Should().HaveCount(1);
            listOfRefObjects.Single().RefId.Should().Be(listItemId);
        }

        [Fact]
        public async Task ShouldDownloadMultiple()
        {
            int maxNumberOfDownloadFiles = 3;
            string searchString = string.Empty;

            ContentSearchResult result = await _fixture.GetRandomContentsAsync(searchString, maxNumberOfDownloadFiles, new[] { ContentType.Bitmap, ContentType.TextDocument })
                ;
            Assert.True(result.Results.Count > 0);

            await _client.Content.DownloadFilesAsync(
                result,
                _fixture.TempDirectory,
                true,
                successDelegate: (content) =>
                {
                    Console.WriteLine(content.GetFileMetadata().FileName);
                },
                errorDelegate: Console.WriteLine);
        }

        [Fact]
        public async Task ShouldDownloadSingle()
        {
            string contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            Assert.False(string.IsNullOrEmpty(contentId));
            ContentDetail contentDetail = await _client.Content.GetAsync(contentId, new[] { ContentResolveBehavior.Content });

            var fileMetadata = contentDetail.GetFileMetadata();
            var fileName = new Random().Next(0, 999999) + "-" + fileMetadata.FileName + ".jpg";
            var filePath = Path.Combine(_fixture.TempDirectory, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            using (var response = await _client.Content.DownloadAsync(contentId, "Original", null, null, "bytes=0-20000000"))
            {
                response.GetFileName().Should().Be(fileMetadata.FileName);

                var stream = response.Stream;
                Assert.True(stream.CanRead);

                await response.Stream.WriteToFileAsync(filePath);
                Assert.True(File.Exists(filePath));
            }
        }

        [Theory,
         InlineData(ThumbnailSize.Small),
         InlineData(ThumbnailSize.Medium),
         InlineData(ThumbnailSize.Large),
         InlineData(ThumbnailSize.Preview)]
        public async Task ShouldDownloadThumbnail(ThumbnailSize size)
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);

            // Act
            using (var response = await _client.Content.DownloadThumbnailAsync(contentId, size))
            {
                // Assert
                await AssertFileResponseOkAndNonEmpty(response, "image/jpeg");
            }
        }

        [Theory,
         InlineData(ThumbnailSize.Small),
         InlineData(ThumbnailSize.Medium),
         InlineData(ThumbnailSize.Large),
         InlineData(ThumbnailSize.Preview)]
        public async Task ShouldDownloadFileFormatIconIfOutputForThumbnailNotRendered(ThumbnailSize size)
        {
            // Arrange
            var planetJsonPath = Path.Combine(_fixture.ExampleSchemaBasePath, "Planet.json");
            var importResult = await _client.Ingest.UploadAndImportFilesAsync(new[] { planetJsonPath });
            var details = await importResult.FetchDetail();

            var contentId = details.SucceededIds.Single();

            // Act
            using var response = await _client.Content.DownloadThumbnailAsync(contentId, size);

            // Assert
            await AssertFileResponseOkAndNonEmpty(response, "image/svg+xml");
        }

        [Fact]
        public async Task ShouldUpdateMetadata()
        {
            // Arrange
            var expectedName = "test" + new Random().Next(0, 999999);
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            var request = new ContentMetadataUpdateRequest
            {
                LayerSchemaIds = new List<string> { nameof(SimpleLayer) },
                Metadata = Metadata.From(
                    new SimpleLayer
                    {
                        Name = expectedName
                    })
            };

            // Act
            var response = await _client.Content.UpdateMetadataAsync(contentId, request, new[] { ContentResolveBehavior.Metadata });

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedName, response.Layer<SimpleLayer>().Name);

            response.Audit.CreatedByUser.Should().BeResolved();
            response.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        public async Task ShouldUpdateMetadataMany()
        {
            // Arrange
            var randomContents = await _fixture.GetRandomContentsAsync(".jpg", 2);
            var contentIds = randomContents.Results.Select(i => i.Id).ToList();

            var request1 = new ContentMetadataUpdateItem
            {
                Id = contentIds[0],
                LayerSchemaIds = new List<string> { nameof(SimpleLayer) },
                Metadata = Metadata.From(new SimpleLayer { Name = "Content1" })
            };

            var request2 = new ContentMetadataUpdateItem
            {
                Id = contentIds[1],
                LayerSchemaIds = new List<string> { nameof(SimpleLayer) },
                Metadata = Metadata.From(new SimpleLayer { Name = "Content2" })
            };

            // Act
            var result = await _client.Content.UpdateMetadataManyAsync(new ContentMetadataUpdateManyRequest
            {
                AllowMissingDependencies = false,
                Items = new List<ContentMetadataUpdateItem> { request1, request2 }
            });

            // Assert
            Assert.Equal(BusinessProcessLifeCycle.Succeeded, result.LifeCycle);
        }

        [Fact]
        public async Task ShouldSetLayerAndResolveDisplayValues()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            var request =
                ContentMetadataUpdateRequest.LayerMergeUpdate((PersonShot ps) => ps.Description, "test description");

            // Act
            var response = await _client.Content.UpdateMetadataAsync(contentId, request, new[] { ContentResolveBehavior.Metadata, ContentResolveBehavior.InnerDisplayValueName });

            // Assert
            Assert.Equal("test description", response.LayerDisplayValues<PersonShot>().Name);
        }

        [Fact]
        public async Task ShouldMergeLayersOnMetadataUpdate()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            var request = new ContentMetadataUpdateRequest
            {
                LayerSchemaIds = new List<string> { nameof(PersonShot) },
                Metadata = Metadata.From(new PersonShot { Description = "test description" })
            };

            await _client.Content.UpdateMetadataAsync(contentId, request);

            request = new ContentMetadataUpdateRequest
            {
                LayerSchemaIds = new List<string> { nameof(AllDataTypesContract) },
                Metadata = Metadata.From(new AllDataTypesContract { IntegerField = 12345 }),
                LayerSchemasUpdateOptions = UpdateOption.Merge
            };

            // Act
            var response = await _client.Content.UpdateMetadataAsync(contentId, request, new[] { ContentResolveBehavior.Metadata });

            // Assert
            Assert.Equal("test description", response.Layer<PersonShot>().Description);
            Assert.Equal(12345, response.Layer<AllDataTypesContract>().IntegerField);
        }

        [Fact]
        public async Task ShouldReplaceLayersOnMetadataUpdate()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            var request = new ContentMetadataUpdateRequest
            {
                LayerSchemaIds = new List<string> { nameof(PersonShot) },
                Metadata = Metadata.From(new PersonShot { Description = "test description" })
            };

            var contentDetail = await _client.Content.UpdateMetadataAsync(contentId, request);
            var layerIds = contentDetail.LayerSchemaIds.ToList();
            layerIds.Remove(nameof(PersonShot));
            layerIds.Add(nameof(AllDataTypesContract));

            request = new ContentMetadataUpdateRequest
            {
                LayerSchemaIds = layerIds,
                Metadata = Metadata.From(new AllDataTypesContract { IntegerField = 12345 }),
                LayerSchemasUpdateOptions = UpdateOption.Replace
            };

            // Act
            var response = await _client.Content.UpdateMetadataAsync(contentId, request, new[] { ContentResolveBehavior.Metadata });

            // Assert
            response.HasLayer<PersonShot>().Should().BeFalse();
            Assert.Equal(12345, response.Layer<AllDataTypesContract>().IntegerField);
        }

        [Fact]
        public async Task ShouldReplaceContentOnMetadataUpdate()
        {
            // Arrange
            var content = await CreateContentItem();

            // Act
            var request = new ContentMetadataUpdateRequest
            {
                Content = new object(),
                ContentFieldsUpdateOptions = UpdateOption.Replace
            };

            var contentDetail = await _client.Content.UpdateMetadataAsync(content.Id, request, new[] { ContentResolveBehavior.Content });

            // Assert
            contentDetail.ContentAs<ContentItem>().Name.Should().BeNull();
        }

        [Fact]
        public async Task ShouldMergeFieldsOnMetadataUpdate()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            var request = new ContentMetadataUpdateRequest
            {
                LayerSchemaIds = new List<string> { nameof(AllDataTypesContract) },
                Metadata = Metadata.From(new AllDataTypesContract { IntegerField = 12345 })
            };

            await _client.Content.UpdateMetadataAsync(contentId, request);

            request = ContentMetadataUpdateRequest.LayerMergeUpdate((AllDataTypesContract a) => a.StringField, "test string");

            // Act
            var response = await _client.Content.UpdateMetadataAsync(contentId, request, new[] { ContentResolveBehavior.Metadata });

            // Assert
            Assert.Equal(12345, response.Layer<AllDataTypesContract>().IntegerField);
            Assert.Equal("test string", response.Layer<AllDataTypesContract>().StringField);
        }

        [Fact]
        public async Task ShouldReplaceFieldsOnMetadataUpdate()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            var request = new ContentMetadataUpdateRequest
            {
                LayerSchemaIds = new List<string> { nameof(AllDataTypesContract) },
                Metadata = Metadata.From(new AllDataTypesContract { IntegerField = 12345 })
            };

            await _client.Content.UpdateMetadataAsync(contentId, request);

            request = new ContentMetadataUpdateRequest
            {
                LayerSchemaIds = new List<string> { nameof(AllDataTypesContract) },
                Metadata = Metadata.From(new AllDataTypesContract { StringField = "test string" }),
                LayerFieldsUpdateOptions = UpdateOption.Replace
            };

            // Act
            var response = await _client.Content.UpdateMetadataAsync(contentId, request, new[] { ContentResolveBehavior.Metadata });

            // Assert
            Assert.Null(response.Layer("allDataTypesContract")["integerField"]);
            Assert.Equal("test string", response.Layer<AllDataTypesContract>().StringField);
        }

        [Fact]
        public async Task ShouldCreateContentWithTriggerFieldInLayerAndTriggerItOnMetadataUpdate()
        {
            // Arrange
            var (contentSchema, layerSchema) = await CreateSchemasForTriggerTests();

            var contentCreateRequest = new ContentCreateRequest
            {
                ContentSchemaId = contentSchema.Id,
                Content = new object(),
                LayerSchemaIds = new[] { layerSchema.Id }
            };

            var contentDetail = await _client.Content.CreateAsync(contentCreateRequest);

            // Act
            var updateContentRequest = new ContentMetadataUpdateRequest
            {
                LayerSchemaIds = new[] { layerSchema.Id },
                Metadata = Metadata.From(
                    layerSchema.Id.ToLowerCamelCase(),
                    new
                    {
                        fieldTrigger = new { _trigger = true }
                    })
            };
            contentDetail = await _client.Content.UpdateMetadataAsync(contentDetail.Id, updateContentRequest, new[] { ContentResolveBehavior.Metadata });

            // Assert
            var fieldTrigger = contentDetail.Layer(layerSchema.Id)["fieldTrigger"];
            fieldTrigger.Should().NotBeNull();
            var triggeredBy = fieldTrigger["triggeredBy"];
            triggeredBy["id"].Should().NotBeNull();
            triggeredBy["firstName"].Should().NotBeNull();
            triggeredBy["lastName"].Should().NotBeNull();
            triggeredBy["emailAddress"].Should().NotBeNull();
            contentDetail.Layer(layerSchema.Id)["fieldTrigger"]["triggeredOn"].Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldSimpleSearchOnInnerTriggeredOnFieldOfTriggerFieldOfLayer()
        {
            // Arrange
            var (contentSchema, layerSchema) = await CreateSchemasForTriggerTests();

            var contentCreateRequest1 = new ContentCreateRequest
            {
                ContentSchemaId = contentSchema.Id,
                Content = new object(),
                LayerSchemaIds = new[] { layerSchema.Id },
                Metadata = Metadata.From(
                    layerSchema.Id.ToLowerCamelCase(),
                    new
                    {
                        fieldTrigger = new { _trigger = true }
                    })
            };

            var contentCreateRequest2 = new ContentCreateRequest
            {
                ContentSchemaId = contentSchema.Id,
                Content = new object(),
                LayerSchemaIds = new[] { layerSchema.Id }
            };

            var contentCreateResult = await _client.Content.CreateManyAsync(new ContentCreateManyRequest { Items = new[] { contentCreateRequest1, contentCreateRequest2 } });
            var contentIds = (await contentCreateResult.FetchDetail()).SucceededIds;

            // Act: reduce the possible contents to the two created (with a filter), and then perform a simple search taking the expected date part and requesting to append an asterisk: it should match the triggeredOn inner field
            // of the trigger field
            var searchRequest = new ContentSearchRequest
            {
                Filter = new AndFilter
                {
                    Filters = new FilterBase[]
                    {
                        new TermsFilter { Field = "id", Terms = contentIds.ToArray() }
                    }
                },
                SearchString = DateTime.UtcNow.Date.ToString("yyyy-MM-dd"),
                SearchBehaviors = new[] { SearchBehavior.WildcardOnSingleTerm }
            };

            var searchResult = await _client.Content.SearchAsync(searchRequest);

            // Assert
            searchResult.Results.Should().HaveCount(1).And.Subject.First().Id.Should().Be(contentIds[0]);
        }

        [Fact]
        public async Task ShouldFilterOnInnerTriggeredByFieldOfTriggerFieldOfLayer()
        {
            // Arrange
            var profile = await _client.Profile.GetAsync();
            var userId = profile.Id;

            var (contentSchema, layerSchema) = await CreateSchemasForTriggerTests();

            var contentCreateRequest1 = new ContentCreateRequest
            {
                ContentSchemaId = contentSchema.Id,
                Content = new object(),
                LayerSchemaIds = new[] { layerSchema.Id },
                Metadata = Metadata.From(
                    layerSchema.Id.ToLowerCamelCase(),
                    new
                    {
                        fieldTrigger = new { _trigger = true }
                    })
            };

            var contentCreateRequest2 = new ContentCreateRequest
            {
                ContentSchemaId = contentSchema.Id,
                LayerSchemaIds = new[] { layerSchema.Id }
            };

            var contentCreateResult = await _client.Content.CreateManyAsync(new ContentCreateManyRequest { Items = new[] { contentCreateRequest1, contentCreateRequest2 } });
            var contentIds = (await contentCreateResult.FetchDetail()).SucceededIds;

            // Act: reduce the possible contents to the two created (filter), and then add a filter for the expected user to have triggered the content: it should match the triggeredBy inner field
            // of the trigger field
            var searchRequest = new ContentSearchRequest
            {
                Filter = new AndFilter
                {
                    Filters = new FilterBase[]
                    {
                        new TermsFilter { Field = "id", Terms = contentIds.ToArray() },
                        new TermFilter { Field = $"{layerSchema.Id.ToLowerCamelCase()}.fieldTrigger.triggeredBy", Term = userId }
                    }
                }
            };

            var searchResult = await _client.Content.SearchAsync(searchRequest);

            // Assert
            searchResult.Results.Should().HaveCount(1).And.Subject.First().Id.Should().Be(contentIds[0]);
        }

        [Fact]
        public async Task ShouldBatchUpdateFieldsByFilter()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            var request = new ContentFieldsBatchUpdateFilterRequest
            {
                FilterRequest = new ContentFilterRequest
                {
                    ChannelId = "rootChannel",
                    Filter = new TermFilter { Field = "id", Term = contentId }
                },
                ChangeCommands = new List<MetadataValuesChangeCommandBase>
                {
                    new MetadataValuesSchemaUpsertCommand
                    {
                        SchemaId = nameof(SimpleLayer),
                        Value = new SimpleLayer
                        {
                            Name = "testlocation"
                        }
                    }
                }
            };

            // Act
            var result = await _client.Content.BatchUpdateFieldsByFilterAsync(request);

            // Assert
            Assert.True(result.LifeCycle == BusinessProcessLifeCycle.Succeeded);
        }

        [Fact]
        public async Task ShouldBatchUpdateFieldsByIds()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);

            var content = await _client.Content.GetAsync(contentId);
            var updateRequest = new ContentFieldsBatchUpdateRequest
            {
                ContentIds = new List<string> { content.Id },
                ChangeCommands = new List<MetadataValuesChangeCommandBase>
                {
                    new MetadataValuesSchemaUpsertCommand
                    {
                        SchemaId = nameof(SimpleLayer),
                        Value = new SimpleLayer
                        {
                            Name = "testlocation"
                        }
                    }
                }
            };

            // Act
            var result = await _client.Content.BatchUpdateFieldsByIdsAsync(updateRequest);

            // Assert
            Assert.True(result.LifeCycle == BusinessProcessLifeCycle.Succeeded);
        }

        [Fact]
        public async Task ShouldThrowExceptionWhenContentNotFound()
        {
            var contentId = "foobar.baz";
            await Assert.ThrowsAsync<ContentNotFoundException>(async () =>
            {
                await _client.Content.GetAsync(contentId);
            });
        }

        [Fact]
        public async Task ShouldDownloadSingleResized()
        {
            // Arrange
            var resizeTarget = 200;

            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            contentId.Should().NotBeNullOrEmpty();

            var contentDetail = await _client.Content.GetAsync(contentId, new[] { ContentResolveBehavior.Content });

            var imageMetadata = contentDetail.ContentAs<ImageMetadata>();
            var fileName = nameof(ShouldDownloadSingleResized) + new Random().Next(0, 999999) + "-" + imageMetadata.FileName + ".jpg";
            var filePath = Path.Combine(_fixture.TempDirectory, fileName);

            var sourceAspectRatio = (float)imageMetadata.Width / imageMetadata.Height;

            if (File.Exists(filePath))
                File.Delete(filePath);

            // Act
            using (var response = await _client.Content.DownloadAsync(contentId, "Original", resizeTarget, resizeTarget))
            {
                await response.Stream.WriteToFileAsync(filePath);
            }

            // Assert
            File.Exists(filePath).Should().BeTrue();

            using (var bitmap = new Bitmap(filePath))
            {
                Math.Max(bitmap.Width, bitmap.Height).Should().Be(resizeTarget, "should resize to target");

                var resizedAspectRatio = (float)bitmap.Width / bitmap.Height;
                resizedAspectRatio.Should().BeInRange(
                    0.98f * sourceAspectRatio,
                    1.02f * sourceAspectRatio,
                    "should keep aspect ratio");
            }
        }

        [Fact]
        public async Task ShouldEditContent()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            contentId.Should().NotBeNullOrEmpty();

            // Act
            using (var response = await _client.Content.EditOutputAsync(contentId, "Preview", "resize-to:200x200"))
            {
                // Assert
                var bitmap = new Bitmap(response.Stream);
                bitmap.Width.Should().Be(200);
                bitmap.Height.Should().Be(200);
            }
        }

        [Fact]
        public async Task ShouldEditAndCropToFocalPoint()
        {
            // Arrange
            var contentIds = await UploadAndImportContents(searchPattern: "0396_1ZvuywK5v6s.jpg");
            var contentId = contentIds.Single();

            var contentDetail = await _client.Content.GetAsync(contentId, new[] { ContentResolveBehavior.Content });
            var imageMetadata = (ImageMetadata)contentDetail.GetFileMetadata();

            // Focal points are given with relative coordinates so that they can be leveraged with multiple output formats
            var topLeft = new PointF(x: 0.3f, y: 0.2f);
            var bottomRight = new PointF(x: 0.4f, y: 0.4f);

            // assign a focal point manually. this could also be done using a tagging service
            var imageAnalyticsLayerId = "ImageAnalytics";
            var focalPointIdentifier = "testPoint";

            await _client.Content.UpdateMetadataAsync(
                contentId,
                new ContentMetadataUpdateRequest()
                {
                    LayerSchemaIds = new List<string> { imageAnalyticsLayerId },
                    Metadata = new Dictionary<string, object>()
                    {
                        [imageAnalyticsLayerId.ToLowerCamelCase()] = new Dictionary<string, object>()
                        {
                            ["focalPoints"] = new List<object>
                            {
                                new
                                {
                                    identifier = focalPointIdentifier,
                                    confidenceLevel = 0.75,
                                    source = new
                                    {
                                        _refId = "d61e3e2376fe439d866bb71839dc832c" // "User defined"
                                    },

                                    // of type FocalPointCoordinate
                                    position = new List<object>
                                    {
                                        new { x = topLeft.X, y = topLeft.Y },
                                        new { x = bottomRight.X, y = topLeft.Y },
                                        new { x = bottomRight.X, y = bottomRight.Y },
                                        new { x = topLeft.X, y = bottomRight.Y }
                                    }
                                }
                            }
                        }
                    }
                });

            var fileName = nameof(ShouldEditAndCropToFocalPoint) + new Random().Next(0, 999999) + "-" + imageMetadata.FileName;
            var filePath = Path.Combine(_fixture.TempDirectory, fileName);

            // crop to our focal point and automatically derive size of cropping rectangle
            var conversionPreset = $"crop:auto,fp:{focalPointIdentifier}";

            // usually it is advisable to use Preview or another smaller outputFormat in favor of Original
            using (var downloadedImage = await _client.Content.EditOutputAsync(contentId, "Original", conversionPreset))
            {
                await downloadedImage.Stream.WriteToFileAsync(filePath);
            }

            // Assert
            File.Exists(filePath).Should().BeTrue();

            var expectedHeight = (bottomRight.Y - topLeft.Y) * imageMetadata.Height;
            var expectedWidth = (bottomRight.X - topLeft.X) * imageMetadata.Width;

            using (var bitmap = new Bitmap(filePath))
            {
                bitmap.Height.Should().BeInRange((int)(0.98f * expectedHeight), (int)(1.02f * expectedHeight));
                bitmap.Width.Should().BeInRange((int)(0.98f * expectedWidth), (int)(1.02f * expectedWidth));
            }
        }

        [Fact]
        public async Task ShouldGet()
        {
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            Assert.False(string.IsNullOrEmpty(contentId));

            ContentDetail result = await _client.Content.GetAsync(contentId, new[]
            {
                ContentResolveBehavior.InnerDisplayValueList,
                ContentResolveBehavior.Owner,
                ContentResolveBehavior.Permissions
            });

            result.Id.Should().NotBeNullOrEmpty();
            result.Owner.Should().NotBeNull();
            result.ContentRights.Should().NotBeNull();
            result.ContentRights.Should().NotBeEmpty();

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        public async Task ShouldGetDocumentMetadata()
        {
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);

            if (string.IsNullOrEmpty(contentId))
                contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.docx", 20);

            Assert.False(string.IsNullOrEmpty(contentId));

            ContentDetail result = await _client.Content.GetAsync(contentId, new[] { ContentResolveBehavior.Content });

            FileMetadata fileMetadata = result.GetFileMetadata();
            Assert.False(string.IsNullOrEmpty(fileMetadata.FileName));
        }

        [Fact]
        public async Task ShouldGetVectorMetadata()
        {
            // sample001.ai
            var contentId = await _fixture.GetRandomContentIdAsync("contentType:VectorGraphic", 1);
            contentId.Should().NotBeNullOrEmpty();

            ContentDetail result = await _client.Content.GetAsync(contentId, new[] { ContentResolveBehavior.Content });

            var metadata = result.GetFileMetadata().As<VectorMetadata>();
            metadata.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldGetWithResolvedObjects()
        {
            var contentDetail = await CreateContentReferencingSimpleField(ContentResolveBehavior.Content, ContentResolveBehavior.LinkedListItems);

            // Assert
            contentDetail.ContentAs<ContentItemWithTagBox>().Object.NameField.Should().Be("simpleField");
        }

        [Fact]
        public async Task ShouldGetWithoutResolvedObjects()
        {
            var contentDetail = await CreateContentReferencingSimpleField(ContentResolveBehavior.Content);

            // Assert
            contentDetail.ContentAs<ContentItemWithTagBox>().Object.NameField.Should().BeNull();
        }

        [Fact]
        public async Task ShouldSearch()
        {
            // Arrange
            var channelId = "rootChannel";
            var searchFieldPath =
                nameof(ContentDetail.Audit).ToLowerCamelCase() + "." +
                nameof(UserAudit.CreationDate).ToLowerCamelCase();

            var sortInfos = new List<SortInfo>
            {
                new SortInfo { Direction = SortDirection.Asc, Field = searchFieldPath }
            };

            var filter = new TermFilter { Field = "contentSchemaId", Term = "ImageMetadata" };
            var request = new ContentSearchRequest
            {
                ChannelId = channelId,
                Sort = sortInfos,
                Filter = filter,
            };

            // Act
            ContentSearchResult result = await _client.Content.SearchAsync(request);

            // Assert
            result.Results.Count.Should().BeGreaterThan(0);
            result.Results.First().LifeCycle.Should().Be(LifeCycle.Active);
        }

        [Fact]
        public async Task ShouldSearchByChannelAndScrollThroughResults()
        {
            string channelId = "rootChannel";

            var sortInfos = new List<SortInfo>
            {
                new SortInfo { Direction = SortDirection.Asc, Field = "audit.creationDate" }
            };

            var request = new ContentSearchRequest
            {
                ChannelId = channelId,
                Sort = sortInfos,
                Limit = 5
            };

            int i = 0;
            ContentSearchResult result;

            do
            {
                result = await _client.Content.SearchAsync(request);
                result.Results.Should().NotBeEmpty();
            }
            while (++i < 3 && ((result.PageToken = result.PageToken) != null));
        }

        [Fact]
        public async Task ShouldSearchAndResolveContentRights()
        {
            // Arrange
            var channelId = "rootChannel";
            var filter = new TermFilter { Field = "contentSchemaId", Term = "ImageMetadata" };
            var request = new ContentSearchRequest { ChannelId = channelId, Filter = filter, ResolveBehaviors = new[] { ContentSearchResolveBehavior.Permissions } };

            // Act
            ContentSearchResult result = await _client.Content.SearchAsync(request);

            // Assert
            result.Results.Count.Should().BeGreaterThan(0);
            result.Results.Should().OnlyContain(c => c.ContentRights != null);
        }

        [Fact]
        public async Task ShouldDeleteAndRestoreContent()
        {
            // Arrange
            var request = new ContentCreateRequest
            {
                Content = JsonConvert.DeserializeObject(@"{ ""name"": ""contentToTrash"" }"),
                ContentSchemaId = "ContentItem"
            };

            // Act
            var content = await _client.Content.CreateAsync(request);

            // Deactivate
            await _client.Content.DeleteAsync(content.Id);
            await Assert.ThrowsAsync<ContentNotFoundException>(async () => await _client.Content.GetAsync(content.Id));

            // Reactivate
            await _client.Content.RestoreAsync(content.Id, timeout: TimeSpan.FromMinutes(1));

            // Assert
            Assert.NotNull(await _client.Content.GetAsync(content.Id));
        }

        [Fact]
        public async Task ShouldDeleteAndRestoreContentMany()
        {
            // Arrange
            var content1 = await _client.Content.CreateAsync(new ContentCreateRequest
            {
                Content = JsonConvert.DeserializeObject(@"{ ""name"": ""contentToTrashMany1"" }"),
                ContentSchemaId = "ContentItem"
            });

            var content2 = await _client.Content.CreateAsync(new ContentCreateRequest
            {
                Content = JsonConvert.DeserializeObject(@"{ ""name"": ""contentToTrashMany2"" }"),
                ContentSchemaId = "ContentItem"
            });

            var contentIds = new List<string> { content1.Id, content2.Id };

            // Deactivate
            var deactivationRequest = new ContentDeleteManyRequest()
            {
                ContentIds = contentIds
            };

            var businessProcess = await _client.Content.DeleteManyAsync(deactivationRequest);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            await Assert.ThrowsAsync<ContentNotFoundException>(async () => await _client.Content.GetAsync(contentIds[0]));
            await Assert.ThrowsAsync<ContentNotFoundException>(async () => await _client.Content.GetAsync(contentIds[1]));

            // Reactivate
            var reactivateRequest = new ContentRestoreManyRequest()
            {
                ContentIds = contentIds
            };

            businessProcess = await _client.Content.RestoreManyAsync(reactivateRequest);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            // Assert
            Assert.NotNull(await _client.Content.GetAsync(contentIds[0]));
            Assert.NotNull(await _client.Content.GetAsync(contentIds[1]));
        }

        [Fact]
        public async Task ShouldCreateUpdateDeleteAndRestoreContentWithoutWaitingSearchDocs()
        {
            // Act
            // Create
            var request = new ContentCreateRequest
            {
                Content = new { Name = $"{Guid.NewGuid():N}" },
                ContentSchemaId = nameof(ContentItem)
            };

            var content = await _client.Content.CreateAsync(request, waitSearchDocCreation: false);

            // Update
            var updatedName = $"{Guid.NewGuid():N}";
            await _client.Content.UpdateMetadataAsync(content.Id, new ContentMetadataUpdateRequest { Content = new DataDictionary { { "name", updatedName } } }, waitSearchDocCreation: false)
                ;

            // Delete
            await _client.Content.DeleteAsync(content.Id);

            // Assert
            await Assert.ThrowsAsync<ContentNotFoundException>(async () => await _client.Content.GetAsync(content.Id));

            // Restore
            await _client.Content.RestoreAsync(content.Id, waitSearchDocCreation: false);

            // Assert
            content = await _client.Content.GetAsync(content.Id, new[] { ContentResolveBehavior.Content });
            content.Should().NotBeNull();
            content.AsContentItem<ContentItem>().Content.Name.Should().Be(updatedName);
        }

        [Fact]
        public async Task ShouldCreateUpdateDeleteAndRestoreContentManyWithoutWaitingSearchDocs()
        {
            // Act
            // Create
            var content1CreateRequest = new ContentCreateRequest
            {
                RequestId = $"{Guid.NewGuid():N}",
                Content = new { Name = $"{Guid.NewGuid():N}" },
                ContentSchemaId = nameof(ContentItem)
            };

            var content2CreateRequest = new ContentCreateRequest
            {
                RequestId = $"{Guid.NewGuid():N}",
                Content = new { Name = $"{Guid.NewGuid():N}" },
                ContentSchemaId = nameof(ContentItem)
            };

            var createResult = await _client.Content.CreateManyAsync(new ContentCreateManyRequest { Items = new[] { content1CreateRequest, content2CreateRequest } }, waitSearchDocCreation: false)
                ;

            var createdContents = (await createResult.FetchDetail()).SucceededItems.ToArray();

            var content1Id = createdContents.Single(c => c.RequestId == content1CreateRequest.RequestId).Item.Id;
            var content2Id = createdContents.Single(c => c.RequestId == content2CreateRequest.RequestId).Item.Id;
            var contentIds = new List<string> { content1Id, content2Id };

            // Update
            var expectedName1 = $"{Guid.NewGuid():N}";
            var expectedName2 = $"{Guid.NewGuid():N}";

            var updateItem1 = new ContentMetadataUpdateItem
            {
                Id = content1Id,
                Content = new DataDictionary { { "name", expectedName1 } }
            };

            var updateItem2 = new ContentMetadataUpdateItem
            {
                Id = content2Id,
                Content = new DataDictionary { { "name", expectedName2 } }
            };

            await _client.Content.UpdateMetadataManyAsync(new ContentMetadataUpdateManyRequest { Items = new[] { updateItem1, updateItem2 } }, waitSearchDocCreation: false);

            // Delete
            var businessProcess = await _client.Content.DeleteManyAsync(new ContentDeleteManyRequest { ContentIds = contentIds });
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id, waitForContinuationCompletion: false);

            // Assert
            await Assert.ThrowsAsync<ContentNotFoundException>(async () => await _client.Content.GetAsync(contentIds[0]));
            await Assert.ThrowsAsync<ContentNotFoundException>(async () => await _client.Content.GetAsync(contentIds[1]));

            // Reactivate
            businessProcess = await _client.Content.RestoreManyAsync(new ContentRestoreManyRequest { ContentIds = contentIds });
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id, waitForContinuationCompletion: false);

            var contents = await _client.Content.GetManyAsync(contentIds, new[] { ContentResolveBehavior.Content });

            // Assert
            contents.Should().HaveCount(2).And.Subject.Select(c => c.AsContentItem<ContentItem>().Content.Name).Should().BeEquivalentTo(expectedName1, expectedName2);
        }

        [Fact]
        public async Task ShouldBatchUpdateWithoutWithoutWaitingSearchDocs()
        {
            // Arrange
            var content1CreateRequest = new ContentCreateRequest
            {
                RequestId = $"{Guid.NewGuid():N}",
                Content = new { Name = $"{Guid.NewGuid():N}" },
                ContentSchemaId = nameof(ContentItem)
            };

            var content2CreateRequest = new ContentCreateRequest
            {
                RequestId = $"{Guid.NewGuid():N}",
                Content = new { Name = $"{Guid.NewGuid():N}" },
                ContentSchemaId = nameof(ContentItem)
            };

            var createResult = await _client.Content.CreateManyAsync(new ContentCreateManyRequest { Items = new[] { content1CreateRequest, content2CreateRequest } }, waitSearchDocCreation: false)
                ;

            var createdContents = (await createResult.FetchDetail()).SucceededItems.ToArray();

            var content1Id = createdContents.Single(c => c.RequestId == content1CreateRequest.RequestId).Item.Id;
            var content2Id = createdContents.Single(c => c.RequestId == content2CreateRequest.RequestId).Item.Id;
            var contentIds = new List<string> { content1Id, content2Id };

            // Act
            var temporaryName = $"{Guid.NewGuid():N}";
            var expectedName = $"{Guid.NewGuid():N}";

            var updateRequest = new ContentFieldsBatchUpdateRequest
            {
                ContentIds = contentIds,
                ChangeCommands = new List<MetadataValuesChangeCommandBase>
                {
                    new MetadataValuesSchemaUpsertCommand { SchemaId = nameof(ContentItem), Value = new DataDictionary { { "name", temporaryName } } }
                }
            };

            await _client.Content.BatchUpdateFieldsByIdsAsync(updateRequest, waitSearchDocCreation: false);

            updateRequest.ChangeCommands.First().As<MetadataValuesSchemaUpsertCommand>().Value = new DataDictionary { { "name", expectedName } };
            await _client.Content.BatchUpdateFieldsByIdsAsync(updateRequest, waitSearchDocCreation: false);

            var contents = await _client.Content.GetManyAsync(contentIds, new[] { ContentResolveBehavior.Content });

            // Assert
            contents.Should().HaveCount(2).And.Subject.Select(c => c.AsContentItem<ContentItem>().Content.Name).Should().OnlyContain(s => s == expectedName);
        }

        [Fact]
        public async Task ShouldDeleteContentManyByFilter()
        {
            // Arrange
            string uniqueValue = $"{Guid.NewGuid():N}";

            var content1 = await _client.Content.CreateAsync(new ContentCreateRequest
            {
                Content = JsonConvert.DeserializeObject($"{{ \"name\": \"{uniqueValue}_1\" }}"),
                ContentSchemaId = "ContentItem"
            });

            var content2 = await _client.Content.CreateAsync(new ContentCreateRequest
            {
                Content = JsonConvert.DeserializeObject($"{{ \"name\": \"{uniqueValue}_2\" }}"),
                ContentSchemaId = "ContentItem"
            });

            // Deactivate
            var deactivationRequest = new ContentDeleteManyFilterRequest
            {
                FilterRequest = new ContentFilterRequest
                {
                    ChannelId = "rootChannel",
                    SearchString = $"{uniqueValue}*"
                }
            };

            var businessProcess = await _client.Content.DeleteManyByFilterAsync(deactivationRequest);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            await Assert.ThrowsAsync<ContentNotFoundException>(async () => await _client.Content.GetAsync(content1.Id));
            await Assert.ThrowsAsync<ContentNotFoundException>(async () => await _client.Content.GetAsync(content2.Id));
        }

        [Fact]
        public async Task ShouldUpdateFile()
        {
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg -0030_JabLtzJl8bc", 20);

            var file = await _client.Ingest.UploadFileAsync(Path.Combine(_fixture.ExampleFilesBasePath, "0030_JabLtzJl8bc.jpg"));
            var updateRequest = new ContentFileUpdateRequest { IngestFile = file };

            var businessProcess = await _client.Content.UpdateFileAsync(contentId, updateRequest);
            var waitResult = await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);
        }

        [Fact]
        public async Task Should_replace_virtual_content_and_remove_layers_if_specified()
        {
            var namePrefix = $"{nameof(Should_replace_virtual_content_and_remove_layers_if_specified).Replace("_", string.Empty)}{Guid.NewGuid():N}";
            var contentSchemaId = namePrefix + "Content";
            var layerId = namePrefix + "Layer";

            var schemas = await _client.Schema.CreateManyAsync(new SchemaCreateManyRequest
            {
                Schemas = new List<SchemaCreateRequest>
                {
                    new SchemaCreateRequest
                    {
                        Id = contentSchemaId,
                        Names = new TranslatedStringDictionary { ["en"] = contentSchemaId },
                        Descriptions = new TranslatedStringDictionary { ["en"] = contentSchemaId },
                        Types = new[] { SchemaType.Content },
                        ViewForAll = true
                    },
                    new SchemaCreateRequest
                    {
                        Id = layerId,
                        Names = new TranslatedStringDictionary { ["en"] = layerId },
                        Descriptions = new TranslatedStringDictionary { ["en"] = layerId },
                        Types = new[] { SchemaType.Layer },
                        ReferencedInContentSchemaIds = new[]
                        {
                            contentSchemaId,
                            nameof(DocumentMetadata)
                        }
                    }
                }
            });

            var createdSchemas = await schemas.FetchDetail();
            createdSchemas.FailedItems.Should().BeEmpty();

            var content = await _client.Content.CreateAsync(new ContentCreateRequest
            {
                ContentSchemaId = contentSchemaId,
                LayerSchemaIds = new[] { layerId }
            });

            content.LayerSchemaIds.Should().ContainSingle().Which.Should().Be(layerId);

            var uploadedImage = await UploadFile("*.jpg");

            // check up-front if the replacement can be done or would cause layer removal
            var replacementCheck = await _client.Content.CheckUpdateFileAsync(
                content.Id,
                new ContentFileUpdateCheckRequest { IngestFile = uploadedImage });

            replacementCheck.Errors.Should().BeEmpty();
            var problematicChange = replacementCheck.ProblematicChanges.Should().ContainSingle().Which;
            problematicChange.IncompatibleLayerAssignments.Should().Contain(layerId, $"layer is not available for {nameof(ImageMetadata)}");

            // try to use a document instead
            var ex = await Record.ExceptionAsync(() => UploadAndReplaceContent(content.Id, "*.pdf"));
            var typeMismatchException = ex.Should().BeOfType<ContentFileReplaceTypeMismatchException>().Which;
            typeMismatchException.OriginalContentType.Should().Be(ContentType.Virtual);
            typeMismatchException.NewContentType.Should().Be(ContentType.InterchangeDocument);

            // we also need to opt-in to change ContentType
            await UploadAndReplaceContent(content.Id, "*.pdf", requestModifier: request => request.AllowContentTypeChange = true);
            content = await _client.Content.GetAsync(content.Id);
            content.ContentType.Should().Be(ContentType.InterchangeDocument);

            // we can opt-in to let the system remove the layer
            await ReplaceContentFile(
                content.Id,
                new ContentFileUpdateRequest
                {
                    IngestFile = uploadedImage,
                    AllowContentTypeChange = true,
                    AcceptableLayerUnassignments = problematicChange.IncompatibleLayerAssignments
                });
        }

        [Fact]
        public async Task ShouldUpdatePermissions()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            var contentDetail = await _client.Content.GetAsync(contentId);
            var permissionSetId = (await _fixture.ContentPermissions.Create()).Id;

            var contentPermissionSetIds = new List<string>
            {
                permissionSetId
            };

            var request = new ContentPermissionsUpdateRequest
            {
                ContentPermissionSetIds = contentPermissionSetIds
            };

            // Act
            var result = await _client.Content.UpdatePermissionsAsync(contentDetail.Id, request);

            var currentContentDetail = await _client.Content.GetAsync(contentId);
            var currentContentPermissionSetIds = currentContentDetail.ContentPermissionSetIds.Select(i => i).ToList();

            // Assert
            Assert.True(!contentPermissionSetIds.Except(currentContentPermissionSetIds).Any());
            Assert.True(!currentContentPermissionSetIds.Except(contentPermissionSetIds).Any());

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        public async Task ShouldUpdatePermissionsMany()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 20);
            var contentDetail = await _client.Content.GetAsync(contentId);
            var permissionSetId = (await _fixture.ContentPermissions.Create()).Id;

            var contentPermissionSetIds = new List<string>
            {
                permissionSetId
            };

            var manyRequest = new ContentPermissionsUpdateManyRequest
            {
                Items = new List<ContentPermissionsUpdateItem>
                {
                    new ContentPermissionsUpdateItem
                    {
                        ContentId = contentDetail.Id,
                        ContentPermissionSetIds = contentPermissionSetIds
                    }
                }
            };

            // Act
            await _client.Content.UpdatePermissionsManyAsync(manyRequest);

            var currentContentDetail = await _client.Content.GetAsync(contentId);
            var currentContentPermissionSetIds = currentContentDetail.ContentPermissionSetIds.Select(i => i).ToList();

            // Assert
            currentContentPermissionSetIds.Should().BeEquivalentTo(contentPermissionSetIds);
        }

        [Fact]
        public async Task ShouldUseDisplayLanguageToResolveDisplayPatterns()
        {
            // Arrange
            var schemaId = $"DisplayLanguageContentSchema{Guid.NewGuid():N}";
            var contentSchema = new SchemaDetail
            {
                Id = schemaId,
                Names = new TranslatedStringDictionary { { "en", "Display language content schema" } },
                Types = new List<SchemaType> { SchemaType.Content },
                Fields = new List<FieldBase>
                {
                    new FieldString { Id = "value1", Names = new TranslatedStringDictionary { { "en", "Value 1" } } },
                    new FieldString { Id = "value2", Names = new TranslatedStringDictionary { { "en", "Value 2" } } }
                },
                ViewForAll = true,
                DisplayPatterns = new List<DisplayPattern>
                {
                    new DisplayPattern
                    {
                        DisplayPatternType = DisplayPatternType.Name,
                        TemplateEngine = TemplateEngine.DotLiquid,
                        Templates = new TranslatedStringDictionary
                        {
                            { "en", $"{{{{data.{schemaId.ToLowerCamelCase()}.value1}}}}" },
                            { "de", $"{{{{data.{schemaId.ToLowerCamelCase()}.value2}}}}" }
                        }
                    }
                }
            };

            await _client.Schema.CreateAsync(contentSchema, TimeSpan.FromMinutes(1));

            var content = new ContentCreateRequest
            {
                ContentSchemaId = schemaId,
                Content = new
                {
                    value1 = "value1",
                    value2 = "value2"
                }
            };

            var detail = await _client.Content.CreateAsync(content);

            // Act
            var englishClient = _fixture.GetLocalizedPictureparkService("en");
            var englishContent = await englishClient.Content.GetAsync(detail.Id, new[] { ContentResolveBehavior.Content, ContentResolveBehavior.OuterDisplayValueName });

            var germanClient = _fixture.GetLocalizedPictureparkService("de");
            var germanContent = await germanClient.Content.GetAsync(detail.Id, new[] { ContentResolveBehavior.Content, ContentResolveBehavior.OuterDisplayValueName });

            // Assert
            englishContent.DisplayValues[DisplayPatternType.Name.ToString().ToLowerCamelCase()].Should().Be("value1");
            germanContent.DisplayValues[DisplayPatternType.Name.ToString().ToLowerCamelCase()].Should().Be("value2");
        }

        [Fact]
        public async Task ShouldFetchResultFromCreateMany()
        {
            // Arrange
            var requests = Enumerable.Range(0, 201).Select(
                x => new ContentCreateRequest
                {
                    Content = new
                    {
                        name = $"Content #{x}"
                    },
                    ContentSchemaId = "ContentItem"
                }).ToList();

            var result = await _client.Content.CreateManyAsync(
                new ContentCreateManyRequest
                {
                    Items = requests
                });

            // Act
            var detail = await result.FetchDetail(new[] { ContentResolveBehavior.Content });

            // Assert
            detail.SucceededItems.Should().HaveCount(201);
            detail.SucceededItems.Select(i => ((dynamic)i.Item.Content).name).ToArray().Distinct().Should().HaveCount(201);
        }

        [Fact]
        public async Task ShouldHandleDuplicateFilenameWhenDownloading()
        {
            // Arrange
            const int numberOfUploads = 2;

            for (var i = 0; i < numberOfUploads; i++)
            {
                await _client.Ingest.UploadAndImportFilesAsync(
                    new[]
                    {
                        Path.Combine(_fixture.ExampleFilesBasePath, "0559_BYu8ITUWMfc.jpg"),
                    });
            }

            var contents = await _client.Content.SearchAsync(new ContentSearchRequest { SearchString = "fileMetadata.fileName:0559_BYu8ITUWMfc.jpg" });

            // Act
            var targetFolder = Path.Combine(_fixture.TempDirectory, nameof(ShouldHandleDuplicateFilenameWhenDownloading) + Guid.NewGuid().ToString("N"));
            await _client.Content.DownloadFilesAsync(contents, targetFolder, overwriteIfExists: false);

            // Assert
            new DirectoryInfo(targetFolder).EnumerateFiles("*").Should().HaveCountGreaterOrEqualTo(numberOfUploads);
        }

        [Fact]
        public async Task ShouldListHistoricVersions()
        {
            var versioningState = (await _client.Info.GetInfoAsync()).LicenseInformation
                .HistoricVersioningState;
            var contentId = (await UploadAndImportContents()).Single();
            await UploadAndReplaceContent(contentId);

            var versions = await _client.Content.GetVersionsAsync(contentId, new HistoricVersionSearchRequest());

            if (versioningState == HistoricVersioningState.Enabled)
            {
                // If historic versioning is enabled on the customer, the original version is preserved when the content is replaced.
                var version = versions.Results.Should().ContainSingle().Which;
                version.Replaced.Should().BeWithin(TimeSpan.FromMinutes(2));
                version.ContentId.Should().Be(contentId);
                version.VersionNumber.Should().Be(1);
                version.CreatedByXmpWriteback.Should().BeFalse();
            }
            else
            {
                // If historic versioning is disabled or suspended, the original version is not preserved when the content is replaced.
                versions.Results.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task ShouldGetDownloadLinkForHistoricVersions()
        {
            var versioningState = (await _client.Info.GetInfoAsync()).LicenseInformation
                .HistoricVersioningState;
            var contentId = (await UploadAndImportContents()).Single();
            await UploadAndReplaceContent(contentId);

            if (versioningState == HistoricVersioningState.Enabled)
            {
                // If historic versioning is enabled on the customer, the original version is preserved when the content is replaced.
                // Versions are numbered in sequence from 1.
                var downloadLink = await _client.Content.GetVersionDownloadLinkAsync(contentId, 1);

                using (var httpClient = new HttpClient())
                using (var response = await httpClient.GetAsync(downloadLink.DownloadUrl))
                {
                    response.EnsureSuccessStatusCode();

                    var fileName = response.Content.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                    Assert.EndsWith(".jpg", fileName);

                    var filePath = Path.Combine(_fixture.TempDirectory, fileName);

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(filePath))
                    {
                        await stream.CopyToAsync(fileStream);

                        // Assert
                        Assert.True(stream.Length > 10);
                    }
                }
            }
            else
            {
                // If historic versioning is disabled, the original version is not preserved when the content is replaced,
                // therefore download link cannot be generated.
                Func<Task> createVersion = () => _client.Content.GetVersionDownloadLinkAsync(contentId, 1);
                await createVersion.Should().ThrowAsync<ContentHistoricVersionNotFoundException>();
            }
        }

        [Fact]
        public async Task ShouldDeleteHistoricVersion()
        {
            var versioningState = (await _client.Info.GetInfoAsync()).LicenseInformation
                .HistoricVersioningState;
            var contentId = (await UploadAndImportContents()).Single();
            await UploadAndReplaceContent(contentId);

            if (versioningState == HistoricVersioningState.Enabled)
            {
                // If historic versioning is enabled on the customer, the original version is preserved when the content is replaced.
                var versions = await _client.Content.GetVersionsAsync(contentId, new HistoricVersionSearchRequest());
                versions.Results.Should().HaveCount(1);

                // Versions are numbered in sequence from 1.
                await _client.Content.DeleteVersionAsync(contentId, 1);

                versions = await _client.Content.GetVersionsAsync(contentId, new HistoricVersionSearchRequest());
                versions.Results.Should().BeEmpty();
            }
            else
            {
                // If historic versioning is disabled, the original version is not preserved when the content is replaced.
                Func<Task> deleteVersion = () => _client.Content.DeleteVersionAsync(contentId, 1);
                await deleteVersion.Should().ThrowAsync<ContentHistoricVersionNotFoundException>();
            }
        }

        [Fact]
        public async Task ShouldResolveHistoricVersionCount()
        {
            var versioningState = (await _client.Info.GetInfoAsync()).LicenseInformation
                .HistoricVersioningState;
            var contentId = (await UploadAndImportContents()).Single();
            await UploadAndReplaceContent(contentId);

            var content = await _client.Content.GetAsync(contentId, new[] { ContentResolveBehavior.HistoricVersionCount });

            if (versioningState == HistoricVersioningState.Enabled)
            {
                // If historic versioning is enabled on the customer, the original version is preserved when the content is replaced.
                content.HistoricVersionCount.Should().Be(1);
            }
            else
            {
                // If historic versioning is disabled or suspended, the original version is not preserved when the content is replaced.
                content.HistoricVersionCount.Should().BeNull();
            }
        }

        [Fact]
        public async Task ShouldRespectProtectionFlagOnSchema()
        {
            var contentSchema = await _client.Schema.CreateAsync(
                new SchemaCreateRequest
                {
                    Id = $"ContentSchema{Guid.NewGuid():N}",
                    Types = new List<SchemaType> { SchemaType.Content },
                    ViewForAll = true,
                    Names = new TranslatedStringDictionary
                    {
                        ["en"] = "Content schema"
                    },
                    MetadataProtection = new MetadataProtection
                    {
                        PreventCreate = false,
                        PreventUpdate = false,
                        PreventDelete = true
                    }
                });

            var content = await _client.Content.CreateAsync(
                new ContentCreateRequest
                {
                    ContentSchemaId = contentSchema.Schema.Id,
                    Content = new object()
                });

            var ex = await Record.ExceptionAsync(() => _client.Content.DeleteAsync(content.Id));
            ex.Should().BeOfType<SchemasMetadataProtectionException>();
        }

        [Fact]
        public async Task ShouldResolveFilterTemplatesAndReturnHasItems()
        {
            // Arrange
            await SchemaHelper.CreateSchemasIfNotExistentAsync<ContentWithDynamicView>(_client);

            // Act
            var contentForViewField = new ContentWithDynamicView { FilterValue = $"{Guid.NewGuid():N}" };

            var content = await _client.Content.CreateAsync(
                new ContentCreateRequest
                {
                    ContentSchemaId = Metadata.ResolveSchemaId(contentForViewField),
                    Content = contentForViewField
                },
                new[]
                {
                    ContentResolveBehavior.Content,
                    ContentResolveBehavior.DynamicViewFields,
                    ContentResolveBehavior.DynamicViewFieldsWithHasItems
                },
                waitSearchDocCreation: true);

            // Assert
            var viewField = content.AsContentItem<ContentWithDynamicView>().Content.ViewField;
            viewField.Meta.Should().NotBeNull();

            var withHasItems = viewField.Meta.Should().BeOfType<DynamicViewFieldMetaWithHasItems>().Which;
            withHasItems.Filter.Should().BeOfType<TermsFilter>().Which.Terms.Should().Contain(contentForViewField.FilterValue);
            withHasItems.HasItems.Should().BeTrue();

            var searchResultForFilter =
                await _client.Content.SearchAsync(new ContentSearchRequest { Filter = withHasItems.Filter, Limit = 1 });
            searchResultForFilter.TotalResults.Should().Be(1);
            searchResultForFilter.Results.Should().Contain(c => c.Id == content.Id);
        }

        [Fact]
        public async Task ShouldSetAndUnsetDisplayContent()
        {
            // Arrange
            var randomContents = await _fixture.GetRandomContentsAsync(".jpg", 2);
            var contentIds = randomContents.Results.Select(i => i.Id).ToList();

            var contentId = contentIds[0];
            var displayContentId = contentIds[1];

            // Act - set the display content
            var setDisplayContentRequest = new SetDisplayContentRequest
            {
                DisplayContentId = displayContentId
            };

            var result = await _client.Content.SetDisplayContentAsync(
                contentId,
                setDisplayContentRequest,
                new[]
                {
                    ContentResolveBehavior.Content,
                    ContentResolveBehavior.DisplayContentOutputs,
                });

            // Assert
            result.DisplayContentId.Should().Be(displayContentId);
            result.DisplayContentOutputs.Should().NotBeNull();
            result.DisplayContentOutputs.Should().NotBeEmpty();

            // unset display content by setting the 'DisplayContentId' property to null
            setDisplayContentRequest = new SetDisplayContentRequest
            {
                DisplayContentId = null
            };

            result = await _client.Content.SetDisplayContentAsync(
                contentId,
                setDisplayContentRequest,
                new[]
                {
                    ContentResolveBehavior.Content,
                    ContentResolveBehavior.DisplayContentOutputs,
                });

            // Assert
            result.DisplayContentId.Should().Be(null);
            result.DisplayContentOutputs.Should().BeEmpty();
        }

        [PictureparkSchema(SchemaType.Content)]
        public class ContentWithDynamicView
        {
            [PictureparkDynamicView(typeof(DynamicViewFilterProvider))]
            [PictureparkDynamicViewUiSettings(ItemFieldViewMode.List, showRelatedContentOnDownload: false)]
            public DynamicViewObject ViewField { get; set; }

            [PictureparkSearch(Index = true)]
            public string FilterValue { get; set; }

            internal class DynamicViewFilterProvider : IFilterProvider
            {
                public static readonly string[] FilterTerms = { $"{{{{data.{nameof(ContentWithDynamicView).ToLowerCamelCase()}.{Field}}}}}", "hardcodedValue" };

                public static string Field => "filterValue";

                public FilterBase GetFilter() => new TermsFilter
                {
                    Field = $"{nameof(ContentWithDynamicView).ToLowerCamelCase()}.{Field}",
                    Terms = FilterTerms
                };
            }
        }

        private static async Task AssertFileResponseOkAndNonEmpty(FileResponse response, string expectedContentType = null)
        {
            using (var stream = new MemoryStream())
            {
                response.StatusCode.Should().Be((int)HttpStatusCode.OK);

                if (expectedContentType != null)
                {
                    var contentType = response.Headers["Content-Type"].Single();
                    contentType.Should().Be(expectedContentType);
                }

                await response.Stream.CopyToAsync(stream);
                stream.Length.Should().BeGreaterOrEqualTo(10);
            }
        }

        private async Task<ContentDetail> CreateContentItem()
        {
            // Arrange
            var contentSchema = await SchemaHelper.CreateSchemasIfNotExistentAsync<ContentItem>(_client);

            var content = await _client.Content.CreateAsync(new ContentCreateRequest
            {
                ContentSchemaId = contentSchema.Id,
                Content = new ContentItem
                {
                    Name = "Jozef"
                }
            });

            // Act
            var contentDetail = await _client.Content
                .GetAsync(content.Id)
                ;

            contentDetail.Id.Should().Be(content.Id);
            contentDetail.Should().NotBeNull();
            contentDetail.Content.Should().NotBeNull();

            return contentDetail;
        }

        private async Task<ContentDetail> CreateContentReferencingSimpleField(params ContentResolveBehavior[] behaviors)
        {
            // Arrange
            var contentSchema = await SchemaHelper.CreateSchemasIfNotExistentAsync<ContentItemWithTagBox>(_client);

            var listItemCreate = new ListItemCreateRequest
            {
                ContentSchemaId = nameof(SimpleReferenceObject),
                Content = new SimpleReferenceObject
                {
                    NameField = "simpleField"
                }
            };

            var listItem = await _client.ListItem.CreateAsync(listItemCreate);

            var content = await _client.Content.CreateAsync(new ContentCreateRequest
            {
                ContentSchemaId = contentSchema.Id,
                Content = new ContentItemWithTagBox
                {
                    Name = "Jozef",
                    Object = new SimpleReferenceObject
                    {
                        RefId = listItem.Id
                    }
                }
            });

            // Act
            var contentDetail = await _client.Content
                .GetAsync(content.Id, behaviors)
                ;

            contentDetail.Id.Should().Be(content.Id);
            contentDetail.Should().NotBeNull();
            contentDetail.Content.Should().NotBeNull();

            return contentDetail;
        }

        private async Task<(SchemaDetail, SchemaDetail)> CreateSchemasForTriggerTests()
        {
            var contentSchemaId = $"TestContent{Guid.NewGuid():N}";
            var layerSchemaId = $"TestLayer{Guid.NewGuid():N}";
            var schemaCreateManyRequest = new SchemaCreateManyRequest
            {
                Schemas = new[]
                {
                    new SchemaCreateRequest { Id = contentSchemaId, Names = new TranslatedStringDictionary { { "en", "Test content" } }, Types = new[] { SchemaType.Content }, ViewForAll = true, LayerSchemaIds = new HashSet<string> { layerSchemaId } },
                    new SchemaCreateRequest
                    {
                        Id = layerSchemaId,
                        Names = new TranslatedStringDictionary { { "en", "Test layer" } },
                        Types = new[] { SchemaType.Layer },
                        ViewForAll = true,
                        ReferencedInContentSchemaIds = new HashSet<string> { contentSchemaId },
                        Fields = new[]
                        {
                            new FieldTrigger
                            {
                                Id = "fieldTrigger",
                                Names = new TranslatedStringDictionary { { "en", "Field Trigger" } },
                                Descriptions = new TranslatedStringDictionary { { "en", "Field Trigger Description" } },
                                Index = true,
                                SimpleSearch = true
                            }
                        }
                    }
                }
            };
            var result = await _client.Schema.CreateManyAsync(schemaCreateManyRequest);
            var resultDetails = await result.FetchDetail();
            var createdSchemas = resultDetails.SucceededItems;

            return (createdSchemas.First(s => s.Types.Contains(SchemaType.Content)), createdSchemas.First(s => s.Types.Contains(SchemaType.Layer)));
        }

        private IEnumerable<string> GetFilePaths(string searchPattern = "*.jpg", int count = 1)
        {
            var filesInDirectory = Directory.GetFiles(_fixture.ExampleFilesBasePath, searchPattern).ToList();

            var numberOfFilesInDirectory = filesInDirectory.Count;
            var numberOfUploadFiles = Math.Min(count, numberOfFilesInDirectory);

            var randomNumber = Random.Shared.Next(0, numberOfFilesInDirectory - numberOfUploadFiles);
            return filesInDirectory
                .Skip(randomNumber)
                .Take(numberOfUploadFiles);
        }

        private async Task<IReadOnlyList<string>> UploadAndImportContents(int count = 1, string searchPattern = "*.jpg")
        {
            var importResult = await _client.Ingest.UploadAndImportFilesAsync(GetFilePaths(searchPattern, count));
            var details = await importResult.FetchDetail();

            return details.SucceededIds;
        }

        private async Task UploadAndReplaceContent(string contentId, string searchPattern = "*.jpg", Action<ContentFileUpdateRequest> requestModifier = null)
        {
            var file = await UploadFile(searchPattern);

            var contentFileUpdateRequest = new ContentFileUpdateRequest { IngestFile = file };
            requestModifier?.Invoke(contentFileUpdateRequest);

            await ReplaceContentFile(contentId, contentFileUpdateRequest);
        }

        private async Task ReplaceContentFile(string contentId, ContentFileUpdateRequest updateRequest)
        {
            var businessProcess = await _client.Content.UpdateFileAsync(contentId, updateRequest);
            var waitResult = await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);
        }

        private async Task<IngestFile> UploadFile(string searchPattern = ".jpg")
        {
            var filePath = GetFilePaths(searchPattern, count: 1).Single();
            return await _client.Ingest.UploadFileAsync(filePath);
        }
    }
}
