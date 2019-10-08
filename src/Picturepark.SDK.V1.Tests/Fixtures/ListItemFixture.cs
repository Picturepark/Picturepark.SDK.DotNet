using Picturepark.SDK.V1.Tests.Contracts;
using System.Threading.Tasks;
using FluentAssertions;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ListItemFixture : ClientFixture
    {
        public ListItemFixture()
        {
            Setup().Wait();
        }

        public async Task Setup()
        {
            await SetupSchema<Tag>().ConfigureAwait(false);

            (await Client.Info.GetInfoAsync().ConfigureAwait(false)).LanguageConfiguration.MetadataLanguages.Should()
                .Contain(new[] { "en", "de" }, "some tests require customer used for testing to have both 'en' and 'de' metadata languages configured");
        }
    }
}
