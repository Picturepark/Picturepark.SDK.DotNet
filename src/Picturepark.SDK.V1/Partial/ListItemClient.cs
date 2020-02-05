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
        public const string RootObjectRequestId = "rootObjectRequestId";

        private readonly IBusinessProcessClient _businessProcessClient;

        public ListItemClient(IBusinessProcessClient businessProcessClient, IPictureparkServiceSettings settings, HttpClient httpClient)
            : this(settings, httpClient)
        {
            _businessProcessClient = businessProcessClient;
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationWithRequestIdResult> CreateFromObjectAsync(object content, bool allowMissingDependencies = false, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
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

            var schemaId = ClassToSchemaConverter.ResolveSchemaId(content.GetType());

            createManyRequest.Items.Add(new ListItemCreateRequest
            {
                ContentSchemaId = schemaId,
                Content = content,
                RequestId = RootObjectRequestId
            });

            var objectResult = await CreateManyAsync(createManyRequest, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
            return objectResult;
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationWithRequestIdResult> CreateManyAsync(ListItemCreateManyRequest createManyRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!createManyRequest.Items.Any())
            {
                return ListItemBatchOperationWithRequestIdResult.Empty;
            }

            var businessProcess = await CreateManyCoreAsync(createManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResultWithRequestId(businessProcess.Id, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationResult> UpdateManyAsync(ListItemUpdateManyRequest listItemUpdateManyRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!listItemUpdateManyRequest.Items.Any())
            {
                return ListItemBatchOperationResult.Empty;
            }

            var businessProcess = await UpdateManyCoreAsync(listItemUpdateManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationResult> BatchUpdateFieldsByFilterAsync(ListItemFieldsBatchUpdateFilterRequest updateRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await BatchUpdateFieldsByFilterCoreAsync(updateRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationResult> BatchUpdateFieldsByIdsAsync(ListItemFieldsBatchUpdateRequest updateRequest, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var businessProcess = await BatchUpdateFieldsByIdsCoreAsync(updateRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForBusinessProcessAndReturnResult(businessProcess.Id, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<T> GetAndConvertToAsync<T>(string listItemId, string schemaId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listItem = await GetAsync(listItemId, new ListItemResolveBehavior[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems }, cancellationToken).ConfigureAwait(false);
            return listItem.ConvertTo<T>();
        }

        /// <inheritdoc />
        public async Task<ICollection<T>> GetManyAndConvertToAsync<T>(IEnumerable<string> listItemIds, string schemaId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listItems = await GetManyAsync(listItemIds, new ListItemResolveBehavior[] { ListItemResolveBehavior.Content, ListItemResolveBehavior.LinkedListItems }, cancellationToken).ConfigureAwait(false);
            return listItems.Select(li => li.ConvertTo<T>()).ToList();
        }

        /// <inheritdoc />
        public async Task<ListItemDetail> UpdateAsync(string listItemId, object content, UpdateOption updateOption = UpdateOption.Merge, IEnumerable<ListItemResolveBehavior> resolveBehaviors = null, bool allowMissingDependencies = false, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var updateRequest = new ListItemUpdateRequest
            {
                Content = content,
                ContentFieldsUpdateOptions = updateOption
            };

            return await UpdateAsync(listItemId, updateRequest, resolveBehaviors, allowMissingDependencies, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ListItemDetail> UpdateAsync(string listItemId, ListItemUpdateRequest updateRequest, IEnumerable<ListItemResolveBehavior> resolveBehaviors = null, bool allowMissingDependencies = false, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await UpdateCoreAsync(listItemId, updateRequest, resolveBehaviors, allowMissingDependencies, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationResult> WaitForBusinessProcessAndReturnResult(string businessProcessId, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _businessProcessClient.WaitForCompletionAsync(businessProcessId, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);

            return new ListItemBatchOperationResult(this, businessProcessId, result.LifeCycleHit, _businessProcessClient);
        }

        /// <inheritdoc />
        public async Task<ListItemBatchOperationWithRequestIdResult> WaitForBusinessProcessAndReturnResultWithRequestId(string businessProcessId, TimeSpan? timeout = null, bool waitSearchDocCreation = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _businessProcessClient.WaitForCompletionAsync(businessProcessId, timeout, waitSearchDocCreation, cancellationToken).ConfigureAwait(false);

            return new ListItemBatchOperationWithRequestIdResult(this, businessProcessId, result.LifeCycleHit, _businessProcessClient);
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
                            if (refObject == null || (string.IsNullOrEmpty(refObject.RefId) && string.IsNullOrEmpty(refObject.RefRequestId)))
                            {
                                var schemaId = value.GetType().Name;

                                // Add metadata object if it does not already exist
                                if (referencedListItems.Where(i => i.ContentSchemaId == schemaId).Select(i => i.Content).All(i => i != value))
                                {
                                    var listItemRequestId = Guid.NewGuid().ToString("N");
                                    if (refObject != null)
                                        refObject.RefRequestId = listItemRequestId;

                                    referencedListItems.Insert(0, new ListItemCreateRequest
                                    {
                                        ContentSchemaId = schemaId,
                                        Content = value,
                                        RequestId = listItemRequestId
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
                        if (refObject == null || (string.IsNullOrEmpty(refObject.RefId) && string.IsNullOrEmpty(refObject.RefRequestId)))
                        {
                            var schemaId = value.GetType().Name;

                            var hasValueBeenAdded = referencedListItems
                                .Any(i => i.ContentSchemaId == schemaId && i.Content == value);

                            if (hasValueBeenAdded)
                                continue;

                            var listItemRequestId = Guid.NewGuid().ToString("N");
                            if (refObject != null)
                                refObject.RefRequestId = listItemRequestId;
                            referencedListItems.Insert(0, new ListItemCreateRequest
                            {
                                ContentSchemaId = schemaId,
                                Content = value,
                                RequestId = listItemRequestId
                            });
                        }
                    }
                }
            }
        }
    }
}
