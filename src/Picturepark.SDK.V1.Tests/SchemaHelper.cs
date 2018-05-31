using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests
{
    public static class SchemaHelper
    {
        public static async Task<SchemaDetail> CreateSchemasIfNotExistentAsync<T>(PictureparkClient client)
            where T : class
        {
            var childSchemas = await client.Schemas.GenerateSchemasAsync(typeof(T));

            foreach (var schema in childSchemas)
            {
                if (await client.Schemas.ExistsAsync(schema.Id) == false)
                {
                    try
                    {
                        await client.Schemas.CreateAndWaitForCompletionAsync(schema, true);
                    }
                    catch (DuplicateSchemaException)
                    {
                        // ignore DuplicateSchemaException exceptions
                    }
                }
            }

            var schemaId = typeof(T).Name;
            return await client.Schemas.GetAsync(schemaId);
        }
    }
}