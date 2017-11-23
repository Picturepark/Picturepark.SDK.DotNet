using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Conversion;
using System.Net.Http;

namespace Picturepark.SDK.V1
{
	public partial class SchemaClient
	{
		private readonly BusinessProcessClient _businessProcessClient;

		public SchemaClient(BusinessProcessClient businessProcessesClient, IPictureparkClientSettings settings, HttpClient httpClient)
			: this(settings, httpClient)
		{
			BaseUrl = businessProcessesClient.BaseUrl;
			_businessProcessClient = businessProcessesClient;
		}

		/// <summary>Generates the <see cref="SchemaDetail"/>s for the given type and the referenced types.</summary>
		/// <param name="type">The type.</param>
		/// <param name="schemaDetails">The existing schema details.</param>
		/// <param name="generateDependencySchema">Specifies whether to generate dependent schemas.</param>
		/// <returns>The collection of schema details.</returns>
		public async Task<ICollection<SchemaDetail>> GenerateSchemasAsync(Type type, IEnumerable<SchemaDetail> schemaDetails = null, bool generateDependencySchema = true)
		{
			var schemaConverter = new ClassToSchemaConverter();
			return await schemaConverter.GenerateAsync(type, schemaDetails ?? new List<SchemaDetail>(), generateDependencySchema).ConfigureAwait(false);
		}

		/// <summary>Creates or updates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		public void CreateOrUpdate(SchemaDetail schemaDetail, bool enableForBinaryFiles)
		{
			Task.Run(async () => await CreateOrUpdateAsync(schemaDetail, enableForBinaryFiles)).GetAwaiter().GetResult();
		}

		/// <summary>Creates or updates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		/// <returns>The task.</returns>
		public async Task CreateOrUpdateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles)
		{
			if (await ExistsAsync(schemaDetail.Id))
			{
				await UpdateAsync(schemaDetail, enableForBinaryFiles).ConfigureAwait(false);
			}
			else
			{
				await CreateAsync(schemaDetail, enableForBinaryFiles).ConfigureAwait(false);
			}
		}

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		public void Create(SchemaDetail schemaDetail, bool enableForBinaryFiles)
		{
			Task.Run(async () => await CreateAsync(schemaDetail, enableForBinaryFiles)).GetAwaiter().GetResult();
		}

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		/// <returns>The task.</returns>
		public async Task CreateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles)
		{
			// Map schema to binary schemas
			if (enableForBinaryFiles && schemaDetail.Types.Contains(SchemaType.Layer))
			{
				var binarySchemas = new List<string>
				{
					nameof(FileMetadata),
					nameof(AudioMetadata),
					nameof(DocumentMetadata),
					nameof(ImageMetadata),
					nameof(VideoMetadata),
				};

				schemaDetail.ReferencedInContentSchemaIds = binarySchemas;
			}

			await CreateAsync(schemaDetail).ConfigureAwait(false);
		}

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task CreateAsync(SchemaDetail schemaDetail)
		{
			var businessProcess = await CreateAsync(new SchemaCreateRequest
			{
				Aggregations = schemaDetail.Aggregations,
				Descriptions = schemaDetail.Descriptions,
				DisplayPatterns = schemaDetail.DisplayPatterns,
				Fields = schemaDetail.Fields,
				Id = schemaDetail.Id,
				SchemaPermissionSetIds = schemaDetail.SchemaPermissionSetIds,
				Names = schemaDetail.Names,
				ParentSchemaId = schemaDetail.ParentSchemaId,
				Public = schemaDetail.Public,
				ReferencedInContentSchemaIds = schemaDetail.ReferencedInContentSchemaIds,
				Sort = schemaDetail.Sort,
				SortOrder = schemaDetail.SortOrder,
				Types = schemaDetail.Types,
				LayerSchemaIds = schemaDetail.LayerSchemaIds
			}).ConfigureAwait(false);

			await _businessProcessClient.WaitForCompletionAsync(businessProcess.Id).ConfigureAwait(false);
		}

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public void Create(SchemaDetail schemaDetail)
		{
			Task.Run(async () => await CreateAsync(schemaDetail)).GetAwaiter().GetResult();
		}

		/// <summary>Deletes the a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public void Delete(string schemaId)
		{
			Task.Run(async () => await DeleteAsync(schemaId)).GetAwaiter().GetResult();
		}

		/// <summary>Deletes the a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task DeleteAsync(string schemaId)
		{
			var process = await DeleteCoreAsync(schemaId).ConfigureAwait(false);
			await _businessProcessClient.WaitForCompletionAsync(process.Id).ConfigureAwait(false);
		}

		/// <summary>Updates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task UpdateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles)
		{
			if (enableForBinaryFiles && schemaDetail.Types.Contains(SchemaType.Layer))
			{
				var binarySchemas = new List<string>
				{
					nameof(FileMetadata),
					nameof(AudioMetadata),
					nameof(DocumentMetadata),
					nameof(ImageMetadata),
					nameof(VideoMetadata),
				};

				schemaDetail.ReferencedInContentSchemaIds = binarySchemas;
			}

			await UpdateAsync(schemaDetail.Id, new SchemaUpdateRequest
			{
				Aggregations = schemaDetail.Aggregations,
				Descriptions = schemaDetail.Descriptions,
				DisplayPatterns = schemaDetail.DisplayPatterns,
				Fields = schemaDetail.Fields,
				SchemaPermissionSetIds = schemaDetail.SchemaPermissionSetIds,
				Names = schemaDetail.Names,
				Public = schemaDetail.Public,
				ReferencedInContentSchemaIds = schemaDetail.ReferencedInContentSchemaIds,
				Sort = schemaDetail.Sort,
				SortOrder = schemaDetail.SortOrder,
				Types = schemaDetail.Types,
				LayerSchemaIds = schemaDetail.LayerSchemaIds
			}).ConfigureAwait(false);
		}

		/// <summary>Updates a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <param name="updateRequest">The update request.</param>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public void Update(string schemaId, SchemaUpdateRequest updateRequest)
		{
			Task.Run(async () => await UpdateAsync(schemaId, updateRequest)).GetAwaiter().GetResult();
		}

		/// <summary>Updates a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <param name="updateRequest">The update request.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task UpdateAsync(string schemaId, SchemaUpdateRequest updateRequest)
		{
			var process = await UpdateCoreAsync(schemaId, updateRequest).ConfigureAwait(false);
			await _businessProcessClient.WaitForCompletionAsync(process.Id).ConfigureAwait(false);
		}

		/// <summary>Checks whether a schema ID already exists.</summary>
		/// <param name="schemaId">The schema ID.</param>
		public bool Exists(string schemaId)
		{
			return Task.Run(async () => await ExistsAsync(schemaId)).GetAwaiter().GetResult();
		}

		/// <summary>Checks whether a schema ID already exists.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task<bool> ExistsAsync(string schemaId)
		{
			return (await ExistsAsync(schemaId, null).ConfigureAwait(false)).Exists;
		}
	}
}
