﻿using System;
using System.Linq;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Threading.Tasks;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "ConversionPresetTemplates")]
    public class ConversionPresetTemplateTests : IClassFixture<ClientFixture>
    {
        private readonly IPictureparkService _client;

        public ConversionPresetTemplateTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task ShouldCreateUpdateAndDelete()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString("N");
            var createRequest = new ConversionPresetTemplateCreateRequest
            {
                Names = new TranslatedStringDictionary { { "en", $"Template Name {guid}" } },
                Descriptions = new TranslatedStringDictionary { { "en", $"Template Description {guid}" } },
                Template = $"My template {guid}",
                OutputFormatId = "Preview"
            };

            var conversionPresetTemplate = await _client.ConversionPresetTemplate.CreateAsync(createRequest);
            conversionPresetTemplate.Should().NotBeNull();
            conversionPresetTemplate.Template.Should().EndWith(guid);

            var updateRequest = new ConversionPresetTemplateUpdateRequest
            {
                Names = createRequest.Names,
                Descriptions = createRequest.Descriptions,
                Template = createRequest.Template + " mod"
            };
            conversionPresetTemplate = await _client.ConversionPresetTemplate.UpdateAsync(conversionPresetTemplate.Id, updateRequest);
            conversionPresetTemplate.Should().NotBeNull();
            conversionPresetTemplate.Template.Should().EndWith("mod");

            await _client.ConversionPresetTemplate.DeleteAsync(conversionPresetTemplate.Id);

            await Assert.ThrowsAsync<ConversionPresetTemplateNotFoundException>(async () => await _client.ConversionPresetTemplate.GetAsync(conversionPresetTemplate.Id));
        }

        [Fact]
        public async Task ShouldCreateManyUpdateManyAndDeleteMany()
        {
            var createRequestMany = new ConversionPresetTemplateCreateManyRequest
            {
                Items = Enumerable.Range(0, 3)
                    .Select(
                        i => new ConversionPresetTemplateCreateRequest
                        {
                            Names = new TranslatedStringDictionary { { "en", $"Template Name {i}" } },
                            Descriptions = new TranslatedStringDictionary { { "en", $"Template Description {i}" } },
                            Template = $"My template {i}",
                            RequestId = i.ToString(),
                            OutputFormatId = "Preview"
                        })
                    .ToArray()
            };

            var businessProcess = await _client.ConversionPresetTemplate.CreateManyAsync(createRequestMany);
            var waitResult = await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);
            waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);

            var businessProcessBatch = await _client.BusinessProcess.GetSuccessfulItemsAsync(businessProcess.Id, 3);
            var successfulRows = ((BusinessProcessBatchItemBatchResponse)businessProcessBatch.Data).Items;
            successfulRows.Count.Should().Be(3);
            successfulRows.Select(r => r.RequestId).Should().BeEquivalentTo("0", "1", "2");

            var createdIds = successfulRows.Select(r => r.Id).ToArray();

            var updateRequestMany = new ConversionPresetTemplateUpdateManyRequest
            {
                Items = createdIds
                    .Select(
                        id => new ConversionPresetTemplateUpdateManyRequestItem()
                        {
                            Id = id,
                            Names = new TranslatedStringDictionary { { "en", $"Template Name {id} mod" } },
                            Descriptions = new TranslatedStringDictionary { { "en", $"Template Description {id} mod" } },
                            Template = $"My template {id} mod",
                        })
                    .ToArray()
            };

            businessProcess = await _client.ConversionPresetTemplate.UpdateManyAsync(updateRequestMany);
            waitResult = await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);
            waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);

            businessProcessBatch = await _client.BusinessProcess.GetSuccessfulItemsAsync(businessProcess.Id, 3);
            successfulRows = ((BusinessProcessBatchItemBatchResponse)businessProcessBatch.Data).Items;
            successfulRows.Count.Should().Be(3);
            successfulRows.Select(r => r.Id).Should().BeEquivalentTo(createdIds);

            var conversionPresetTemplates = await _client.ConversionPresetTemplate.GetManyAsync(createdIds);
            conversionPresetTemplates.Should().HaveCount(createdIds.Length);
            conversionPresetTemplates.Select(p => p.Template).Should().OnlyContain(s => s.EndsWith("mod"));

            businessProcess = await _client.ConversionPresetTemplate.DeleteManyAsync(new ConversionPresetTemplateDeleteManyRequest { Ids = createdIds });
            waitResult = await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);
            waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);

            conversionPresetTemplates = await _client.ConversionPresetTemplate.GetManyAsync(createdIds);
            conversionPresetTemplates.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldSearch()
        {
            var createRequestMany = new ConversionPresetTemplateCreateManyRequest
            {
                Items = Enumerable.Range(0, 3)
                    .Select(
                        i => new ConversionPresetTemplateCreateRequest
                        {
                            Names = new TranslatedStringDictionary { { "en", $"Template Name {i}" } },
                            Descriptions = new TranslatedStringDictionary { { "en", $"Template Description {i}" } },
                            Template = $"My template {i}",
                            RequestId = i.ToString(),
                            OutputFormatId = "Preview"
                        })
                    .ToArray()
            };

            var businessProcess = await _client.ConversionPresetTemplate.CreateManyAsync(createRequestMany);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);
            var successfulRows = ((BusinessProcessBatchItemBatchResponse)(await _client.BusinessProcess.GetSuccessfulItemsAsync(businessProcess.Id, 3)).Data).Items;
            var searchRequest = new ConversionPresetTemplateSearchRequest
            {
                Filter = new TermsFilter { Field = nameof(ConversionPresetTemplate.Id).ToLowerCamelCase(), Terms = successfulRows.Select(r => r.Id).ToArray() }, SearchString = "1"
            };

            var searchResult = await _client.ConversionPresetTemplate.SearchAsync(searchRequest);
            searchResult.Results.Should().HaveCount(1).And.Subject.First().Template.Should().Be("My template 1");
        }
    }
}