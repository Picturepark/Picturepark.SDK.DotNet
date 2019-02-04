using Picturepark.SDK.V1.Tests.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

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
            var schemasToCreate = new List<SchemaDetail>();

            foreach (var schema in schemas)
            {
                if (!await Client.Schema.ExistsAsync(schema.Id).ConfigureAwait(false))
                {
                    schemasToCreate.Add(schema);
                }
            }

            await Client.Schema.CreateManyAsync(schemasToCreate, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        }
    }
}
