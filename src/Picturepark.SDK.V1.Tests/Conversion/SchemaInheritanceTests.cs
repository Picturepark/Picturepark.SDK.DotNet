using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NJsonSchema.Converters;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Conversion
{
    public class SchemaInheritanceTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly PictureparkClient _client;

        public SchemaInheritanceTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldOverwriteField()
        {
            /// Act
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(Person));

            /// Assert
            Assert.Equal(2, schemas.Count);

            // check person
            var person = schemas.First();
            Assert.Equal(3, person.Fields.Count);
            Assert.Equal(0, person.FieldsOverwrite.Count);

            Assert.False(person.Fields.First(f => f.Id == "parent").Required);

            // check teacher
            var teacher = schemas.Last();
            Assert.Equal(0, teacher.Fields.Count);
            Assert.Equal(1, teacher.FieldsOverwrite.Count);

            Assert.True(teacher.FieldsOverwrite.First(f => f.Id == "parent").Required);

            // check PictureparkListItemCreateTemplateAttribute
            var field = (FieldOverwriteSingleTagbox)teacher.FieldsOverwrite.First(f => f.Id == "parent");
            Assert.Equal("foo", field.ListItemCreateTemplate);
            Assert.True(field.OverwriteListItemCreateTemplate);
        }

        [PictureparkReference]
        [PictureparkSchemaType(SchemaType.List)]
        [PictureparkSchemaType(SchemaType.Struct)]
        [JsonConverter(typeof(JsonInheritanceConverter), "kind")]
        public class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            [PictureparkTagbox("{ 'kind': 'TermFilter', 'field': 'firstName', Term: 'Foo' }")]
            public virtual Person Parent { get; set; }
        }

        public class Teacher : Person
        {
            [PictureparkRequired]
            [PictureparkListItemCreateTemplate("foo")]
            public override Person Parent { get; set; }
        }
    }
}
