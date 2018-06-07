using Picturepark.Microsite.PressPortal.Services;
using Picturepark.SDK.V1.Contract;
using System;
using System.Threading.Tasks;

namespace Picturepark.Microsite.PressPortal.Helpers
{
    public class ServiceHelper : IServiceHelper
    {
        private readonly IPictureparkServiceClient _client;

        public ServiceHelper(IPictureparkServiceClient client)
        {
            _client = client;
        }

        public async Task EnsureSchemaExists<T>(Action<SchemaDetail> beforeCreateOrUpdateAction, bool updateSchema)
        {
            // Ensure that schema exists in PCP
            var schemas = await _client.Schemas.GenerateSchemasAsync(typeof(T));

            foreach (var schema in schemas)
            {
                beforeCreateOrUpdateAction?.Invoke(schema);

                if (!updateSchema)
                {
                    if (await _client.Schemas.ExistsAsync(schema.Id) == false)
                        await _client.Schemas.CreateAndWaitForCompletionAsync(schema, true);
                }
                else
                {
                    await _client.Schemas.CreateOrUpdateAndWaitForCompletionAsync(schema, true);
                }
            }
        }
    }
}
