using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Conversion;
using System.Net.Http;
using System.Threading;

namespace Picturepark.SDK.V1
{
	public partial class SchemaClient
	{
		private readonly BusinessProcessClient _businessProcessClient;

		public SchemaClient(BusinessProcessClient businessProcessesClient, IPictureparkClientSettings settings, HttpClient httpClient)
			: this(settings, httpClient)
		{
			_businessProcessClient = businessProcessesClient;
		}

		/// <summary>Generates the <see cref="SchemaDetail"/>s for the given type and the referenced types.</summary>
		/// <param name="type">The type.</param>
		/// <param name="schemaDetails">The existing schema details.</param>
		/// <param name="generateDependencySchema">Specifies whether to generate dependent schemas.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The collection of schema details.</returns>
		public async Task<ICollection<SchemaDetail>> GenerateSchemasAsync(Type type, IEnumerable<SchemaDetail> schemaDetails = null, bool generateDependencySchema = true, CancellationToken cancellationToken = default(CancellationToken))
		{
			var schemaConverter = new ClassToSchemaConverter();
			return await schemaConverter.GenerateAsync(type, schemaDetails ?? new List<SchemaDetail>(), generateDependencySchema).ConfigureAwait(false);
		}

		/// <summary>Creates or updates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		public void CreateOrUpdateAndWaitForCompletion(SchemaDetail schemaDetail, bool enableForBinaryFiles)
		{
			Task.Run(async () => await CreateOrUpdateAndWaitForCompletionAsync(schemaDetail, enableForBinaryFiles).ConfigureAwait(false)).GetAwaiter().GetResult();
		}

		/// <summary>Creates or updates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The task.</returns>
		public async Task CreateOrUpdateAndWaitForCompletionAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (await ExistsAsync(schemaDetail.Id, null, cancellationToken).ConfigureAwait(false))
			{
				await UpdateAndWaitForCompletionAsync(schemaDetail, enableForBinaryFiles, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await CreateAndWaitForCompletionAsync(schemaDetail, enableForBinaryFiles, cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		public void CreateAndWaitForCompletion(SchemaDetail schemaDetail, bool enableForBinaryFiles)
		{
			Task.Run(async () => await CreateAndWaitForCompletionAsync(schemaDetail, enableForBinaryFiles).ConfigureAwait(false)).GetAwaiter().GetResult();
		}

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The task.</returns>
		public async Task CreateAndWaitForCompletionAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, CancellationToken cancellationToken = default(CancellationToken))
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

			await CreateAndWaitForCompletionAsync(schemaDetail, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public void CreateAndWaitForCompletion(SchemaDetail schemaDetail)
		{
			Task.Run(async () => await CreateAndWaitForCompletionAsync(schemaDetail).ConfigureAwait(false)).GetAwaiter().GetResult();
		}

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task CreateAndWaitForCompletionAsync(SchemaDetail schemaDetail, CancellationToken cancellationToken = default(CancellationToken))
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
				Public = schemaDetail.Public,
				ReferencedInContentSchemaIds = schemaDetail.ReferencedInContentSchemaIds,
				Sort = schemaDetail.Sort,
				SortOrder = schemaDetail.SortOrder,
				Types = schemaDetail.Types,
				LayerSchemaIds = schemaDetail.LayerSchemaIds
			};

			var businessProcess = await CreateAsync(createRequest, cancellationToken).ConfigureAwait(false);
			await _businessProcessClient.WaitForCompletionAsync(businessProcess.Id, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>Deletes the a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public void DeleteAndWaitForCompletion(string schemaId)
		{
			Task.Run(async () => await DeleteAndWaitForCompletionAsync(schemaId).ConfigureAwait(false)).GetAwaiter().GetResult();
		}

		/// <summary>Deletes the a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task DeleteAndWaitForCompletionAsync(string schemaId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var process = await DeleteAsync(schemaId, cancellationToken).ConfigureAwait(false);
			await _businessProcessClient.WaitForCompletionAsync(process.Id, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>Updates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task UpdateAndWaitForCompletionAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, CancellationToken cancellationToken = default(CancellationToken))
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

			var updateRequest = new SchemaUpdateRequest
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
			};

			await UpdateAndWaitForCompletionAsync(schemaDetail.Id, updateRequest, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>Updates a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <param name="updateRequest">The update request.</param>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public void UpdateAndWaitForCompletion(string schemaId, SchemaUpdateRequest updateRequest)
		{
			Task.Run(async () => await UpdateAndWaitForCompletionAsync(schemaId, updateRequest).ConfigureAwait(false)).GetAwaiter().GetResult();
		}

		/// <summary>Updates a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <param name="updateRequest">The update request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task UpdateAndWaitForCompletionAsync(string schemaId, SchemaUpdateRequest updateRequest, CancellationToken cancellationToken = default(CancellationToken))
		{
			var process = await UpdateAsync(schemaId, updateRequest, cancellationToken).ConfigureAwait(false);
			await _businessProcessClient.WaitForCompletionAsync(process.Id, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>Checks whether a schema ID already exists.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <param name="fieldId">The optional field ID.</param>
		public bool Exists(string schemaId, string fieldId = null)
		{
			return Task.Run(async () => await ExistsCoreAsync(schemaId, fieldId).ConfigureAwait(false)).GetAwaiter().GetResult().Exists;
		}

		/// <summary>Checks whether a schema ID already exists.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <param name="fieldId">The optional field ID.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task<bool> ExistsAsync(string schemaId, string fieldId = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return (await ExistsCoreAsync(schemaId, null, cancellationToken).ConfigureAwait(false)).Exists;
		}
	}
}
