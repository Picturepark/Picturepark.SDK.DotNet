using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentAssertions;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class ShareTests : IClassFixture<ShareFixture>
    {
        private readonly ShareFixture _fixture;
        private readonly IPictureparkService _client;

        public ShareTests(ShareFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldAggregate()
        {
            // Arrange
            await CreateShareAndReturnId(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            }).ConfigureAwait(false);

            // Act
            var request = new ShareAggregationRequest
            {
                SearchString = string.Empty,
                Aggregators = new List<AggregatorBase>
                {
                    new TermsAggregator
                    {
                        Field = nameof(Share.ShareType).ToLowerCamelCase(),
                        Size = 10,
                        Name = "ShareType"
                    }
                }
            };

            var result = await _client.Share.AggregateAsync(request).ConfigureAwait(false);

            // Assert
            var aggregation = result.GetByName("ShareType");
            aggregation.Should().NotBeNull();
            aggregation.AggregationResultItems.Count.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldSearchAndAggregateAllTogether()
        {
            // Arrange
            var name = "Embed share to search and aggregate" + new Random().Next(0, 999999);
            var description1 = "Description of Embed share 1";
            var description2 = "Description of Embed share 2";

            var shareId1 = await CreateShareAndReturnId(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = description1,
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = name
            }).ConfigureAwait(false);
            var shareId2 = await CreateShareAndReturnId(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = description2,
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = name
            }).ConfigureAwait(false);

            // Act
            var request = new ShareSearchRequest
            {
                Filter = new AndFilter
                {
                    Filters = new List<FilterBase>
                    {
                        FilterBase.FromExpression<Share>(i => i.ShareType, ShareType.Embed.ToString()),
                        FilterBase.FromExpression<Share>(i => i.Name, name)
                    }
                },
                Aggregators = new List<AggregatorBase>
                {
                    new TermsAggregator
                    {
                        Field = nameof(Share.ShareType).ToLowerCamelCase(),
                        Size = 10,
                        Name = "ShareType"
                    },
                    new TermsAggregator
                    {
                        Field = "description",
                        Size = 10,
                        Name = "descriptionAggregation"
                    }
                }
            };

            var result = await _client.Share.SearchAsync(request).ConfigureAwait(false);

            // Assert
            result.Results.Should().HaveCount(2).And.Subject.Select(r => r.Id).Should().BeEquivalentTo(shareId1, shareId2);

            var shareTypeAggregation = result.AggregationResults.FirstOrDefault(ar => ar.Name == "ShareType");
            shareTypeAggregation.Should().NotBeNull();
            shareTypeAggregation.AggregationResultItems.Should().HaveCount(1).And.Subject.First(ar => ar.Name == ShareType.Embed.ToString()).Count.Should().Be(2);

            var descriptionAggregation = result.AggregationResults.FirstOrDefault(ar => ar.Name == "descriptionAggregation");
            descriptionAggregation.Should().NotBeNull();
            descriptionAggregation.AggregationResultItems.Should().HaveCount(2);
            descriptionAggregation.AggregationResultItems.Where(ar => ar.Name == description1).Should().HaveCount(1).And.Subject.First().Count.Should().Be(1);
            descriptionAggregation.AggregationResultItems.Where(ar => ar.Name == description2).Should().HaveCount(1).And.Subject.First().Count.Should().Be(1);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldUpdateEmbed()
        {
            // Arrange
            var shareId = await CreateShareAndReturnId(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            }).ConfigureAwait(false);

            var share = await _client.Share.GetAsync(shareId).ConfigureAwait(false);

            // Act
            var request = share.AsEmbedUpdateRequest(r => r.Description = "Foo");

            var updateBp = await _client.Share.UpdateAsync(shareId, request).ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(updateBp.Id).ConfigureAwait(false);

            // Assert
            var updatedShare = await _client.Share.GetAsync(shareId).ConfigureAwait(false);
            updatedShare.Description.Should().Be("Foo");
            updatedShare.Schemas.Should().BeNull();
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldDeleteMany()
        {
            // Arrange
            var shareId = await CreateShareAndReturnId(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            }).ConfigureAwait(false);

            var share = await _client.Share.GetAsync(shareId).ConfigureAwait(false);
            shareId.Should().Be(share.Id);

            // Act
            var deleteManyRequest = new ShareDeleteManyRequest
            {
                Ids = new List<string> { shareId }
            };

            var deleteBusinessProcess = await _client.Share.DeleteManyAsync(deleteManyRequest).ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(deleteBusinessProcess.Id).ConfigureAwait(false);

            var summary = await _client.BusinessProcess.GetSummaryAsync(deleteBusinessProcess.Id).ConfigureAwait(false) as BusinessProcessSummaryBatchBased;
            summary.Should().NotBeNull();
            summary.SucceededItemCount.Should().Be(1);

            await Assert.ThrowsAsync<ShareNotFoundException>(async () =>
            {
                await _client.Share.GetAsync(shareId).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldCreateBasicShare()
        {
            // Arrange
            // Act
            var createResult = await CreateShareAndReturnId(new ShareBasicCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Basic share aaa",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Basic share aaa",
                RecipientEmails = new List<UserEmail>
                {
                    _fixture.Configuration.EmailRecipient
                },
                LanguageCode = "en"
            }).ConfigureAwait(false);

            // Assert
            createResult.Should().NotBeNull();
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldCreateBasicShareWithWrongContentsAndFail()
        {
            // Arrange
            var outputFormatIds = new List<string> { "Original" };

            var shareContentItems = new List<ShareContentBase>
            {
                new ShareContent { ContentId = "NonExistingId1", OutputFormatIds = outputFormatIds },
                new ShareContent { ContentId = "NonExistingId2", OutputFormatIds = outputFormatIds }
            };

            var request = new ShareBasicCreateRequest
            {
                Contents = shareContentItems,
                Description = "Description of share with wrong content ids",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Share with wrong content ids",
                RecipientEmails = new List<UserEmail>
                {
                    _fixture.Configuration.EmailRecipient
                },
                LanguageCode = "en"
            };

            // Act
            var createBusinessProcess = await _client.Share.CreateAsync(request).ConfigureAwait(false);

            // Assert
            await Assert.ThrowsAsync<ContentNotFoundException>(
                async () => await _client.BusinessProcess.WaitForCompletionAsync(createBusinessProcess.Id).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldCreateEmbedShare()
        {
            // Arrange
            // Act
            var shareId = await CreateShareAndReturnId(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = "Embed share bbb"
            }).ConfigureAwait(false);

            // Assert
            var share = await _client.Share.GetAsync(shareId).ConfigureAwait(false);
            share.Id.Should().Be(shareId);
            share.Schemas.Should().BeNull();
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldFailOnSharingDuplicatedContentOutputs()
        {
            // Arrange
            var contents = await GetRandomShareContent(1).ConfigureAwait(false);

            // Share content twice
            contents.Add(contents.First());

            await Assert.ThrowsAsync<DuplicateSharedOutputException>(async () =>
            {
                await CreateShareAndReturnId(new ShareEmbedCreateRequest
                {
                    Contents = contents,
                    Description = "Description of Embed share bbb",
                    ExpirationDate = new DateTime(2020, 12, 31),
                    Name = "Embed share bbb"
                }).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldSearch()
        {
            // Arrange
            var name = "Embed share to search" + new Random().Next(0, 999999);
            await CreateShareAndReturnId(new ShareEmbedCreateRequest
            {
                Contents = await GetRandomShareContent().ConfigureAwait(false),
                Description = "Description of Embed share bbb",
                ExpirationDate = new DateTime(2020, 12, 31),
                Name = name
            }).ConfigureAwait(false);

            // Act
            var request = new ShareSearchRequest
            {
                Limit = 100,
                Filter = new AndFilter
                {
                    Filters = new List<FilterBase>
                    {
                        FilterBase.FromExpression<Share>(i => i.ShareType, ShareType.Embed.ToString()),
                        FilterBase.FromExpression<Share>(i => i.Name, name)
                    }
                }
            };

            // Act
            var result = await _client.Share.SearchAsync(request).ConfigureAwait(false);

            // Assert
            result.TotalResults.Should().Be(1);
            result.Results.First().Name.Should().Be(name);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldGetShareByToken()
        {
            // Arrange
            var outputFormatIds = new List<string> { "Original", "Preview" };
            var contentIds = new List<string>();

            while (contentIds.Count < 2)
            {
                var contentId = await _fixture.GetRandomContentIdAsync(string.Empty, 30).ConfigureAwait(false);

                if (!contentIds.Contains(contentId))
                    contentIds.Add(contentId);
            }

            var shareContentItems = contentIds.Select(x => new ShareContent() { ContentId = x, OutputFormatIds = outputFormatIds }).ToList<ShareContentBase>();

            var request = new ShareEmbedCreateRequest
            {
                Contents = shareContentItems,
                Description = "Description of Embed share",
                ExpirationDate = DateTime.Now + TimeSpan.FromDays(7),
                Name = "Embed share"
            };

            var contents = await _client.Content.GetManyAsync(contentIds).ConfigureAwait(false);
            var contentSchemaIds = contents.Select(s => s.ContentSchemaId).Distinct().ToList();

            var shareId = await CreateShareAndReturnId(request).ConfigureAwait(false);
            var embedDetail = await _client.Share.GetAsync(shareId, new List<ShareResolveBehavior> { ShareResolveBehavior.Schemas }).ConfigureAwait(false);

            embedDetail.Should().NotBeNull();
            embedDetail.Id.Should().Be(shareId);
            embedDetail.Schemas.Should().NotBeEmpty();
            embedDetail.Schemas.Should().NotBeEmpty().And.Subject.Should().Contain(s => contentSchemaIds.Contains(s.Id));

            // Act
            var result = await _client.Share.GetShareJsonAsync(((ShareDataEmbed)embedDetail.Data).Token, resolveBehaviors: new List<ShareResolveBehavior> { ShareResolveBehavior.Schemas }).ConfigureAwait(false);

            result.Should().NotBeNull();
            result.Id.Should().Be(shareId);
            result.Schemas.Should().NotBeEmpty().And.Subject.Should().Contain(s => contentSchemaIds.Contains(s.Id));
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldGetShareOutputByToken()
        {
            // Arrange
            var contents = await GetRandomShareContent(".jpg").ConfigureAwait(false);
            var shareId = await CreateShareAndReturnId(new ShareEmbedCreateRequest
            {
                Contents = contents,
                Description = "Description of Embed share",
                ExpirationDate = DateTime.Now + TimeSpan.FromDays(7),
                Name = "Embed share"
            }).ConfigureAwait(false);

            var embedDetail = await _client.Share.GetAsync(shareId).ConfigureAwait(false);

            var shareOutput = (ShareOutputEmbed)embedDetail.ContentSelections.First().Outputs.First();

            // Act
            using (var result = await _client.Share.DownloadAsync(shareOutput.Token).ConfigureAwait(false))
            {
                // Assert
                Assert.NotNull(result);
            }
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldGetShareOutputByContentIdAndOutputFormatId()
        {
            var contents = await GetRandomShareContent(".jpg").ConfigureAwait(false);
            var shareId = await CreateShareAndReturnId(new ShareEmbedCreateRequest
            {
                Contents = contents,
                Description = "Description of Embed share",
                ExpirationDate = DateTime.Now + TimeSpan.FromDays(7),
                Name = "Embed share"
            }).ConfigureAwait(false);

            var embedDetail = await _client.Share.GetAsync(shareId).ConfigureAwait(false);

            // Act
            using (var result = await _client.Share
                .DownloadWithOutputFormatIdAsync(((ShareDataEmbed)embedDetail.Data).Token, contents.First().ContentId, "Original")
                .ConfigureAwait(false))
            {
                // Assert
                Assert.NotNull(result);
            }
        }

        [Theory]
        [Trait("Stack", "Shares")]
        [InlineData("", 400, 400)]
        [InlineData("resize-to:200x150", 200, 150)]
        public async Task ShouldUsePresetToEditOutput(string preset, int expectedWidth, int expectedHeight)
        {
            var randomContents = await _fixture.GetRandomContentsAsync(".jpg", 1).ConfigureAwait(false);
            var contentId = randomContents.Results.Single().Id;

            var shareId = await CreateShareAndReturnId(
                new ShareEmbedCreateRequest
                {
                    Contents = new List<ShareContentBase>
                    {
                        new EmbedContent
                        {
                            ContentId = contentId,
                            OutputFormatIds = new[] { "Preview" },
                            ConversionPresets = new List<ConversionPreset>
                            {
                                new ConversionPreset
                                {
                                    OutputFormatId = "Preview",
                                    Conversion = "resize-to:400x400",
                                    Locked = string.IsNullOrEmpty(preset)
                                }
                            }
                        }
                    },
                    OutputAccess = OutputAccess.None,
                    Description = "Description of Embed share",
                    ExpirationDate = DateTime.Now + TimeSpan.FromDays(7),
                    Name = "Embed share with conversion"
                }).ConfigureAwait(false);

            var embedDetail = await _client.Share.GetAsync(shareId).ConfigureAwait(false);
            var token = embedDetail.ContentSelections.Single().Outputs.OfType<ShareOutputEmbed>().Single(o => !o.DynamicRendering).Token;

            // Act
            using (var result = await _client.Share
                .DownloadWithConversionPresetAsync(token, preset)
                .ConfigureAwait(false))
            {
                var tempFile = Path.GetTempFileName();
                await result.Stream.WriteToFileAsync(tempFile).ConfigureAwait(false);

                var bitmap = new Bitmap(tempFile);
                bitmap.Width.Should().Be(expectedWidth);
                bitmap.Height.Should().Be(expectedHeight);
            }
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldGetShareOutputByContentIdAndSize()
        {
            // Arrange transfer
            var timeout = TimeSpan.FromMinutes(2);
            var transferName = nameof(ShouldGetShareOutputByContentIdAndSize) + "-" + new Random().Next(1000, 9999);

            var filesInDirectory = Directory.GetFiles(_fixture.ExampleFilesBasePath, "0033_aeVA-j1y2BY.jpg").ToList();

            var importFilePaths = filesInDirectory.Select(fn => new FileLocations(fn, $"{Path.GetFileNameWithoutExtension(fn)}_1{Path.GetExtension(fn)}")).ToList();

            // Act
            var uploadOptions = new UploadOptions
            {
                SuccessDelegate = Console.WriteLine,
                ErrorDelegate = args => Console.WriteLine(args.Exception)
            };
            var createTransferResult = await _client.Transfer.UploadFilesAsync(transferName, importFilePaths, uploadOptions).ConfigureAwait(false);

            var importRequest = new ImportTransferRequest
            {
                ContentPermissionSetIds = new List<string>(),
                Metadata = null,
                LayerSchemaIds = new List<string>()
            };

            await _client.Transfer.ImportAndWaitForCompletionAsync(createTransferResult.Transfer, importRequest, timeout).ConfigureAwait(false);

            // Assert
            var transferResult = await _client.Transfer.SearchFilesByTransferIdAsync(createTransferResult.Transfer.Id).ConfigureAwait(false);
            var contentIds = transferResult.Select(r => r.ContentId).ToList();

            Assert.Equal(importFilePaths.Count, contentIds.Count);

            // Arrange get share
            var contentId = contentIds.First();
            var outputFormatIds = new List<string> { "Original", "Preview" };
            var shareContentItems = new List<ShareContentBase>
            {
                new ShareContent { ContentId = contentId, OutputFormatIds = outputFormatIds },
            };

            var request = new ShareEmbedCreateRequest
            {
                Contents = shareContentItems,
                Description = "Description of Embed share",
                ExpirationDate = DateTime.Now + TimeSpan.FromDays(7),
                Name = "Embed share"
            };

            var shareId = await CreateShareAndReturnId(request).ConfigureAwait(false);
            var embedDetail = await _client.Share.GetAsync(shareId).ConfigureAwait(false);

            var shareOutput = (ShareOutputEmbed)embedDetail.ContentSelections.Single().Outputs.First();

            // Act
            using (var result = await _client.Share.DownloadAsync(shareOutput.Token, 10, 10).ConfigureAwait(false))
            {
                // Assert
                Assert.NotNull(result);
            }
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldContainDynamicOutputsBasic()
        {
            var (formatIdOriginal, formatIdPreview, shareContents) = await PrepareDynamicOutputFormatTest();

            var fullId = await CreateShareAndReturnId(new ShareBasicCreateRequest()
            {
                Contents = shareContents,
                OutputAccess = OutputAccess.Full,
                Name = formatIdOriginal + "-share",
                LanguageCode = "en"
            }).ConfigureAwait(false);

            var previewId = await CreateShareAndReturnId(new ShareBasicCreateRequest()
            {
                Contents = shareContents,
                OutputAccess = OutputAccess.Preview,
                Name = formatIdPreview + "-share",
                LanguageCode = "en"
            }).ConfigureAwait(false);

            await AssertSharesContainCorrectOutputs(fullId, previewId, formatIdOriginal, formatIdPreview);
        }

        [Fact]
        [Trait("Stack", "Shares")]
        public async Task ShouldContainDynamicOutputsEmbed()
        {
            var (formatIdOriginal, formatIdPreview, shareContents) = await PrepareDynamicOutputFormatTest();

            var fullId = await CreateShareAndReturnId(new ShareEmbedCreateRequest()
            {
                Contents = shareContents,
                OutputAccess = OutputAccess.Full,
                Name = formatIdOriginal + "-share"
            }).ConfigureAwait(false);

            var previewId = await CreateShareAndReturnId(new ShareEmbedCreateRequest()
            {
                Contents = shareContents,
                OutputAccess = OutputAccess.Preview,
                Name = formatIdPreview + "-share"
            }).ConfigureAwait(false);

            await AssertSharesContainCorrectOutputs(fullId, previewId, formatIdOriginal, formatIdPreview);
        }

        [Fact]
        public async Task ShouldRetrieveContentsBasedOnLimit()
        {
            // Arrange
            var shareId = await CreateShareWithContentsAndReturnId(10).ConfigureAwait(false);

            // Act
            var share = await _client.Share.GetAsync(shareId, new ShareResolveBehavior[0], 5).ConfigureAwait(false);

            // Assert
            share.ContentSelections.Should().HaveCount(5);
            share.ContentCount.Should().Be(10);
        }

        [Fact]
        public async Task ShouldPageOverContentsInShare()
        {
            // Arrange
            var shareId = await CreateShareWithContentsAndReturnId(10).ConfigureAwait(false);

            // Act
            string pageToken = null;
            ShareContentDetailResult result;

            var totalContents = 0;

            do
            {
                result = await _client.Share.GetContentsInShareAsync(shareId, 2, pageToken).ConfigureAwait(false);
                totalContents += result.Results.Count;
            }
            while ((pageToken = result.PageToken) != null);

            // Assert
            totalContents.Should().Be(10);
        }

        private async Task AssertSharesContainCorrectOutputs(
            string shareFullId,
            string sharePreviewId,
            string formatIdOriginal,
            string formatIdPreview)
        {
            var shareFull = await _client.Share.GetAsync(shareFullId).ConfigureAwait(false);
            var sharePreview = await _client.Share.GetAsync(sharePreviewId).ConfigureAwait(false);

            shareFull.ContentSelections.Single().Outputs.Should().Contain(o => o.OutputFormatId == formatIdPreview);
            shareFull.ContentSelections.Single().Outputs.Should().Contain(o => o.OutputFormatId == formatIdOriginal);

            sharePreview.ContentSelections.Single().Outputs.Should().Contain(o => o.OutputFormatId == formatIdPreview);
            sharePreview.ContentSelections.Single().Outputs.Should().NotContain(o => o.OutputFormatId == formatIdOriginal);
        }

        private async Task<(string outputFormatIdOriginal, string outputFormatIdPreview, ICollection<ShareContentBase> shareContent)> PrepareDynamicOutputFormatTest([CallerMemberName] string testName = null)
        {
            var formats = new[] { "Original", "Preview" }.Select(sourceFormat =>
            {
                var uniqueName = string.Join("-", testName, sourceFormat, Guid.NewGuid().ToString("N"));
                var formatName = uniqueName + "-OF";
                return new OutputFormat
                {
                    Id = formatName,
                    Names = new TranslatedStringDictionary
                    {
                        { "en", formatName }
                    },
                    Dynamic = true,
                    Format = new JpegFormat
                    {
                        Quality = 95
                    },
                    SourceOutputFormats = new SourceOutputFormats
                    {
                        Image = sourceFormat
                    }
                };
            }).ToList();

            await _client.OutputFormat.CreateManyAsync(new OutputFormatCreateManyRequest { Items = formats }).ConfigureAwait(false);

            var contentToShare = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 10).ConfigureAwait(false);
            return (
                formats[0].Id,
                formats[1].Id,
                new[] { new ShareContent { ContentId = contentToShare } }
            );
        }

        private async Task<List<ShareContentBase>> GetRandomShareContent(int count = 2)
        {
            var outputFormatIds = new List<string> { "Original" };

            var randomContents = await _fixture.GetRandomContentsAsync(string.Empty, count).ConfigureAwait(false);
            var shareContentItems = randomContents.Results.Select(i =>
                new ShareContent
                {
                    ContentId = i.Id,
                    OutputFormatIds = outputFormatIds
                }).ToList<ShareContentBase>();

            return shareContentItems;
        }

        private async Task<List<ShareContentBase>> GetRandomShareContent(string searchstring, int count = 2)
        {
            var outputFormatIds = new List<string> { "Original", "Preview" };

            var randomContents = await _fixture.GetRandomContentsAsync(searchstring, count).ConfigureAwait(false);
            var shareContentItems = randomContents.Results.Select(i =>
                new ShareContent
                {
                    ContentId = i.Id,
                    OutputFormatIds = outputFormatIds
                }).ToList<ShareContentBase>();

            return shareContentItems;
        }

        private async Task<string> CreateShareWithContentsAndReturnId(int count)
        {
            var contents = await _fixture.GetRandomContentsAsync("fileMetadata.fileExtension:.jpg", count).ConfigureAwait(false);
            return await CreateShareAndReturnId(
                new ShareBasicCreateRequest
                {
                    LanguageCode = _fixture.DefaultLanguage,
                    Contents = contents.Results.Select(
                        c => new ShareContent
                        {
                            ContentId = c.Id,
                            OutputFormatIds = new[] { "Original" }
                        }).ToList<ShareContentBase>(),
                    Name = $"{Guid.NewGuid():N}"
                }).ConfigureAwait(false);
        }

        private async Task<string> CreateShareAndReturnId(ShareBaseCreateRequest createRequest)
        {
            var createProcess = await _client.Share.CreateAsync(createRequest).ConfigureAwait(false);
            var waitResult = await _client.BusinessProcess.WaitForCompletionAsync(createProcess.Id).ConfigureAwait(false);

            if (waitResult.LifeCycleHit == BusinessProcessLifeCycle.Succeeded)
                _fixture.CreatedShareIds.Enqueue(createProcess.ReferenceId);

            return createProcess.ReferenceId;
        }
    }
}
