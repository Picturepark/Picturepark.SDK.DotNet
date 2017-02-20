using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Clients;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Interfaces;

namespace Picturepark.SDK.V1
{
	public class MetadataObjectClient : MetadataObjectsClientBase
    {
        private readonly TransferClient _transferClient;

        public MetadataObjectClient(TransferClient transferClient, IAuthClient authClient)
            : base(authClient)
		{
		    BaseUrl = transferClient.BaseUrl;
		    _transferClient = transferClient;
		}

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<MetadataObjectDetailViewItem> CreateAsync(MetadataObjectCreateRequest metadataObject, bool resolve = false, int timeout = 60000)
        {
            return await CreateAsync(metadataObject, resolve, timeout, string.Empty);
        }

        public MetadataObjectDetailViewItem Create(MetadataObjectCreateRequest metadataObject, bool resolve = false, int timeout = 60000)
        {
            return Task.Run(async () => await CreateAsync(metadataObject, resolve: resolve, timeout: timeout)).GetAwaiter().GetResult();
        }

        // TODO(rsu): Rename
        public async Task<MetadataObjectViewItem> CreateAbcAsync(MetadataObjectCreateRequest createRequest)
		{
			var result = await CreateManyAsync(new List<MetadataObjectCreateRequest> { createRequest });
			return result.First();
		}

		public async Task DeleteAsync(string objectId, CancellationToken cancellationToken = default(CancellationToken))
		{
			await DeleteAsync(objectId, 60000, cancellationToken);
		}

		public void Delete(string objectId)
		{
			Task.Run(async () => await DeleteAsync(objectId)).GetAwaiter().GetResult();
		}

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<MetadataObjectDetailViewItem> UpdateAsync(string objectId, MetadataObjectUpdateRequest updateRequest, bool resolve = false, string pattern = "", int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await UpdateAsync(objectId, updateRequest, resolve, timeout, pattern, cancellationToken);
		}

		public MetadataObjectDetailViewItem Update(string objectId, MetadataObjectUpdateRequest updateRequest, bool resolve = false, string pattern = "", int timeout = 60000)
		{
			return Task.Run(async () => await UpdateAsync(objectId, updateRequest, resolve: resolve, timeout: timeout, pattern: pattern)).GetAwaiter().GetResult();
		}

		public async Task<List<MetadataObjectViewItem>> CreateFromPOCO(object obj, string schemaId)
		{
			var metadataObjects = new List<MetadataObjectCreateRequest>();
			var metadata = new MetadataDictionary();
			metadata[schemaId] = obj;

			var referencedObjects = await CreateReferencedObjects(obj);

			metadataObjects.Add(new MetadataObjectCreateRequest
			{
				MetadataSchemaId = schemaId,
				Metadata = metadata
			});
			var objectResult = await CreateManyAsync(metadataObjects);

			var allResults = objectResult.Concat(referencedObjects).ToList();
			return allResults;
		}

		public IEnumerable<MetadataObjectViewItem> CreateMany(IEnumerable<MetadataObjectCreateRequest> objects)
		{
			return Task.Run(async () => await CreateManyAsync(objects)).GetAwaiter().GetResult();
		}

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<IEnumerable<MetadataObjectViewItem>> CreateManyAsync(IEnumerable<MetadataObjectCreateRequest> metadataObjects, CancellationToken? cancellationToken = null)
		{
			if (metadataObjects.Any())
			{
				var createRequest = await CreateManyCoreAsync(metadataObjects, cancellationToken ?? CancellationToken.None);
				var result = await createRequest.Wait4MetadataAsync(this);

				var bulkResult = result.BusinessProcess as BusinessProcessBulkResponseViewItem;
				if (bulkResult.Response.Rows.Any(i => i.Succeeded == false))
					throw new Exception("Could not save all objects");

				// Fetch created objects
				var searchResult = await SearchAsync(new MetadataObjectSearchRequest
				{
					Start = 0,
					Limit = 1000,
					Filter = new TermsFilter
					{
						Field = "Id",
						Terms = bulkResult.Response.Rows.Select(i => i.Id).ToList()
					}
				});

				return searchResult.Results;
			}
			else
			{
				return new List<MetadataObjectViewItem>();
			}
		}

		public async Task UpdateMetadataObjectAsync(MetadataObjectUpdateRequest updateRequest)
		{
			var result = await UpdateManyAsync(new List<MetadataObjectUpdateRequest>() { updateRequest });
			var wait = await result.Wait4MetadataAsync(this);
		}

		public async Task UpdateMetadataObjectAsync(MetadataObjectDetailViewItem metadataObject, object obj, string schemaId)
		{
			var convertedObject = new MetadataObjectViewItem
			{
				ContentMetadataSchemaId = metadataObject.ContentMetadataSchemaId,
				EntityType = metadataObject.EntityType,
				Id = metadataObject.Id,
				Metadata = metadataObject.Metadata,
				MetadataSchemaIds = metadataObject.MetadataSchemaIds
			};
			await UpdateMetadataObjectAsync(convertedObject, obj, schemaId);
		}

		public async Task UpdateMetadataObjectAsync(MetadataObjectViewItem metadataObject, object obj, string schemaId)
		{
			var metadata = new MetadataDictionary();
			metadata[schemaId] = obj;

			var request = new MetadataObjectUpdateRequest()
			{
				Id = metadataObject.Id,
				Metadata = metadata,
				MetadataSchemaIds = metadataObject.MetadataSchemaIds
			};

			await UpdateMetadataObjectAsync(request);
		}

		public async Task<T> GetObjectAsync<T>(string objectId)
		{
			var metadataViewItem = await GetAsync(objectId);
			return metadataViewItem.Metadata.Get<T>();
		}

		public async Task ImportFromJsonAsync(string jsonFilePath, bool includeObjects)
		{
			var filePaths = new List<string>() { jsonFilePath };

			var batchName = "Metadata import: " + Path.GetFileName(jsonFilePath);
			List<string> fileNames = filePaths.Select(file => Path.GetFileName(file)).ToList();

			// Create batch
			TransferViewItem transfer = await _transferClient.CreateBatchAsync(fileNames, batchName);

			// Upload files
			string directoryPath = Path.GetDirectoryName(filePaths.First());

			await _transferClient.UploadFilesAsync(
				filePaths,
				directoryPath,
				transfer,
				successDelegate: (file) => { Debug.WriteLine(file); },
				errorDelegate: (error) => { Debug.WriteLine(error); }
				);

			// Import metadata
			string fileTransferId = await _transferClient.GetFileTransferIdFromBatchTransferId(transfer.Id);
			await ImportAsync(null, fileTransferId, includeObjects);
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

		private async Task<IEnumerable<MetadataObjectViewItem>> CreateReferencedObjects(object obj)
		{
			var referencedMetadataObjects = new List<MetadataObjectCreateRequest>();
			BuildReferencedMetadataObjects(obj, referencedMetadataObjects);

			// Assign Ids on ObjectCreation
			foreach (var referencedObject in referencedMetadataObjects)
			{
				referencedObject.MetadataObjectId = Guid.NewGuid().ToString("N");
			}

			var results = await CreateManyAsync(referencedMetadataObjects);

			foreach (var result in results)
			{
				var object2Update = referencedMetadataObjects.SingleOrDefault(i => i.MetadataObjectId == result.Id);
				var reference = (object2Update.Metadata as Dictionary<string, object>)[object2Update.MetadataSchemaId] as IReference;
				reference.refId = result.Id;
			}

			return results;
		}

		private void BuildReferencedMetadataObjects(object obj, List<MetadataObjectCreateRequest> referencedMetadataObjects)
		{
			// Scan child properties for references
			var nonReferencedProperties = obj.GetType().GetProperties().Where(i => !typeof(IReference).IsAssignableFrom(i.PropertyType.GenericTypeArguments.FirstOrDefault()) && !typeof(IReference).IsAssignableFrom(i.PropertyType));
			foreach (var property in nonReferencedProperties.Where(i => !IsSimpleType(i.PropertyType)))
			{
				if (property.PropertyType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IList)))
				{
					foreach (var value in (IList)property.GetValue(obj))
					{
						BuildReferencedMetadataObjects(value, referencedMetadataObjects);
					}
				}
				else
				{
					BuildReferencedMetadataObjects(property.GetValue(obj), referencedMetadataObjects);
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
								var metadata = new MetadataDictionary();
								metadata[schemaId] = value;

								// Add metadata object if it does not already exist
								if (referencedMetadataObjects.Where(i => i.MetadataSchemaId == schemaId).Select(i => i.Metadata).All(i => i[schemaId] != value))
								{
									referencedMetadataObjects.Insert(0, new MetadataObjectCreateRequest
									{
										MetadataSchemaId = schemaId,
										Metadata = metadata
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
							var metadata = new MetadataDictionary();
							metadata[schemaId] = value;

							// Add metadata object if it does not already exist
							if (referencedMetadataObjects.Where(i => i.MetadataSchemaId == schemaId).Select(i => i.Metadata).All(i => i[schemaId] != value))
							{
								referencedMetadataObjects.Insert(0, new MetadataObjectCreateRequest
								{
									MetadataSchemaId = schemaId,
									Metadata = metadata
								});
							}
						}
					}
				}
			}
		}
	}
}
