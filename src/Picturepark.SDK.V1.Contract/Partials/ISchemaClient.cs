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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task CreateOrUpdateAndWaitForCompletionAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task CreateAndWaitForCompletionAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Creates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task CreateAndWaitForCompletionAsync(SchemaDetail schemaDetail, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Deletes the a schema.</summary>
        /// <param name="schemaId">The schema ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task DeleteAndWaitForCompletionAsync(string schemaId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Updates the given <see cref="SchemaDetail"/>.</summary>
        /// <param name="schemaDetail">The schema detail.</param>
        /// <param name="enableForBinaryFiles">Specifies whether to enable the schema for binary files.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task UpdateAndWaitForCompletionAsync(SchemaDetail schemaDetail, bool enableForBinaryFiles, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Updates a schema.</summary>
        /// <param name="schemaId">The schema ID.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task UpdateAndWaitForCompletionAsync(string schemaId, SchemaUpdateRequest updateRequest, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Checks whether a schema ID already exists.</summary>
        /// <param name="schemaId">The schema ID.</param>
        /// <param name="fieldId">The optional field ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<bool> ExistsAsync(string schemaId, string fieldId = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}