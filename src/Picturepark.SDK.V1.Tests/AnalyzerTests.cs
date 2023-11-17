using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
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
        private readonly IPictureparkService _client;

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
            if (!await _client.Schema.ExistsAsync(nameof(AnalyzerTestObject)))
            {
                var schemas = await _client.Schema.GenerateSchemasAsync(typeof(AnalyzerTestObject));
                var schemasToCreate = new List<SchemaDetail>();
                var schemasToUpdate = new List<SchemaDetail>();

                foreach (var schema in schemas)
                {
                    if (await _client.Schema.ExistsAsync(schema.Id))
                    {
                        schemasToUpdate.Add(schema);
                    }
                    else
                    {
                        schemasToCreate.Add(schema);
                    }
                }

                await _client.Schema.CreateManyAsync(schemasToCreate, false, TimeSpan.FromMinutes(1));

                foreach (var schema in schemasToUpdate)
                {
                    await _client.Schema.UpdateAsync(schema, true, TimeSpan.FromMinutes(1));
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
                    SimpleField = "Simple12Field",
                    NoDiacriticsField = "Bröt",
                    KeywordLowercaseField = "JpG"
                };

                var res = await _client.ListItem.CreateFromObjectAsync(analyzerValue);
                var resDetail = await res.FetchDetail();
                resDetail.SucceededItems.Should().NotBeEmpty();
            }

            var requestSchemaIds = new[] { nameof(AnalyzerTestObject) };

            var simpleResults = await _client.ListItem.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.SimpleField, "simple", Analyzer.Simple)
            });

            Assert.True(simpleResults.TotalResults > 0);

            var pathResults = await _client.ListItem.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.PathHierarchyField, "Path/Hierarchy", Analyzer.PathHierarchy)
            });

            Assert.True(pathResults.TotalResults > 0);

            var languageResults = await _client.ListItem.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.LanguageField, "citi", language: "en", useAnalyzer: true)
            });

            Assert.True(languageResults.TotalResults > 0);

            var edgeNgramResults = await _client.ListItem.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.EdgeNGramField, "edg", Analyzer.EdgeNGram)
            });

            Assert.True(edgeNgramResults.TotalResults > 0);

            var ngramResults = await _client.ListItem.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.NGramField, "mfield", Analyzer.NGram)
            });

            Assert.True(ngramResults.TotalResults > 0);

            var noDiactricsResults = await _client.ListItem.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.NoDiacriticsField, "brot", Analyzer.NoDiacritics)
            });

            Assert.True(noDiactricsResults.TotalResults > 0);

            var keywordLowercaseResults = await _client.ListItem.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = requestSchemaIds,
                Filter = FilterBase.FromExpression<AnalyzerTestObject>(o => o.KeywordLowercaseField, "jPg", Analyzer.KeywordLowercase)
            });

            Assert.True(keywordLowercaseResults.TotalResults > 0);
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

            [PictureparkNoDiacriticsAnalyzer(Index = true)]
            public string NoDiacriticsField { get; set; }

            [PictureparkKeywordLowercaseAnalyzer(Index = true)]
            public string KeywordLowercaseField { get; set; }
        }
    }
}
