using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface ISchemaClient
    {
        /// <summary>Generates the <see cref="SchemaDetail"/>s for the given type and the referenced types.</summary>
        /// <param name="type">The type.</param>
        /// <param name="schemaDetails">The existing schema details.</param>
        /// <param name="generateDependencySchema">Specifies whether to generate dependent schemas.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection of schema details.</returns>
        Task<ICollection<SchemaDetail>> GenerateSchemasAsync(Type type, IEnumerable<SchemaDetail> schemaDetails = null, bool generateDependencySchema = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Creates or updates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
        /// <param name="timeout">Maximum time to wait for the operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task<ISchemaResult> CreateOrUpdateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
        /// <param name="timeout">Maximum time to wait for the operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task<SchemaCreateResult> CreateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Create the given enumerable of <see cref="SchemaDetail"/>.
        /// </summary>
        /// <param name="schemaDetails">The schema details.</param>
        /// <param name="enableForBinaryFiles">Specifies whether to enable the schemas for binary files.</param>
        /// <param name="timeout">Maximum time to wait for the operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection of created schemas.</returns>
        Task<ICollection<SchemaDetail>> CreateManyAsync(IEnumerable<SchemaDetail> schemaDetails, bool enableForBinaryFiles, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="timeout">Maximum time to wait for the operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<SchemaCreateResult> CreateAsync(SchemaDetail schemaDetail, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Updates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
        /// <param name="timeout">Maximum time to wait for the operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<SchemaUpdateResult> UpdateAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Checks whether a schema ID already exists.</summary>
        /// <param name="schemaId">The schema ID.</param>
        /// <param name="fieldId">The optional field ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<bool> ExistsAsync(string schemaId, string fieldId = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}