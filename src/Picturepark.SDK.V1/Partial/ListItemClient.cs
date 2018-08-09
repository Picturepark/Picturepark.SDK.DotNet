using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using System.Net.Http;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Results;
using Picturepark.SDK.V1.Conversion;

namespace Picturepark.SDK.V1
{
    public partial class ListItemClient
    {
        private readonly IBusinessProcessClient _businessProcessClient;

        public ListItemClient(IBusinessProcessClient businessProcessClient, IPictureparkClientSettings settings, HttpClient httpClient)
            : this(settings, httpClient)
        {
            _businessProcessClient = businessProcessClient;
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationResult> CreateFromObjectAsync(object content, bool allowMissingDependencies = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var createManyRequest = new ListItemCreateManyRequest
            {
                Items = new List<ListItemCreateRequest>(),
                AllowMissingDependencies = allowMissingDependencies
            };

            var referencedObjects = CreateReferencedObjectsAsync(content);
            foreach (var listItemCreateRequest in referencedObjects)
            {
                createManyRequest.Items.Add(listItemCreateRequest);
            }

            var schemaId = ClassToSchemaConverter.ResolveSchemaName(content.GetType());

            createManyRequest.Items.Add(new ListItemCreateRequest
            {
                ContentSchemaId = schemaId,
                Content = content
            });

            var objectResult = await CreateManyAsync(createManyRequest, timeout, cancellationToken).ConfigureAwait(false);
            return objectResult;
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationResult> CreateManyAsync(ListItemCreateManyRequest createManyRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!createManyRequest.Items.Any())
            {
                return ListItemBatchOperationResult.Empty;
            }

            var businessProcess = await CreateManyCoreAsync(createManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationResult> UpdateManyAsync(ListItemUpdateManyRequest listItemUpdateManyRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!listItemUpdateManyRequest.Items.Any())
            {
                return ListItemBatchOperationResult.Empty;
            }

            var businessProcess = await UpdateManyCoreAsync(listItemUpdateManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationResult> BatchUpdateFieldsByFilterAsync(ListItemFieldsBatchUpdateFilterRequest updateRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await BatchUpdateFieldsByFilterCoreAsync(updateRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationResult> BatchUpdateFieldsByIdsAsync(ListItemFieldsBatchUpdateRequest updateRequest, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await BatchUpdateFieldsByIdsCoreAsync(updateRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Gets an existing list item and converts its content to the requested type.</summary>
        /// <typeparam name="T">The requested content type.</typeparam>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="schemaId">The schema ID of the requested type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<T> GetAndConvertToAsync<T>(string listItemId, string schemaId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listItem = await GetAsync(listItemId, new ListItemResolveBehaviour[] { ListItemResolveBehaviour.Content, ListItemResolveBehaviour.LinkedListItems }, cancellationToken).ConfigureAwait(false);
            return listItem.ConvertTo<T>();
        }

        /// <summary>Gets a list of existing list items and converts their content to the requested type.</summary>
        /// <typeparam name="T">The requested content type.</typeparam>
        /// <param name="listItemIds">The list of list item IDs.</param>
        /// <param name="schemaId">The schema ID of the requested type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of converted objects.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<ICollection<T>> GetManyAndConvertToAsync<T>(IEnumerable<string> listItemIds, string schemaId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listItems = await GetManyAsync(listItemIds, new ListItemResolveBehaviour[] { ListItemResolveBehaviour.Content, ListItemResolveBehaviour.LinkedListItems }, cancellationToken).ConfigureAwait(false);
            return listItems.Select(li => li.ConvertTo<T>()).ToList();
        }

        /// <summary>Updates a list item by providing its content.</summary>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="content">The content which must match the item's schema ID.</param>
        /// <param name="resolveBehaviours">List of enum that control which parts of the list item are resolved and returned.</param>
        /// <param name="allowMissingDependencies">Allow creating <see cref="ListItem"/>s that refer to list items or contents that don't exist in the system.</param>
        /// <param name="timeout">The timeout in milliseconds to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated <see cref="ListItemDetail"/>.</returns>
        public async Task<ListItemDetail> UpdateAsync(string listItemId, object content, IEnumerable<ListItemResolveBehaviour> resolveBehaviours = null, bool allowMissingDependencies = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var updateRequest = new ListItemUpdateRequest()
            {
                Content = content
            };

            return await UpdateAsync(listItemId, updateRequest, resolveBehaviours, allowMissingDependencies, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Updates a list item.</summary>
        /// <param name="listItemId">ID of the list item.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <param name="resolveBehaviours">List of enum that control which parts of the list item are resolved and returned.</param>
        /// <param name="allowMissingDependencies">Allow creating <see cref="ListItem"/>s that refer to list items or contents that don't exist in the system.</param>
        /// <param name="timeout">The timeout in milliseconds to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated <see cref="ListItemDetail"/>.</returns>
        public async Task<ListItemDetail> UpdateAsync(string listItemId, ListItemUpdateRequest updateRequest, IEnumerable<ListItemResolveBehaviour> resolveBehaviours = null, bool allowMissingDependencies = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await UpdateCoreAsync(listItemId, updateRequest, resolveBehaviours, allowMissingDependencies, timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationResult> WaitForBusinessProcessAndReturnResult(string businessProcessId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _businessProcessClient.WaitForCompletionAsync(businessProcessId, timeout, cancellationToken).ConfigureAwait(false);

            return new ListItemBatchOperationResult(this, businessProcessId, result.LifeCycleHit, _businessProcessClient);
        }

        private bool IsSimpleType(Type type)
        {
            return
                type.GetTypeInfo().IsValueType ||
                type.GetTypeInfo().IsPrimitive ||
                new Type[]
                {
                    typeof(string),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        private IList<ListItemCreateRequest> CreateReferencedObjectsAsync(object obj)
        {
            var referencedListItems = new List<ListItemCreateRequest>();

            BuildReferencedListItems(obj, referencedListItems);

            // Assign Ids on ObjectCreation
            foreach (var referencedObject in referencedListItems)
            {
                referencedObject.ListItemId = Guid.NewGuid().ToString("N");
            }

            return referencedListItems;
        }

        private void BuildReferencedListItems(object obj, List<ListItemCreateRequest> referencedListItems)
        {
            if (obj == null)
                return;

            // Scan child properties for references
            var nonReferencedProperties = obj.GetType()
                .GetProperties()
                .Where(i => i.PropertyType.GenericTypeArguments.FirstOrDefault()?.GetTypeInfo().GetCustomAttribute<PictureparkReferenceAttribute>() == null &&
                            i.PropertyType.GetTypeInfo().GetCustomAttribute<PictureparkReferenceAttribute>() == null);

            foreach (var property in nonReferencedProperties.Where(i => !IsSimpleType(i.PropertyType)))
            {
                if (property.PropertyType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IList)))
                {
                    var list = property.GetValue(obj);
                    if (list != null)
                    {
                        foreach (var value in (IList)list)
                        {
                            BuildReferencedListItems(value, referencedListItems);
                        }
                    }
                }
                else
                {
                    BuildReferencedListItems(property.GetValue(obj), referencedListItems);
                }
            }

            var referencedProperties = obj.GetType()
                .GetProperties()
                .Where(i => i.PropertyType.GenericTypeArguments.FirstOrDefault()?.GetTypeInfo().GetCustomAttribute<PictureparkReferenceAttribute>() != null ||
                            i.PropertyType.GetTypeInfo().GetCustomAttribute<PictureparkReferenceAttribute>() != null);

            foreach (var referencedProperty in referencedProperties)
            {
                var isListProperty = referencedProperty.PropertyType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IList));
                if (isListProperty)
                {
                    // MultiTagbox
                    var values = (IList)referencedProperty.GetValue(obj);
                    if (values != null)
                    {
                        foreach (var value in values)
                        {
                            var refObject = value as IReferenceObject;
                            if (refObject == null || string.IsNullOrEmpty(refObject.RefId))
                            {
                                var schemaId = value.GetType().Name;

                                // Add metadata object if it does not already exist
                                if (referencedListItems.Where(i => i.ContentSchemaId == schemaId).Select(i => i.Content).All(i => i != value))
                                {
                                    referencedListItems.Insert(0, new ListItemCreateRequest
                                    {
                                        ContentSchemaId = schemaId,
                                        Content = value
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    // SingleTagbox
                    var value = referencedProperty.GetValue(obj);
                    if (value != null)
                    {
                        var refObject = value as IReferenceObject;
                        if (refObject == null || string.IsNullOrEmpty(refObject.RefId))
                        {
                            var schemaId = value.GetType().Name;

                            var hasValueBeenAdded = referencedListItems
                                .Any(i => i.ContentSchemaId == schemaId && i.Content == value);

                            if (!hasValueBeenAdded)
                            {
                                referencedListItems.Insert(0, new ListItemCreateRequest
                                {
                                    ContentSchemaId = schemaId,
                                    Content = value
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}
