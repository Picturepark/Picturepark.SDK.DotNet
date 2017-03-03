using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Authentication;
using Picturepark.SDK.V1.Contract.Extensions;
using Picturepark.SDK.V1.Conversion;

namespace Picturepark.SDK.V1
{
    public partial class SchemaClient
    {
        private readonly BusinessProcessClient _businessProcessClient;

        public SchemaClient(BusinessProcessClient businessProcessesClient, IAuthClient authClient)
            : base(authClient)
        {
            BaseUrl = businessProcessesClient.BaseUrl;
            _businessProcessClient = businessProcessesClient;
        }

        public List<SchemaDetailViewItem> GenerateSchemaFromPOCO(Type type, List<SchemaDetailViewItem> schemaList, bool generateDependencySchema = true)
        {
            var schemaConverter = new ClassToSchemaConverter();
            return schemaConverter.Generate(type, schemaList, generateDependencySchema);
        }

        public async Task CreateOrUpdateAsync(SchemaDetailViewItem metadataSchema)
        {
            if (await ExistsAsync(metadataSchema.Id))
            {
                await UpdateAsync(metadataSchema);
            }
            else
            {
                await CreateAsync(metadataSchema);
            }
        }

        public void CreateOrUpdate(SchemaDetailViewItem metadataSchema)
        {
            Task.Run(async () => await CreateOrUpdateAsync(metadataSchema)).GetAwaiter().GetResult();
        }

        public async Task CreateAsync(SchemaDetailViewItem metadataSchema, bool enableForBinaryFiles)
        {
            // Map schema to binary schemas
            if (enableForBinaryFiles && metadataSchema.Types.Contains(SchemaType.Layer))
            {
                var binarySchemas = new List<string>
                {
                    nameof(FileMetadata),
                    nameof(AudioMetadata),
                    nameof(DocumentMetadata),
                    nameof(ImageMetadata),
                    nameof(VideoMetadata),
                };
                metadataSchema.ReferencedInSchemaIds = binarySchemas;
            }

            await CreateAsync(metadataSchema);
        }

        public void Create(SchemaDetailViewItem metadataSchema, bool enableForBinaryFiles)
        {
            Task.Run(async () => await CreateAsync(metadataSchema, enableForBinaryFiles)).GetAwaiter().GetResult();
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task CreateAsync(SchemaDetailViewItem metadataSchema)
        {
            var process = await CreateAsync(new SchemaCreateRequest
            {
                Aggregations = metadataSchema.Aggregations,
                Descriptions = metadataSchema.Descriptions,
                DisplayPatterns = metadataSchema.DisplayPatterns,
                Fields = metadataSchema.Fields,
                FullTextFields = metadataSchema.FullTextFields,
                Id = metadataSchema.Id,
                MetadataPermissionSetIds = metadataSchema.MetadataPermissionSetIds,
                Names = metadataSchema.Names,
                ParentSchemaId = metadataSchema.ParentSchemaId,
                Public = metadataSchema.Public,
                ReferencedInSchemaIds = metadataSchema.ReferencedInSchemaIds,
                Sort = metadataSchema.Sort,
                SortOrder = metadataSchema.SortOrder,
                Types = metadataSchema.Types
            });
            await WaitForCompletionAsync(process);
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void Create(SchemaDetailViewItem metadataSchema)
        {
            Task.Run(async () => await CreateAsync(metadataSchema)).GetAwaiter().GetResult();
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task DeleteAsync(string schemaId)
        {
            var process = await DeleteCoreAsync(schemaId);
            await WaitForCompletionAsync(process);
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void Delete(string schemaId)
        {
            Task.Run(async () => await DeleteAsync(schemaId)).GetAwaiter().GetResult();
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task UpdateAsync(SchemaDetailViewItem metadataSchema)
        {
            await UpdateAsync(metadataSchema.Id, new SchemaUpdateRequest
            {
                Aggregations = metadataSchema.Aggregations,
                Descriptions = new TranslatedStringDictionary { }, //// TODO
                DisplayPatterns = metadataSchema.DisplayPatterns,
                Fields = metadataSchema.Fields,
                FullTextFields = metadataSchema.FullTextFields,
                MetadataPermissionSetIds = metadataSchema.MetadataPermissionSetIds,
                Names = metadataSchema.Names,
                Public = metadataSchema.Public,
                ReferencedInSchemaIds = metadataSchema.ReferencedInSchemaIds,
                Sort = metadataSchema.Sort,
                SortOrder = metadataSchema.SortOrder,
                Types = metadataSchema.Types,
                SchemaIds = metadataSchema.SchemaIds
            });
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task UpdateAsync(string schemaId, SchemaUpdateRequest updateRequest)
        {
            var process = await UpdateCoreAsync(schemaId, updateRequest);
            await WaitForCompletionAsync(process);
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void Update(string schemaId, SchemaUpdateRequest updateRequest)
        {
            Task.Run(async () => await UpdateAsync(schemaId, updateRequest)).GetAwaiter().GetResult();
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<bool> ExistsAsync(string schemaId)
        {
            return (await ExistsAsync(schemaId, null)).Exists;
        }

        public bool Exists(string schemaId)
        {
            return Task.Run(async () => await ExistsAsync(schemaId)).GetAwaiter().GetResult();
        }

        private async Task WaitForCompletionAsync(BusinessProcessViewItem process)
        {
            var wait = await process.Wait4StateAsync("Completed", _businessProcessClient);

            var errors = wait.BusinessProcess.StateHistory?
                .Where(i => i.Error != null)
                .Select(i => i.Error)
                .ToList();

            if (errors != null && errors.Any())
            {
                // TODO: Deserialize and create Aggregate exception
                throw new Exception(errors.First().Exception);
            }
        }
    }
}
