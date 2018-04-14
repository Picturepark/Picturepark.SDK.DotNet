using Picturepark.SDK.V1.Contract;
using System.Linq;
using Xunit;

namespace Picturepark.SDK.V1.Tests
{
    public class FilterBaseTests
    {
        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermFilterWithExpression()
        {
            /// Act
            var termFilter = FilterBase.FromExpression<Person>(p => p.FirstName, "Foo");

            /// Assert
            Assert.Equal("firstName", termFilter.Field);
            Assert.Equal("Foo", termFilter.Term);
        }

        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermsFilterWithExpression()
        {
            /// Act
            var termFilter = FilterBase.FromExpression<Person>(p => p.FirstName, "Foo", "Bar");

            /// Assert
            Assert.Equal("firstName", termFilter.Field);
            Assert.Equal(2, termFilter.Terms.Count);
            Assert.Equal("Foo", termFilter.Terms.ToList()[0]);
            Assert.Equal("Bar", termFilter.Terms.ToList()[1]);
        }

        [Fact]
        [Trait("Stack", "SDK")]
        public void ShouldCreateTermFilterWithExpressionPath()
        {
            /// Act
            var termFilter = FilterBase.FromExpression<Person>(p => p.Car.Name, "Foo");

            /// Assert
            Assert.Equal("car.name", termFilter.Field);
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
    }
}
