using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Attributes.Analyzer;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests
{
    public class AnalyzerTests : IClassFixture<SDKClientFixture>
    {
        private readonly SDKClientFixture _fixture;
        private readonly PictureparkClient _client;

        public AnalyzerTests(SDKClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Analyzer")]
        public async Task ShouldSearchAnalyzedFields()
        {
            /// Arrange
	        if (await _client.Schemas.ExistsAsync(nameof(AnalyzerTestObject)) == false)
            {
                var schemas = _client.Schemas.GenerateSchemaFromPOCO(typeof(AnalyzerTestObject));
                foreach (var schema in schemas)
                {
                    await _client.Schemas.CreateOrUpdateAsync(schema, false);
                }

                var analyzerValue = new AnalyzerTestObject
                {
                    EdgeNGramField = "EdgeNGramFieldValue",
                    LanguageField = new TranslatedStringDictionary
                    {
                        { "x-default", "XDefaultValue" },
                        { "en", "Cities" }
                    },
                    NGramField = "NGramFieldValue",
                    PathHierarchyField = "Path/Hierarchy/Field",
                    SimpleField = "Simple12Field"
                };

                await _client.ListItems.CreateFromPOCOAsync(analyzerValue, nameof(AnalyzerTestObject));
            }

            var simpleResults = await _client.ListItems.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = new List<string> { nameof(AnalyzerTestObject) },
                Filter = new TermFilter
                {
                    Field = "analyzerTestObject.simpleField.simple", // TODO: How to support this with SDK
                    Term = "simple"
                }
            });

            Assert.True(simpleResults.TotalResults > 0);

            var pathResults = await _client.ListItems.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = new List<string> { nameof(AnalyzerTestObject) },
                Filter = new TermFilter
                {
                    Field = "analyzerTestObject.pathHierarchyField.pathhierarchy", // TODO: How to support this with SDK
                    Term = "Path/Hierarchy"
                }
            });

            Assert.True(pathResults.TotalResults > 0);

            var languageResults = await _client.ListItems.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = new List<string> { nameof(AnalyzerTestObject) },
                Filter = new TermFilter
                {
                    Field = "analyzerTestObject.languageField.en.language",
                    Term = "citi" // TODO: We should use MatchQuery here
                }
            });

            Assert.True(languageResults.TotalResults > 0);

            var edgeNgramResults = await _client.ListItems.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = new List<string> { nameof(AnalyzerTestObject) },
                Filter = new TermFilter
                {
                    Field = "analyzerTestObject.edgeNGramField.edgengram",
                    Term = "edg"
                }
            });

            Assert.True(edgeNgramResults.TotalResults > 0);

            var ngramResults = await _client.ListItems.SearchAsync(new ListItemSearchRequest
            {
                SchemaIds = new List<string> { nameof(AnalyzerTestObject) },
                Filter = new TermFilter
                {
                    Field = "analyzerTestObject.nGramField.ngram",
                    Term = "mfield"
                }
            });

            Assert.True(ngramResults.TotalResults > 0);
        }

        [PictureparkSchemaType(SchemaType.List)]
        public class AnalyzerTestObject
        {
            [PictureparkEdgeNGramAnalyzer]
            public string EdgeNGramField { get; set; }

            [PictureparkNGramAnalyzer]
            public string NGramField { get; set; }

            [PictureparkPathHierarchyAnalyzer]
            public string PathHierarchyField { get; set; }

            [PictureparkSimpleAnalyzer]
            public string SimpleField { get; set; }

            [PictureparkLanguageAnalyzer]
            public TranslatedStringDictionary LanguageField { get; set; }
        }
    }
}
