using System;
using Picturepark.SDK.V1.Tests.Contracts;
using System.Linq;
using System.Threading.Tasks;

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
            if (await Client.Schemas.ExistsAsync(nameof(Tag)).ConfigureAwait(false) == false)
            {
                var schema = await Client.Schemas.GenerateSchemasAsync(typeof(Tag)).ConfigureAwait(false);
                await Client.Schemas.CreateAsync(schema.First(), true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            }
        }
    }
}
