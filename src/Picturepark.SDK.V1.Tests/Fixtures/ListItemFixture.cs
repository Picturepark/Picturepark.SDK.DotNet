using Picturepark.SDK.V1.Tests.Contracts;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ListItemFixture : ClientFixture, IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            await SetupSchema<Tag>();

            (await Client.Info.GetInfoAsync()).LanguageConfiguration.MetadataLanguages.Should()
                .Contain(new[] { "en", "de" }, "some tests require customer used for testing to have both 'en' and 'de' metadata languages configured");

            await SchemaHelper.CreateSchemasIfNotExistentAsync<Person>(Client);
        }

        public Task DisposeAsync() => Task.FromResult(0);
    }
}
