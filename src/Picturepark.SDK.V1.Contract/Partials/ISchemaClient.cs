using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface ISchemaClient
    {
        List<SchemaDetailViewItem> GenerateSchemaFromPOCO(Type type, List<SchemaDetailViewItem> schemaList, bool generateDependencySchema = true);

        Task CreateOrUpdateAsync(SchemaDetailViewItem schema);

        void CreateOrUpdate(SchemaDetailViewItem schema);

        Task CreateAsync(SchemaDetailViewItem schema, bool enableForBinaryFiles);

        void Create(SchemaDetailViewItem schema, bool enableForBinaryFiles);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task CreateAsync(SchemaDetailViewItem schema);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        void Create(SchemaDetailViewItem schema);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task DeleteAsync(string schemaId);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task UpdateAsync(SchemaDetailViewItem schema);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task UpdateAsync(string schemaId, SchemaUpdateRequest updateRequest);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<bool> ExistsAsync(string schemaId);

        bool Exists(string schemaId);
    }
}