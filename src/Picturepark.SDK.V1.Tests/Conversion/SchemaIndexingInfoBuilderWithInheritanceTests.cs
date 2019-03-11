#pragma warning disable SA1201 // Elements must appear in the correct order

using System;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.SystemTypes;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Picturepark.SDK.V1.Builders;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Conversion
{
    public class SchemaIndexingInfoBuilderWithInheritanceTests
    {
        public class Parent
        {
            [PictureparkTagbox("{ 'kind': 'TermFilter', 'field': 'contentType', Term: 'FC Aarau' }")]
            public Child Child { get; set; }

            public Child[] Children { get; set; }

            public string Foo { get; set; }

            public string Bar { get; set; }
        }

        [KnownType(typeof(SpecialChild))]
        [PictureparkSchema(SchemaType.Struct)]
        public class Child : Relation
        {
            [PictureparkSearch(Index = true, Boost = 5, SimpleSearch = true)]
            public string FirstName { get; set; }

            [PictureparkSearch(Index = true, Boost = 25, SimpleSearch = true)]
            public string LastName { get; set; }

            public DateTime DateOfBirth { get; set; }
        }

        public class SpecialChild : Child
        {
            [PictureparkSearch(Index = true, Boost = 50, SimpleSearch = true)]
            public string Speciality { get; set; }
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public void ShouldAddIndexOnPropertyPathWithInheritance()
        {
            /// Arrange
            var builder = new SchemaIndexingInfoBuilder<Parent>();

            /// Act
            var info = builder
                .AddIndex(p => ((SpecialChild)p.Child).Speciality)
                .Build();

            /// Assert
            Assert.Contains(info.Fields.Single(f => f.Id == "child").RelatedSchemaIndexing.Fields, f => f.Id == "speciality");
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public void ShouldAddIndexOnCollectionPathWithInheritance()
        {
            /// Arrange
            var builder = new SchemaIndexingInfoBuilder<Parent>();

            /// Act
            var info = builder
                .AddIndex(p => p.Children.OfType<SpecialChild>().Select(c => c.Speciality))
                .Build();

            /// Assert
            Assert.Contains(info.Fields.Single(f => f.Id == "children").RelatedSchemaIndexing.Fields, f => f.Id == "speciality");

            Assert.Null(info.Fields.Single(f => f.Id == "children")
                .RelatedSchemaIndexing.Fields.Single(f => f.Id == "speciality")
                .RelatedSchemaIndexing);
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public void ShouldAddDefaultIndexesWithInheritance()
        {
            /// Arrange
            var builder = new SchemaIndexingInfoBuilder<Parent>();

            /// Act
            var info = builder
                .AddDefaultIndexes(p => p.Child, 1)
                .Build();

            /// Assert
            Assert.Contains(info.Fields.Single(f => f.Id == "child").RelatedSchemaIndexing.Fields, f => f.Id == "speciality");
        }
    }
}
