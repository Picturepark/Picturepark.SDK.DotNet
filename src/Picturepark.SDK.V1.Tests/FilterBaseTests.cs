using Picturepark.SDK.V1.Contract;
using System.Linq;
using Picturepark.SDK.V1.Contract.Attributes;
using Xunit;

namespace Picturepark.SDK.V1.Tests
{
    public class FilterBaseTests
    {
        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermFilterWithExpression()
        {
            // Act
            var termFilter = FilterBase.FromExpression<Person>(p => p.FirstName, "Foo");

            // Assert
            Assert.Equal("firstName", termFilter.Field);
            Assert.Equal("Foo", termFilter.Term);
        }

        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermFilterWithExpressionAndNGramAnalyzer()
        {
            // Act
            var termFilter = FilterBase.FromExpression<Person>(p => p.FirstName, "Foo", Analyzer.NGram);

            // Assert
            Assert.Equal("firstName.ngram", termFilter.Field);
            Assert.Equal("Foo", termFilter.Term);
        }

        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermsFilterWithExpression()
        {
            // Act
            var termFilter = FilterBase.FromExpression<Person>(p => p.FirstName, "Foo", "Bar");

            // Assert
            Assert.Equal("firstName", termFilter.Field);
            Assert.Equal(2, termFilter.Terms.Count);
            Assert.Equal("Foo", termFilter.Terms.ToList()[0]);
            Assert.Equal("Bar", termFilter.Terms.ToList()[1]);
        }

        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermsFilterWithExpressionAndLanguageAnalyzer()
        {
            // Act
            var termFilter = FilterBase.FromExpression<Person>(p => p.FirstName, Analyzer.Language, "Foo", "Bar");

            // Assert
            Assert.Equal("firstName.language", termFilter.Field);
            Assert.Equal(2, termFilter.Terms.Count);
            Assert.Equal("Foo", termFilter.Terms.ToList()[0]);
            Assert.Equal("Bar", termFilter.Terms.ToList()[1]);
        }

        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermFilterWithExpressionPath()
        {
            // Act
            var termFilter = FilterBase.FromExpression<Person>(p => p.Car.Name, "Foo");

            // Assert
            Assert.Equal("car.name", termFilter.Field);
        }

        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermFilterWithExpressionAndSchemaNamePrefix()
        {
            // Act
            var termFilter = FilterBase.FromExpression<CarSchema>(p => p.Model, "Supercar");

            // Assert
            Assert.Equal("carSchema.model", termFilter.Field);
        }

        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermFilterWithExpressionAndSchemaNamePrefixWithOverridenName()
        {
            // Act
            var termFilter = FilterBase.FromExpression<PersonSchema>(p => p.Name, "John");

            // Assert
            Assert.Equal("person.name", termFilter.Field);
        }

        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermFilterWithExpressionAndSchemaNamePrefixAndSimpleAnalyzer()
        {
            // Act
            var termFilter = FilterBase.FromExpression<CarSchema>(p => p.Model, "Supercar", Analyzer.Simple);

            // Assert
            Assert.Equal("carSchema.model.simple", termFilter.Field);
        }

        public class Person
        {
            public string FirstName { get; set; }

            public Car Car { get; set; }
        }

        public class Car
        {
            public string Name { get; set; }
        }

        [PictureparkSchema(SchemaType.Struct, "Person")]
        public class PersonSchema
        {
            public string Name { get; set; }
        }

        [PictureparkSchema(SchemaType.List)]
        public class CarSchema
        {
            public string Model { get; set; }
        }
    }
}
