﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using System.Net.Http;
using Picturepark.SDK.V1.Contract.Attributes;

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

        /// <summary>Creates a <see cref="ListItemDetail"/>.</summary>
        /// <param name="createRequest">The create request.</param>
        /// <param name="resolve">Resolves the data of referenced list items into the contents's content.</param>
        /// <param name="allowMissingDependencies">Allow creating <see cref="ListItem"/>s that refer to list items or contents that don't exist in the system.</param>
        /// <param name="timeout">The timeout in milliseconds to wait for completion.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created <see cref="ListItemDetail"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<ListItemDetail> CreateAsync(ListItemCreateRequest createRequest, bool resolve = false, bool allowMissingDependencies = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await CreateAsync(createRequest, resolve, allowMissingDependencies, timeout, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Creates a <see cref="ListItem"/>s based on an object and its references.</summary>
        /// <param name="content">The object to create <see cref="ListItem"/>s from.</param>
        /// <param name="schemaId">The schema ID of the object.</param>
        /// <param name="allowMissingDependencies">Allow creating <see cref="ListItem"/>s that refer to list items or contents that don't exist in the system.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created <see cref="ListItem"/>s.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<IEnumerable<ListItem>> CreateFromObjectAsync(object content, string schemaId, bool allowMissingDependencies = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            var createManyRequest = new ListItemCreateManyRequest
            {
                Requests = new List<ListItemCreateRequest>(),
                AllowMissingDependencies = allowMissingDependencies
            };

            var referencedObjects = await CreateReferencedObjectsAsync(content, allowMissingDependencies, cancellationToken).ConfigureAwait(false);

            createManyRequest.Requests.Add(new ListItemCreateRequest
            {
                ContentSchemaId = schemaId,
                Content = content
            });

            var objectResult = await CreateManyAsync(createManyRequest, cancellationToken).ConfigureAwait(false);

            var allResults = objectResult.Concat(referencedObjects).ToList();
            return allResults;
        }

        /// <summary>Creates multiple <see cref="ListItem"/>s.</summary>
        /// <param name="createManyRequest">The create many request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created <see cref="ListItem"/>s.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">The business process has not been completed.</exception>
        public async Task<IEnumerable<ListItem>> CreateManyAsync(ListItemCreateManyRequest createManyRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!createManyRequest.Requests.Any())
            {
                return new List<ListItem>();
            }

            var businessProcess = await CreateManyCoreAsync(createManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForCompletionAndReturnItems(businessProcess, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ListItem>> UpdateManyAsync(ListItemUpdateManyRequest updateManyRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!updateManyRequest.Requests.Any())
            {
                return new List<ListItem>();
            }

            var businessProcess = await UpdateManyCoreAsync(updateManyRequest, cancellationToken).ConfigureAwait(false);
            return await WaitForCompletionAndReturnItems(businessProcess, cancellationToken).ConfigureAwait(false);
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
            var listItem = await GetAsync(listItemId, true, cancellationToken: cancellationToken).ConfigureAwait(false);
            return listItem.ConvertTo<T>(schemaId);
        }

        /// <summary>Updates a list item by providing its content.</summary>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="content">The content which must match the item's schema ID.</param>
        /// <param name="resolve">Resolves the data of referenced list items into the contents's content.</param>
        /// <param name="allowMissingDependencies">Allow creating <see cref="ListItem"/>s that refer to list items or contents that don't exist in the system.</param>
        /// <param name="timeout">The timeout in milliseconds to wait for completion.</param>
        /// <param name="patterns">Comma-separated list of display pattern ids. Resolves display values of referenced list items where the display pattern id matches.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated <see cref="ListItemDetail"/>.</returns>
        public async Task<ListItemDetail> UpdateAsync(string listItemId, object content, bool resolve = false, bool allowMissingDependencies = false, TimeSpan? timeout = null, IEnumerable<DisplayPatternType> patterns = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var updateRequest = new ListItemUpdateRequest()
            {
                Id = listItemId,
                Content = content
            };

            return await UpdateAsync(updateRequest, resolve, allowMissingDependencies, timeout, patterns, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Updates a list item.</summary>
        /// <param name="updateRequest">The update request.</param>
        /// <param name="resolve">Resolves the data of referenced list items into the contents's content.</param>
        /// <param name="allowMissingDependencies">Allow creating <see cref="ListItem"/>s that refer to list items or contents that don't exist in the system.</param>
        /// <param name="timeout">The timeout in milliseconds to wait for completion.</param>
        /// <param name="patterns">Comma-separated list of display pattern ids. Resolves display values of referenced list items where the display pattern id matches.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated <see cref="ListItemDetail"/>.</returns>
        public async Task<ListItemDetail> UpdateAsync(ListItemUpdateRequest updateRequest, bool resolve = false, bool allowMissingDependencies = false, TimeSpan? timeout = null, IEnumerable<DisplayPatternType> patterns = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await UpdateCoreAsync(updateRequest.Id, updateRequest, resolve, allowMissingDependencies, timeout, patterns, cancellationToken).ConfigureAwait(false);
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

        private async Task<IEnumerable<ListItem>> WaitForCompletionAndReturnItems(BusinessProcess businessProcess, CancellationToken cancellationToken)
        {
            var waitResult = await _businessProcessClient.WaitForCompletionAsync(businessProcess.Id, null, cancellationToken).ConfigureAwait(false);
            if (waitResult.HasLifeCycleHit)
            {
                if (waitResult.LifeCycleHit == BusinessProcessLifeCycle.Failed)
                {
                    // TODO: ListItemClient.CreateManyAsync: Should we check for Succeeded here?
                    throw new Exception("The business process failed to execute.");
                }

                var details = await _businessProcessClient.GetDetailsAsync(businessProcess.Id, cancellationToken).ConfigureAwait(false);
                var bulkResult = (BusinessProcessDetailsDataBatchResponse)details.Details;

                if (waitResult.LifeCycleHit == BusinessProcessLifeCycle.SucceededWithErrors)
                {
                    if (bulkResult.Response.Rows.Any(i => !i.Succeeded))
                    {
                        // TODO: ListItemClient.CreateManyAsync: Use better exception classes in this method.
                        throw new Exception("Could not save all objects.");
                    }
                }

                // Fetch created objects
                var searchRequest = new ListItemSearchRequest
                {
                    Start = 0,
                    Limit = 1000,
                    Filter = new TermsFilter
                    {
                        Field = "id",
                        Terms = bulkResult.Response.Rows.Select(i => i.Id).ToList()
                    }
                };

                var searchResult = await SearchAsync(searchRequest, cancellationToken).ConfigureAwait(false);
                return searchResult.Results;
            }
            else
            {
                throw new Exception("The business process has not been completed.");
            }
        }

        private async Task<IEnumerable<ListItem>> CreateReferencedObjectsAsync(object obj, bool allowMissingDependencies, CancellationToken cancellationToken = default(CancellationToken))
        {
            var referencedListItems = new List<ListItemCreateRequest>();
            var createManyRequest = new ListItemCreateManyRequest
            {
                Requests = referencedListItems,
                AllowMissingDependencies = allowMissingDependencies
            };

            BuildReferencedListItems(obj, referencedListItems);

            // Assign Ids on ObjectCreation
            foreach (var referencedObject in referencedListItems)
            {
                referencedObject.ListItemId = Guid.NewGuid().ToString("N");
            }

            var results = await CreateManyAsync(createManyRequest, cancellationToken).ConfigureAwait(false);

            foreach (var result in results)
            {
                var objectToUpdate = referencedListItems.SingleOrDefault(i => i.ListItemId == result.Id);

                var reference = objectToUpdate.Content as IReferenceObject;
                if (reference != null)
                {
                    reference.RefId = result.Id;
                }
                else
                {
                    throw new InvalidOperationException("The referenced class '" +
                        objectToUpdate.Content.GetType().FullName +
                        "' does not implement IReferenceObject or inherit from ReferenceObject.");
                }
            }

            return results;
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
