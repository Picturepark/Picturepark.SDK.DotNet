using Picturepark.SDK.V1.Tests.Contracts;
using System;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ContentFixture : ClientFixture
    {
        public ContentFixture()
        {
            Setup().GetAwaiter().GetResult();
        }

        private async Task Setup()
        {
            await SetupSchema(typeof(ContentItem)).ConfigureAwait(false);
            await SetupSchema(typeof(AllDataTypesContract)).ConfigureAwait(false);
            await SetupSchema(typeof(PersonShot)).ConfigureAwait(false);
            await SetupSchema(typeof(SimpleLayer)).ConfigureAwait(false);
        }

        private async Task SetupSchema(Type type)
        {
            var schemas = await Client.Schema.GenerateSchemasAsync(type).ConfigureAwait(false);
            await Client.Schema.CreateManyAsync(schemas, true, TimeSpan.FromMinutes(3)).ConfigureAwait(false);
        }
    }
}
