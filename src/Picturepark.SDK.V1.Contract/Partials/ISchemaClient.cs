using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface ISchemaClient
	{
		List<SchemaDetail> GenerateSchemaFromPOCO(Type type, List<SchemaDetail> schemaList = null, bool generateDependencySchema = true);

		void CreateOrUpdate(SchemaDetail schema, bool enableForBinaryFiles);

		Task CreateOrUpdateAsync(SchemaDetail schema, bool enableForBinaryFiles);

		void Create(SchemaDetail schema, bool enableForBinaryFiles);

		Task CreateAsync(SchemaDetail schema, bool enableForBinaryFiles);

		/// <exception cref="ApiException">A server side error occurred.</exception>
		void Create(SchemaDetail schema);

		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task CreateAsync(SchemaDetail schema);

		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task DeleteAsync(string schemaId);

		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task UpdateAsync(SchemaDetail schema, bool enableForBinaryFiles);

		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task UpdateAsync(string schemaId, SchemaUpdateRequest updateRequest);

		bool Exists(string schemaId);

		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task<bool> ExistsAsync(string schemaId);
	}
}