using System;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Attributes.Analyzer;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests
{
    public class AnalyzerTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly PictureparkClient _client;

        public AnalyzerTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Analyzer")]
        public async Task ShouldSearchAnalyzedFields()
        {
            // Arrange
            if (await _client.Schemas.ExistsAsync(nameof(AnalyzerTestObject)).ConfigureAwait(false) == false)
            {
                var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(AnalyzerTestObject)).ConfigureAwait(false);
                foreach (var schema in schemas)
                {
                    await _client.Schemas.CreateOrUpdateAsync(schema, false, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                }

                var analyzerValue = new AnalyzerTestObject
                {
                    EdgeNGramField = "EdgeNGramFieldValue",
                    LanguageField = new TranslatedStringDictionary
                    {
                        { "en", "Cities" }
                    },
                    NGramField = "NGramFieldValue",
                    PathHierarchyField = "Path/Hierarchy/Field",
                    SimpleField = "Simple12Field"
                };

                await _client.ListItems.CreateFromObjectAsync(analyzerValue, nameof(AnalyzerTestObject)).ConfigureAwait(false);
            }

            var requestSchemaIds = new[] { nameof(AnalyzerTestObject) };

            var simpleResults = await _client.ListItems.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.SimpleField, "simple", Analyzer.Simple)
            }).ConfigureAwait(false);

            Assert.True(simpleResults.TotalResults > 0);

            var pathResults = await _client.ListItems.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.PathHierarchyField, "Path/Hierarchy", Analyzer.PathHierarchy)
            }).ConfigureAwait(false);

            Assert.True(pathResults.TotalResults > 0);

            var languageResults = await _client.ListItems.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.LanguageField, "citi", language: "en", useAnalyzer: true)
            }).ConfigureAwait(false);

            Assert.True(languageResults.TotalResults > 0);

            var edgeNgramResults = await _client.ListItems.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.EdgeNGramField, "edg", Analyzer.EdgeNGram)
            }).ConfigureAwait(false);

            Assert.True(edgeNgramResults.TotalResults > 0);

            var ngramResults = await _client.ListItems.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.NGramField, "mfield", Analyzer.NGram)
            }).ConfigureAwait(false);

            Assert.True(ngramResults.TotalResults > 0);
        }

        [PictureparkSchema(SchemaType.List)]
        public class AnalyzerTestObject
        {
            [PictureparkEdgeNGramAnalyzer(Index = true)]
            public string EdgeNGramField { get; set; }

            [PictureparkNGramAnalyzer(Index = true)]
            public string NGramField { get; set; }

            [PictureparkPathHierarchyAnalyzer(Index = true)]
            public string PathHierarchyField { get; set; }

            [PictureparkSimpleAnalyzer(Index = true)]
            public string SimpleField { get; set; }

            [PictureparkLanguageAnalyzer(Index = true)]
            public TranslatedStringDictionary LanguageField { get; set; }
        }
    }
}
