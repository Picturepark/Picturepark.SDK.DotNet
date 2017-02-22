using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IMetadataSchemasClient
    {
        List<MetadataSchemaDetailViewItem> GenerateSchemaFromPOCO(Type type, List<MetadataSchemaDetailViewItem> schemaList, bool generateDependencySchema = true);

        Task CreateOrUpdateAsync(MetadataSchemaDetailViewItem metadataSchema);

        void CreateOrUpdate(MetadataSchemaDetailViewItem metadataSchema);

        Task CreateAsync(MetadataSchemaDetailViewItem metadataSchema, bool enableForBinaryFiles);

        void Create(MetadataSchemaDetailViewItem metadataSchema, bool enableForBinaryFiles);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task CreateAsync(MetadataSchemaDetailViewItem metadataSchema);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        void Create(MetadataSchemaDetailViewItem metadataSchema);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task DeleteAsync(string schemaId);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        void Delete(string schemaId);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task UpdateAsync(MetadataSchemaDetailViewItem metadataSchema);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task UpdateAsync(string schemaId, MetadataSchemaUpdateRequest updateRequest);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        void Update(string schemaId, MetadataSchemaUpdateRequest updateRequest);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<bool> ExistsAsync(string schemaId);

        bool Exists(string schemaId);
    }
}