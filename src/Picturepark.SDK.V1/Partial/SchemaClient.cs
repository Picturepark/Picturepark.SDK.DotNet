using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Extensions;
using Picturepark.SDK.V1.Conversion;

namespace Picturepark.SDK.V1
{
	public partial class SchemaClient
	{
		private readonly BusinessProcessClient _businessProcessClient;

		public SchemaClient(BusinessProcessClient businessProcessesClient, IPictureparkClientSettings settings) : this(settings)
		{
			BaseUrl = businessProcessesClient.BaseUrl;
			_businessProcessClient = businessProcessesClient;
		}

		public List<SchemaDetailViewItem> GenerateSchemaFromPOCO(Type type, List<SchemaDetailViewItem> schemaList, bool generateDependencySchema = true)
		{
			var schemaConverter = new ClassToSchemaConverter();
			return schemaConverter.Generate(type, schemaList, generateDependencySchema);
		}

		public async Task CreateOrUpdateAsync(SchemaDetailViewItem metadataSchema, bool enableForBinaryFiles)
		{
			if (await ExistsAsync(metadataSchema.Id))
			{
				await UpdateAsync(metadataSchema, enableForBinaryFiles);
			}
			else
			{
				await CreateAsync(metadataSchema, enableForBinaryFiles);
			}
		}

		public void CreateOrUpdate(SchemaDetailViewItem metadataSchema, bool enableForBinaryFiles)
		{
			Task.Run(async () => await CreateOrUpdateAsync(metadataSchema, enableForBinaryFiles)).GetAwaiter().GetResult();
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
				metadataSchema.ReferencedInContentSchemaIds = binarySchemas;
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
				SchemaPermissionSetIds = metadataSchema.SchemaPermissionSetIds,
				Names = metadataSchema.Names,
				ParentSchemaId = metadataSchema.ParentSchemaId,
				Public = metadataSchema.Public,
				ReferencedInContentSchemaIds = metadataSchema.ReferencedInContentSchemaIds,
				Sort = metadataSchema.Sort,
				SortOrder = metadataSchema.SortOrder,
				Types = metadataSchema.Types,
				LayerSchemaIds = metadataSchema.LayerSchemaIds
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
		public async Task UpdateAsync(SchemaDetailViewItem schema, bool enableForBinaryFiles)
		{
			if (enableForBinaryFiles && schema.Types.Contains(SchemaType.Layer))
			{
				var binarySchemas = new List<string>
				{
					nameof(FileMetadata),
					nameof(AudioMetadata),
					nameof(DocumentMetadata),
					nameof(ImageMetadata),
					nameof(VideoMetadata),
				};
				schema.ReferencedInContentSchemaIds = binarySchemas;
			}

			await UpdateAsync(schema.Id, new SchemaUpdateRequest
			{
				Aggregations = schema.Aggregations,
				Descriptions = schema.Descriptions,
				DisplayPatterns = schema.DisplayPatterns,
				Fields = schema.Fields,
				FullTextFields = schema.FullTextFields,
				SchemaPermissionSetIds = schema.SchemaPermissionSetIds,
				Names = schema.Names,
				Public = schema.Public,
				ReferencedInContentSchemaIds = schema.ReferencedInContentSchemaIds,
				Sort = schema.Sort,
				SortOrder = schema.SortOrder,
				Types = schema.Types,
				LayerSchemaIds = schema.LayerSchemaIds
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
