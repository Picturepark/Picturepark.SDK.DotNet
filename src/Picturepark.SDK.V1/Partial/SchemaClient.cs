using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Conversion;
using System.Net.Http;
using System.Threading;
using Picturepark.SDK.V1.Contract.Results;

namespace Picturepark.SDK.V1
{
    public partial class SchemaClient
    {
        private readonly IInfoClient _infoClient;
        private readonly IBusinessProcessClient _businessProcessClient;

        public SchemaClient(IInfoClient infoClient, IBusinessProcessClient businessProcessClient, IPictureparkServiceSettings settings, HttpClient httpClient)
            : this(settings, httpClient)
        {
            _infoClient = infoClient;
            _businessProcessClient = businessProcessClient;
        }

        /// <summary>Generates the <see cref="SchemaDetail"/>s for the given type and the referenced types.</summary>
        /// <remarks>Note: When generating multiple schemas or schemas for types with dependencies, please use the CreateMany method to create them at the server to avoid dependency problems.</remarks>
        /// <param name="type">The type.</param>
        /// <param name="schemaDetails">The existing schema details.</param>
        /// <param name="generateDependencySchema">Specifies whether to generate dependent schemas.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection of schema details.</returns>
        public async Task<ICollection<SchemaDetail>> GenerateSchemasAsync(
            Type type,
            IEnumerable<SchemaDetail> schemaDetails = null,
            bool generateDependencySchema = true,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var config = await _infoClient.GetInfoAsync(cancellationToken).ConfigureAwait(false);
            var schemaConverter = new ClassToSchemaConverter(config.LanguageConfiguration.DefaultLanguage);
            return await schemaConverter.GenerateAsync(type, schemaDetails ?? new List<SchemaDetail>(), generateDependencySchema).ConfigureAwait(false);
        }

        /// <summary>Creates or updates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
        /// <param name="timeout">Maximum time to wait for the operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        public async Task<ISchemaResult> CreateOrUpdateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await ExistsAsync(schemaDetail.Id, cancellationToken).ConfigureAwait(false))
            {
                return await UpdateAsync(schemaDetail, enableForBinaryFiles, timeout, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await CreateAsync(schemaDetail, enableForBinaryFiles, timeout, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
        /// <param name="timeout">Maximum time to wait for the operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        public async Task<SchemaCreateResult> CreateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Map schema to binary schemas
            if (enableForBinaryFiles)
                EnableForBinaryFiles(schemaDetail);

            return await CreateAsync(schemaDetail, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<SchemaBatchOperationResult> CreateManyAsync(IEnumerable<SchemaDetail> schemaDetails, bool enableForBinaryFiles, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!schemaDetails.Any())
            {
                return SchemaBatchOperationResult.Empty;
            }

            var request = new SchemaCreateManyRequest();

            // Map schema to binary schemas
            foreach (var schemaDetail in schemaDetails)
            {
                if (enableForBinaryFiles)
                    EnableForBinaryFiles(schemaDetail);

                var createRequest = MapSchemaDetailToCreateRequest(schemaDetail);
                request.Schemas.Add(createRequest);
            }

            return await CreateManyAsync(request, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<SchemaBatchOperationResult> CreateManyAsync(SchemaCreateManyRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await CreateManyCoreAsync(request, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="timeout">Maximum time to wait for the operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<SchemaCreateResult> CreateAsync(SchemaDetail schemaDetail, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var createRequest = MapSchemaDetailToCreateRequest(schemaDetail);

            return await CreateAsync(createRequest, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Updates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
        /// <param name="timeout">Maximum time to wait for the operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<SchemaUpdateResult> UpdateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (enableForBinaryFiles)
                EnableForBinaryFiles(schemaDetail);

            var updateRequest = MapSchemaDetailToUpdateRequest(schemaDetail);
            return await UpdateAsync(schemaDetail.Id, updateRequest, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<SchemaBatchOperationResult> UpdateManyAsync(SchemaUpdateManyRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await UpdateManyCoreAsync(request, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<SchemaBatchOperationResult> UpdateManyAsync(
            IEnumerable<SchemaDetail> schemaDetails,
            bool enableForBinaryFiles,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!schemaDetails.Any())
            {
                return SchemaBatchOperationResult.Empty;
            }

            var request = new SchemaUpdateManyRequest();

            // Map schema to binary schemas
            foreach (var schemaDetail in schemaDetails)
            {
                if (enableForBinaryFiles)
                    EnableForBinaryFiles(schemaDetail);

                var updateRequest = MapSchemaDetailToUpdateRequest(schemaDetail);
                request.Schemas.Add(updateRequest);
            }

            return await UpdateManyAsync(request, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Checks whether a schema ID already exists.</summary>
        /// <param name="schemaId">The schema ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<bool> ExistsAsync(string schemaId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return (await ExistsCoreAsync(schemaId, cancellationToken).ConfigureAwait(false)).Exists;
        }

        /// <inheritdoc />
        public async Task<SchemaBatchOperationResult> WaitForBusinessProcessAndReturnResult(string businessProcessId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _businessProcessClient.WaitForCompletionAsync(businessProcessId, timeout, cancellationToken: cancellationToken).ConfigureAwait(false);

            return new SchemaBatchOperationResult(this, businessProcessId, result.LifeCycleHit, _businessProcessClient);
        }

        private SchemaCreateRequest MapSchemaDetailToCreateRequest(SchemaDetail schemaDetail)
        {
            var createRequest = new SchemaCreateRequest
            {
                Aggregations = schemaDetail.Aggregations,
                Descriptions = schemaDetail.Descriptions,
                DisplayPatterns = schemaDetail.DisplayPatterns,
                Fields = schemaDetail.Fields,
                Id = schemaDetail.Id,
                SchemaPermissionSetIds = schemaDetail.SchemaPermissionSetIds,
                Names = schemaDetail.Names,
                ParentSchemaId = schemaDetail.ParentSchemaId,
                ViewForAll = schemaDetail.ViewForAll,
                ReferencedInContentSchemaIds = schemaDetail.ReferencedInContentSchemaIds,
                Sort = schemaDetail.Sort,
                Types = schemaDetail.Types,
                LayerSchemaIds = schemaDetail.LayerSchemaIds
            };

            return createRequest;
        }

        private SchemaUpdateItem MapSchemaDetailToUpdateRequest(SchemaDetail schemaDetail)
        {
            var updateRequest = new SchemaUpdateItem
            {
                Id = schemaDetail.Id,
                Aggregations = schemaDetail.Aggregations,
                Descriptions = schemaDetail.Descriptions,
                DisplayPatterns = schemaDetail.DisplayPatterns,
                Fields = schemaDetail.Fields,
                SchemaPermissionSetIds = schemaDetail.SchemaPermissionSetIds,
                Names = schemaDetail.Names,
                ViewForAll = schemaDetail.ViewForAll,
                ReferencedInContentSchemaIds = schemaDetail.ReferencedInContentSchemaIds,
                Sort = schemaDetail.Sort,
                LayerSchemaIds = schemaDetail.LayerSchemaIds,
                FieldsOverwrite = schemaDetail.FieldsOverwrite
            };

            return updateRequest;
        }

        private void EnableForBinaryFiles(SchemaDetail schemaDetail)
        {
            if (schemaDetail.Types.Contains(SchemaType.Layer))
            {
                var binarySchemas = new List<string>
                {
                    nameof(FileMetadata),
                    nameof(AudioMetadata),
                    nameof(DocumentMetadata),
                    nameof(ImageMetadata),
                    nameof(VideoMetadata),
                    nameof(VectorMetadata)
                };

                schemaDetail.ReferencedInContentSchemaIds = binarySchemas;
            }
        }
    }
}
