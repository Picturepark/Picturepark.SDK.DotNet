using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "XmpMapping")]
    public class XmpMappingTests : IClassFixture<ClientFixture>, IAsyncLifetime
    {
        private readonly ClientFixture _fixture;

        private SchemaDetail _schema;

        public XmpMappingTests(ClientFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _schema = await SchemaHelper.CreateSchemasIfNotExistentAsync<TargetSchema>(_fixture.Client);
        }

        public async Task DisposeAsync()
        {
            await Task.Delay(0);
        }

        [Fact]
        public async Task Should_create_mapping()
        {
            await CreateMapping();
        }

        [Fact]
        public async Task Should_search_for_mappings()
        {
            await CreateMapping();

            var searchResult = await _fixture.Client.XmpMapping.SearchAsync(new XmpMappingEntrySearchRequest { Limit = 30 });
            searchResult.TotalResults.Should().BeGreaterThan(0);
            searchResult.Results.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Should_get_target_fields()
        {
            var targets = await _fixture.Client.XmpMapping.GetAvailableTargetsAsync();
            targets.MetadataFields.Should().ContainSingle(m => m.Path == $"{_schema.Id}.title");
        }

        private async Task CreateMapping()
        {
            var businessProcess = await _fixture.Client.XmpMapping.CreateAsync(
                new XmpMappingEntryCreateRequest
                {
                    XmpPath = "XmpMetadata.dc.title",
                    MetadataPath = $"{_schema.Id}.title",
                    Direction = MappingDirection.XmpToMetadata
                });

            var result = await _fixture.Client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);
            result.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);
        }

        [PictureparkSchema(SchemaType.Layer)]
        private class TargetSchema
        {
            public string Title { get; set; }
        }
    }
}