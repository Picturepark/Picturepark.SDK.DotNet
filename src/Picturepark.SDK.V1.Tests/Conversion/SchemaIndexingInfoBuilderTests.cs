#pragma warning disable SA1201 // Elements must appear in the correct order

using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.SystemTypes;
using Picturepark.SDK.V1.Tests.Fixtures;
using System.Linq;
using Picturepark.SDK.V1.Contract.Builders;
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
        public void ShouldIncludeAllPropertiesAndChangeSpecifiedProperties()
        {
            /// Act
            var schemaIndexingInfo = new SchemaIndexingInfoBuilder<Parent>()
                .AddProperties(0)
                .AddProperty(p => p.Child)
                    .WithBoost(11)
                    .WithIndex()
                .AddProperty(p => p.Bar)
                .Build();

            /// Assert
            Assert.Equal(4, schemaIndexingInfo.Fields.Count);
            Assert.Equal("child", schemaIndexingInfo.Fields.First().Id);
            Assert.Equal(11, schemaIndexingInfo.Fields.First().Boost);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public void ShouldChangeAllProperties()
        {
            /// Act
            var schemaIndexingInfo = new SchemaIndexingInfoBuilder<Parent>()
                .AddProperties(0)
                    .WithIndex()
                .Build();

            /// Assert
            Assert.True(schemaIndexingInfo.Fields.All(f => f.Index));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public void ShouldLoadChildProperties()
        {
            /// Act
            var schemaIndexingInfo = new SchemaIndexingInfoBuilder<Parent>()
                .AddProperties(1)
                .Build();

            /// Assert
            Assert.True(schemaIndexingInfo.Fields
                .First(f => f.Id == "child")
                .RelatedSchemaIndexing
                .Fields
                .Any(f => f.Id == "firstName"));
            Assert.True(schemaIndexingInfo.Fields.Any(f => f.Id == "child2"));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public void ShouldLoadOnlyTagboxChildren()
        {
            /// Act
            var schemaIndexingInfo = new SchemaIndexingInfoBuilder<Parent>()
                .AddPropertiesAndTagboxes(1)
                .Build();

            /// Assert
            Assert.True(schemaIndexingInfo.Fields.Any(f => f.Id == "child"));
            Assert.False(schemaIndexingInfo.Fields.Any(f => f.Id == "child2"));
        }

        public class Parent
        {
            [PictureparkTagbox("{ 'kind': 'TermFilter', 'field': 'contentType', Term: 'FC Aarau' }")]
            public Child Child { get; set; }

            public Child Child2 { get; set; }

            public string Foo { get; set; }

            public string Bar { get; set; }
        }

        [PictureparkSchemaType(SchemaType.Struct)]
        public class Child : Relation
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}
