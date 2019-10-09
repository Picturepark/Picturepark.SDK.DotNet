using System;
using System.Linq;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Contracts;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "DisplayValue")]
    public class DisplayValueTests : IClassFixture<ClientFixture>
    {
        private readonly IPictureparkService _client;

        public DisplayValueTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task ShouldGetStatus()
        {
            // Act
            var displayValue = await _client.DisplayValue.GetStatusAsync().ConfigureAwait(false);

            // Assert
            displayValue.State.Should().NotBeNull();
            displayValue.ContentOrLayerSchemaIds.Should().NotBeNull();
            displayValue.ListSchemaIds.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldRerenderDisplayValuesWhenStatusIsRed()
        {
            // Arrange: create content with predefined schema
            var schema = await SchemaHelper.CreateSchemasIfNotExistentAsync<DisplayPatternTest>(_client).ConfigureAwait(false);

            var uniqueName = $"Name_{Guid.NewGuid():N}";
            var request = new ContentCreateRequest
            {
                Content = new DisplayPatternTest { Name = uniqueName },
                ContentSchemaId = nameof(DisplayPatternTest)
            };

            var content = await _client.Content.CreateAsync(request, new[] { ContentResolveBehavior.OuterDisplayValueThumbnail }).ConfigureAwait(false);
            var expectedDisplayValue = schema.DisplayPatterns.First(dp => dp.DisplayPatternType == DisplayPatternType.Thumbnail).Templates["en"].Replace("{{data.displayPatternTest.name}}", uniqueName);
            content.DisplayValues.Thumbnail.Should().Be(expectedDisplayValue);

            // Update schema's display pattern in order to turn the display value status to red
            var uniqueId = $"{Guid.NewGuid():N}";
            schema.DisplayPatterns.First(dp => dp.DisplayPatternType == DisplayPatternType.Thumbnail).Templates["en"] = $"{{{{data.displayPatternTest.name}}}}_{uniqueId}";
            await _client.Schema.UpdateAsync(schema, false).ConfigureAwait(false);

            var displayValueStatus = await _client.DisplayValue.GetStatusAsync().ConfigureAwait(false);
            displayValueStatus.State.Should().Be(DisplayValuesState.Outdated);
            displayValueStatus.ContentOrLayerSchemaIds.Should().Contain(nameof(DisplayPatternTest));

            // Execute the re-rendering and await for its completion
            var businessProcess = await _client.DisplayValue.RerenderAsync().ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id).ConfigureAwait(false);

            // Assert
            content = await _client.Content.GetAsync(content.Id, new[] { ContentResolveBehavior.OuterDisplayValueThumbnail }).ConfigureAwait(false);
            content.DisplayValues[DisplayPatternType.Thumbnail.ToString().ToLowerCamelCase()].Should().Be($"{uniqueName}_{uniqueId}");

            displayValueStatus = await _client.DisplayValue.GetStatusAsync().ConfigureAwait(false);
            displayValueStatus.State.Should().Be(DisplayValuesState.UpToDate);
            displayValueStatus.ContentOrLayerSchemaIds.Should().NotContain(nameof(DisplayPatternTest));
        }
    }
}