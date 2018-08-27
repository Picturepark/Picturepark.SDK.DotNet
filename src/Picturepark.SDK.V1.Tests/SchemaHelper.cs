using System;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests
{
    public static class SchemaHelper
    {
        public static async Task<SchemaDetail> CreateSchemasIfNotExistentAsync<T>(IPictureparkService client)
            where T : class
        {
            var childSchemas = await client.Schema.GenerateSchemasAsync(typeof(T)).ConfigureAwait(false);

            foreach (var schema in childSchemas)
            {
                if (await client.Schema.ExistsAsync(schema.Id).ConfigureAwait(false) == false)
                {
                    try
                    {
                        await client.Schema.CreateAsync(schema, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                    }
                    catch (DuplicateSchemaException)
                    {
                        // ignore DuplicateSchemaException exceptions
                    }
                }
            }

            var schemaId = typeof(T).Name;
            return await client.Schema.GetAsync(schemaId).ConfigureAwait(false);
        }
    }
}