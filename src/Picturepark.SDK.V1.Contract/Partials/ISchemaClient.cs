using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface ISchemaClient
	{
		/// <summary>Generates the <see cref="SchemaDetail"/>s for the given type and the referenced types.</summary>
		/// <param name="type">The type.</param>
		/// <param name="schemaDetails">The existing schema details.</param>
		/// <param name="generateDependencySchema">Specifies whether to generate dependent schemas.</param>
		/// <returns>The collection of schema details.</returns>
		Task<ICollection<SchemaDetail>> GenerateSchemasAsync(Type type, IEnumerable<SchemaDetail> schemaDetails = null, bool generateDependencySchema = true);

		/// <summary>Creates or updates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		void CreateOrUpdate(SchemaDetail schemaDetail, bool enableForBinaryFiles);

		/// <summary>Creates or updates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		/// <returns>The task.</returns>
		Task CreateOrUpdateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles);

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		void Create(SchemaDetail schemaDetail, bool enableForBinaryFiles);

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		/// <returns>The task.</returns>
		Task CreateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles);

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		void Create(SchemaDetail schemaDetail);

		/// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task CreateAsync(SchemaDetail schemaDetail);

		/// <summary>Deletes the a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		void Delete(string schemaId);

		/// <summary>Deletes the a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task DeleteAsync(string schemaId);

		/// <summary>Updates the given <see cref="SchemaDetail"/>.</summary>
		/// <param name="schemaDetail">The schema detail.</param>
		/// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task UpdateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles);

		/// <summary>Updates a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <param name="updateRequest">The update request.</param>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		void Update(string schemaId, SchemaUpdateRequest updateRequest);

		/// <summary>Updates a schema.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <param name="updateRequest">The update request.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task UpdateAsync(string schemaId, SchemaUpdateRequest updateRequest);

		/// <summary>Checks whether a schema ID already exists.</summary>
		/// <param name="schemaId">The schema ID.</param>
		bool Exists(string schemaId);

		/// <summary>Checks whether a schema ID already exists.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <returns>The task.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task<bool> ExistsAsync(string schemaId);
	}
}