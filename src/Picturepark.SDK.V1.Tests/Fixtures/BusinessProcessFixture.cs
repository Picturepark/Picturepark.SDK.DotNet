using System;
using Picturepark.SDK.V1.Tests.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class BusinessProcessFixture : ClientFixture
    {
        public BusinessProcessFixture()
        {
            Setup().Wait();
        }

        public async Task Setup()
        {
            if (await Client.Schema.ExistsAsync(nameof(BusinessProcessTest)) == false)
            {
                var schema = await Client.Schema.GenerateSchemasAsync(typeof(BusinessProcessTest));
                await Client.Schema.CreateAsync(schema.First(), true, TimeSpan.FromMinutes(1));
            }
        }
    }
}
