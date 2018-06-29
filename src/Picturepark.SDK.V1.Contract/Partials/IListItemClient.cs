using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IListItemClient
    {
        /// <summary>Creates a <see cref="ListItem"/>s based on an object and its references.</summary>
        /// <param name="content">The object to create <see cref="ListItem"/>s from.</param>
        /// <param name="schemaId">The schema ID of the object.</param>
        /// <param name="allowMissingDependencies">Allow creating <see cref="ListItem"/>s that refer to list items or contents that don't exist in the system.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created <see cref="ListItem"/>s.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<IEnumerable<ListItem>> CreateFromObjectAsync(object content, string schemaId, bool allowMissingDependencies = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Creates multiple <see cref="ListItem"/>s.</summary>
        /// <param name="createManyRequest">The create many request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created <see cref="ListItem"/>s.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">The business process has not been completed.</exception>
        Task<IEnumerable<ListItem>> CreateManyAsync(ListItemCreateManyRequest createManyRequest, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Gets an existing list item and converts its content to the requested type.</summary>
        /// <typeparam name="T">The requested content type.</typeparam>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="schemaId">The schema ID of the requested type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<T> GetAndConvertToAsync<T>(string listItemId, string schemaId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Updates a list item by providing its content.</summary>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="content">The content which must match the item's schema ID.</param>
        /// <param name="resolveBehaviours">List of enum that control which parts of the list item are resolved and returned.</param>
        /// <param name="allowMissingDependencies">Allow creating <see cref="ListItem"/>s that refer to list items or contents that don't exist in the system.</param>
        /// <param name="timeout">The timeout in milliseconds to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated <see cref="ListItemDetail"/>.</returns>
        Task<ListItemDetail> UpdateAsync(string listItemId, object content, IEnumerable<ListItemResolveBehaviour> resolveBehaviours = null, bool allowMissingDependencies = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Updates a list item.</summary>
        /// <param name="updateRequest">The update request.</param>
        /// <param name="resolveBehaviours">List of enum that control which parts of the list item are resolved and returned.</param>
        /// <param name="allowMissingDependencies">Allow creating <see cref="ListItem"/>s that refer to list items or contents that don't exist in the system.</param>
        /// <param name="timeout">The timeout in milliseconds to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated <see cref="ListItemDetail"/>.</returns>
        Task<ListItemDetail> UpdateAsync(ListItemUpdateRequest updateRequest, IEnumerable<ListItemResolveBehaviour> resolveBehaviours = null, bool allowMissingDependencies = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}