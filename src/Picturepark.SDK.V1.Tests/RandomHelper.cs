using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests
{
	public class RandomHelper
	{
		public static ContentSearchResult GetRandomContents(PictureparkClient client, string searchString, int limit)
		{
			var request = new ContentSearchRequest() { SearchString = searchString, Limit = limit };
			return client.Contents.Search(request);
		}

		public static string GetRandomContentId(PictureparkClient client, string searchString, int limit)
		{
			string contentId = string.Empty;
			ContentSearchRequest request = new ContentSearchRequest() { Limit = limit };

			if (!string.IsNullOrEmpty(searchString))
				request.SearchString = searchString;

			ContentSearchResult result = client.Contents.Search(request);

			if (result.Results.Count > 0)
			{
				int randomNumber = new Random().Next(0, result.Results.Count);
				contentId = result.Results.Skip(randomNumber).First().Id;
			}

			return contentId;
		}

		public static string GetRandomContentPermissionSetId(PictureparkClient client, int limit)
		{
			string permissionSetId = string.Empty;
			PermissionSetSearchRequest request = new PermissionSetSearchRequest { Limit = limit };
			PermissionSetSearchResult result = client.Permissions.SearchContentPermissionsAsync(request).Result;

			if (result.Results.Count > 0)
			{
				var randomNumber = new Random().Next(0, result.Results.Count);
				permissionSetId = result.Results.Skip(randomNumber).First().Id;
			}

			return permissionSetId;
		}

		public static string GetRandomTransferId(PictureparkClient client, TransferState? transferState, int limit)
		{
			var transferId = string.Empty;

			var request = new TransferSearchRequest { Limit = limit };
			if (transferState.HasValue)
			{
				request.Filter = new ChildFilter
				{
					ChildType = "TransferInfo",
					Filter = new TermFilter
					{
						Field = "state",
						Term = transferState.ToString()
					}
				};
			}

			var result = client.Transfers.Search(request);

			var transfers = result.Results;

			if (transfers.Count <= 0)
				return transferId;

			var randomNumber = new Random().Next(0, transfers.Count);
			transferId = transfers.Skip(randomNumber).First().Id;

			return transferId;
		}

		public static string GetRandomFileTransferId(PictureparkClient client, int limit)
		{
			string fileTransferId = string.Empty;
			FileTransferSearchRequest request = new FileTransferSearchRequest() { Limit = limit };
			FileTransferSearchResult result = client.Transfers.SearchFiles(request);

			if (result.Results.Count > 0)
			{
				int randomNumber = new Random().Next(0, result.Results.Count);
				fileTransferId = result.Results.Skip(randomNumber).First().Id;
			}

			return fileTransferId;
		}

		public static string GetRandomMetadataPermissionSetId(PictureparkClient client, int limit)
		{
			string permissionSetId = string.Empty;
			var request = new PermissionSetSearchRequest() { Limit = limit };
			PermissionSetSearchResult result = client.Permissions.SearchSchemaPermissions(request);

			if (result.Results.Count > 0)
			{
				int randomNumber = new Random().Next(0, result.Results.Count);
				permissionSetId = result.Results.Skip(randomNumber).First().Id;
			}

			return permissionSetId;
		}

		public static string GetRandomSchemaId(PictureparkClient client, int limit)
		{
			string schemaId = string.Empty;
			var request = new SchemaSearchRequest { Limit = limit };
			SchemaSearchResult result = client.Schemas.Search(request);

			if (result.Results.Count > 0)
			{
				int randomNumber = new Random().Next(0, result.Results.Count);
				schemaId = result.Results.Skip(randomNumber).First().Id;
			}

			return schemaId;
		}

		public static string GetRandomObjectId(PictureparkClient client, string schemaId, int limit)
		{
			string objectId = string.Empty;

			ListItemSearchRequest request = new ListItemSearchRequest()
			{
				Limit = limit,
				SchemaIds = new List<string> { schemaId }
			};

			var result = client.ListItems.SearchAsync(request).Result;

			if (result.Results.Count > 0)
			{
				int randomNumber = new Random().Next(0, result.Results.Count);
				objectId = result.Results.Skip(randomNumber).First().Id;
			}

			return objectId;
		}

		public static string GetRandomShareId(PictureparkClient client, EntityType entityType, int limit)
		{
			var shareId = string.Empty;

			var request = new ShareSearchRequest
			{
				Limit = limit,
				Filter = new TermFilter { Field = "entityType", Term = entityType.ToString() }
			};

			var result = client.Shares.Search(request);

			var shares = result.Results;
			if (shares.Count > 0)
			{
				var randomNumber = new Random().Next(0, shares.Count);
				shareId = shares.Skip(randomNumber).First().Id;
			}

			return shareId;
		}
	}
}
