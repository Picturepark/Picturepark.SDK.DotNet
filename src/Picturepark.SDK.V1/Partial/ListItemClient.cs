using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Newtonsoft.Json.Linq;
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

		public ListItemDetail Create(ListItemCreateRequest listItem, bool resolve = false, int timeout = 60000)
		{
			return Task.Run(async () => await CreateAsync(listItem, resolve: resolve, timeout: timeout)).GetAwaiter().GetResult();
		}

		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task<ListItemDetail> CreateAsync(ListItemCreateRequest listItem, bool resolve = false, int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await CreateAsync(listItem, resolve, timeout, null, cancellationToken);
		}

		public void Delete(string objectId)
		{
			Task.Run(async () => await DeleteAsync(objectId)).GetAwaiter().GetResult();
		}

		public async Task DeleteAsync(string objectId, CancellationToken cancellationToken = default(CancellationToken))
		{
			await DeleteAsync(objectId, 60000, cancellationToken);
		}

		public ListItemDetail Update(string objectId, ListItemUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000)
		{
			// TODO: ListItemClient.Update: Is this method really needed? The only diff are the order of the parameters...
			return Task.Run(async () => await UpdateAsync(objectId, updateRequest, resolve, patterns, timeout)).GetAwaiter().GetResult();
		}

		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task<ListItemDetail> UpdateAsync(string objectId, ListItemUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken))
		{
			// TODO: ListItemClient.UpdateAsync: Is this method really needed? The only diff are the order of the parameters...
			return await UpdateAsync(objectId, updateRequest, resolve, timeout, patterns, cancellationToken);
		}

		public async Task UpdateAsync(ListItemDetail listItem, object obj, string schemaId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var convertedObject = new ListItem
			{
				ContentSchemaId = listItem.ContentSchemaId,
				Id = listItem.Id,
				Content = listItem.Content
			};

			await UpdateAsync(convertedObject, obj, schemaId, cancellationToken);
		}

		public async Task UpdateAsync(ListItem listItem, object obj, string schemaId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var request = new ListItemUpdateRequest()
			{
				Id = listItem.Id,
				Content = obj
			};

			await UpdateAsync(request, cancellationToken);
		}

		public async Task UpdateAsync(ListItemUpdateRequest updateRequest, CancellationToken cancellationToken = default(CancellationToken))
		{
			var businessProcess = await UpdateManyAsync(new List<ListItemUpdateRequest>() { updateRequest }, cancellationToken);
			var waitResult = await _businessProcessClient.WaitForCompletionAsync(businessProcess.Id, cancellationToken);
		}

		public async Task<IEnumerable<ListItem>> CreateFromObjectAsync(object obj, string schemaId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var listItems = new List<ListItemCreateRequest>();
			var referencedObjects = await CreateReferencedObjectsAsync(obj, cancellationToken);

			listItems.Add(new ListItemCreateRequest
			{
				ContentSchemaId = schemaId,
				Content = obj
			});

			var objectResult = await CreateManyAsync(listItems, cancellationToken);

			var allResults = objectResult.Concat(referencedObjects).ToList();
			return allResults;
		}

		public IEnumerable<ListItem> CreateMany(IEnumerable<ListItemCreateRequest> objects)
		{
			return Task.Run(async () => await CreateManyAsync(objects)).GetAwaiter().GetResult();
		}

		/// <exception cref="ApiException">A server side error occurred.</exception>
		/// <exception cref="PictureparkException">The business process has not been completed.</exception>
		public async Task<IEnumerable<ListItem>> CreateManyAsync(IEnumerable<ListItemCreateRequest> listItems, CancellationToken cancellationToken = default(CancellationToken))
		{
			var listItemCreateRequests = listItems as IList<ListItemCreateRequest> ?? listItems.ToList();
			if (!listItemCreateRequests.Any())
			{
				return new List<ListItem>();
			}

			var businessProcess = await CreateManyCoreAsync(listItemCreateRequests, cancellationToken);

			var waitResult = await _businessProcessClient.WaitForCompletionAsync(businessProcess.Id, cancellationToken);
			if (waitResult.HasLifeCycleHit)
			{
				var details = await _businessProcessClient.GetDetailsAsync(businessProcess.Id, cancellationToken);

				var bulkResult = (BusinessProcessDetailsDataBulkResponse)details.Details;
				if (bulkResult.Response.Rows.Any(i => i.Succeeded == false))
				{
					throw new Exception("Could not save all objects.");
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

				var searchResult = await SearchAsync(searchRequest, cancellationToken);
				return searchResult.Results;
			}
			else
			{
				throw new Exception("The business process has not been completed.");
			}
		}

		public async Task<T> GetObjectAsync<T>(string objectId, string schemaId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var listItem = await GetAsync(objectId, true, cancellationToken: cancellationToken);
			return (listItem.Content as JObject).ToObject<T>();
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

		private async Task<IEnumerable<ListItem>> CreateReferencedObjectsAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
		{
			var referencedListItems = new List<ListItemCreateRequest>();
			BuildReferencedListItems(obj, referencedListItems);

			// Assign Ids on ObjectCreation
			foreach (var referencedObject in referencedListItems)
			{
				referencedObject.ListItemId = Guid.NewGuid().ToString("N");
			}

			var results = await CreateManyAsync(referencedListItems, cancellationToken);

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
			// Scan child properties for references
			var nonReferencedProperties = obj.GetType()
				.GetProperties()
				.Where(i => i.PropertyType.GenericTypeArguments.FirstOrDefault()?.GetTypeInfo().GetCustomAttribute<PictureparkReferenceAttribute>() == null &&
							i.PropertyType.GetTypeInfo().GetCustomAttribute<PictureparkReferenceAttribute>() == null);

			foreach (var property in nonReferencedProperties.Where(i => !IsSimpleType(i.PropertyType)))
			{
				if (property.PropertyType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IList)))
				{
					foreach (var value in (IList)property.GetValue(obj))
					{
						BuildReferencedListItems(value, referencedListItems);
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
