#pragma warning disable SA1201 // Elements must appear in the correct order

using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Attributes.Providers;
using Picturepark.SDK.V1.Contract.SystemTypes;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Linq;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Conversion
{
    public class SchemaIndexingInfoBuilderTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly PictureparkClient _client;

        public SchemaIndexingInfoBuilderTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public void Bcd()
        {
            /// Act
            var schemaIndexingInfo = new SchemaIndexingInfoBuilder<Test>()
                .AddProperties()
                .AddProperty(p => p.Aaa)
                    .WithBoost(11)
                    .WithIndex()
                .AddProperty(p => p.Bar)
                .Build();

            /// Assert
            Assert.Equal(3, schemaIndexingInfo.Fields.Count);
            Assert.Equal("aaa", schemaIndexingInfo.Fields.First().Id);
            Assert.Equal(11, schemaIndexingInfo.Fields.First().Boost);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public void Abc()
        {
            /// Act
            var schemaIndexingInfo = new SchemaIndexingInfoBuilder<Test>()
                .AddProperties()
                    .WithIndex()
                .Build();

            /// Assert
            Assert.True(schemaIndexingInfo.Fields.All(f => f.Index));
        }

        public class Test
        {
            public Aaa Aaa { get; set; }

            public string Foo { get; set; }

            public string Bar { get; set; }
        }

        [PictureparkSchemaType(SchemaType.Struct)]
        public class Aaa : Relation
        {
            public string Foo { get; set; }

            public string Bar { get; set; }
        }
    }
}
