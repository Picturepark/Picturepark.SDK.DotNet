using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Extensions;
using Picturepark.SDK.V1.Contract.Interfaces;
using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1
{
	public partial class ListItemClient
	{
		private readonly IBusinessProcessClient _businessProcessClient;

		public ListItemClient(IBusinessProcessClient businessProcessClient, IPictureparkClientSettings settings) : this(settings)
		{
			_businessProcessClient = businessProcessClient;
		}

		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task<ListItemDetail> CreateAsync(ListItemCreateRequest listItem, bool resolve = false, int timeout = 60000)
		{
			return await CreateAsync(listItem, resolve, timeout, null);
		}

		public ListItemDetail Create(ListItemCreateRequest listItem, bool resolve = false, int timeout = 60000)
		{
			return Task.Run(async () => await CreateAsync(listItem, resolve: resolve, timeout: timeout)).GetAwaiter().GetResult();
		}

		public async Task DeleteAsync(string objectId, CancellationToken cancellationToken = default(CancellationToken))
		{
			await DeleteAsync(objectId, 60000, cancellationToken);
		}

		public void Delete(string objectId)
		{
			Task.Run(async () => await DeleteAsync(objectId)).GetAwaiter().GetResult();
		}

		public ListItemDetail Update(string objectId, ListItemUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000)
		{
			return Task.Run(async () => await UpdateAsync(objectId, updateRequest, resolve, timeout, patterns)).GetAwaiter().GetResult();
		}

		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task<ListItemDetail> UpdateAsync(string objectId, ListItemUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await UpdateAsync(objectId, updateRequest, resolve, timeout, patterns, cancellationToken);
		}

		public async Task<List<ListItem>> CreateFromPOCO(object obj, string schemaId)
		{
			var listItems = new List<ListItemCreateRequest>();

			var referencedObjects = await CreateReferencedObjects(obj);

			listItems.Add(new ListItemCreateRequest
			{
				ContentSchemaId = schemaId,
				Content = obj
			});
			var objectResult = await CreateManyAsync(listItems);

			var allResults = objectResult.Concat(referencedObjects).ToList();
			return allResults;
		}

		public IEnumerable<ListItem> CreateMany(IEnumerable<ListItemCreateRequest> objects)
		{
			return Task.Run(async () => await CreateManyAsync(objects)).GetAwaiter().GetResult();
		}

		/// <exception cref="ApiException">A server side error occurred.</exception>
		public async Task<List<ListItem>> CreateManyAsync(IEnumerable<ListItemCreateRequest> listItems, CancellationToken? cancellationToken = null)
		{
			var listItemCreateRequests = listItems as IList<ListItemCreateRequest> ?? listItems.ToList();
			if (!listItemCreateRequests.Any())
			{
				return new List<ListItem>();
			}

			var createRequest = await CreateManyCoreAsync(listItemCreateRequests, cancellationToken ?? CancellationToken.None);
			var result = await createRequest.Wait4MetadataAsync(_businessProcessClient);

			var bulkResult = (BusinessProcessBulkResponse)result.BusinessProcess;
			if (bulkResult.Response.Rows.Any(i => i.Succeeded == false))
				throw new Exception("Could not save all objects");

			// Fetch created objects
			var searchResult = await SearchAsync(new ListItemSearchRequest
			{
				Start = 0,
				Limit = 1000,
				Filter = new TermsFilter
				{
					Field = "id",
					Terms = bulkResult.Response.Rows.Select(i => i.Id).ToList()
				}
			});

			return searchResult.Results;
		}

		public async Task UpdateListItemAsync(ListItemUpdateRequest updateRequest)
		{
			var result = await UpdateManyAsync(new List<ListItemUpdateRequest>() { updateRequest });
			var wait = await result.Wait4MetadataAsync(_businessProcessClient);
		}

		public async Task UpdateListItemAsync(ListItemDetail listItem, object obj, string schemaId)
		{
			var convertedObject = new ListItem
			{
				ContentSchemaId = listItem.ContentSchemaId,
				EntityType = listItem.EntityType,
				Id = listItem.Id,
				Content = listItem.Content
			};
			await UpdateListItemAsync(convertedObject, obj, schemaId);
		}

		public async Task UpdateListItemAsync(ListItem listItem, object obj, string schemaId)
		{
			var request = new ListItemUpdateRequest()
			{
				Id = listItem.Id,
				Content = obj
			};

			await UpdateListItemAsync(request);
		}

		public async Task<T> GetObjectAsync<T>(string objectId, string schemaId)
		{
			var listItem = await GetAsync(objectId, true);
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

		private async Task<IEnumerable<ListItem>> CreateReferencedObjects(object obj)
		{
			var referencedListItems = new List<ListItemCreateRequest>();
			BuildReferencedListItems(obj, referencedListItems);

			// Assign Ids on ObjectCreation
			foreach (var referencedObject in referencedListItems)
			{
				referencedObject.ListItemId = Guid.NewGuid().ToString("N");
			}

			var results = await CreateManyAsync(referencedListItems);

			foreach (var result in results)
			{
				var object2Update = referencedListItems.SingleOrDefault(i => i.ListItemId == result.Id);
				var reference = object2Update.Content as IReference;
				reference.refId = result.Id;
			}

			return results;
		}

		private void BuildReferencedListItems(object obj, List<ListItemCreateRequest> referencedListItems)
		{
			// Scan child properties for references
			var nonReferencedProperties = obj.GetType().GetProperties().Where(i => !typeof(IReference).IsAssignableFrom(i.PropertyType.GenericTypeArguments.FirstOrDefault()) && !typeof(IReference).IsAssignableFrom(i.PropertyType));
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

			var referencedProperties = obj.GetType().GetProperties().Where(i => typeof(IReference).IsAssignableFrom(i.PropertyType.GenericTypeArguments.FirstOrDefault()) || typeof(IReference).IsAssignableFrom(i.PropertyType));
			foreach (var referencedProperty in referencedProperties)
			{
				if (referencedProperty.PropertyType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IList)))
				{
					// SchemaItems
					var values = (IList)referencedProperty.GetValue(obj);
					if (values != null)
					{
						foreach (var value in values)
						{
							var refIdValue = (string)value.GetType().GetProperty("refId").GetValue(value);
							if (string.IsNullOrEmpty(refIdValue))
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
					// SchemaItem
					// TODO(rsu): Always false?
					if (referencedProperty.GetType().GetProperty("refId") == null)
					{
						var value = referencedProperty.GetValue(obj);
						if (value != null)
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
		}
	}
}
