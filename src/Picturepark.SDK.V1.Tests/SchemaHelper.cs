using System;
using System.Collections.Generic;
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
            var schemasToCreate = new List<SchemaDetail>();

            foreach (var schema in childSchemas)
            {
                if (!await client.Schema.ExistsAsync(schema.Id).ConfigureAwait(false))
                {
                    schemasToCreate.Add(schema);
                }
            }

            await client.Schema.CreateManyAsync(schemasToCreate, true, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            var schemaId = typeof(T).Name;
            return await client.Schema.GetAsync(schemaId).ConfigureAwait(false);
        }
    }
}