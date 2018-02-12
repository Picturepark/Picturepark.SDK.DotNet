using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests
{
	public class RandomHelper
	{
		public static async Task<ContentSearchResult> GetRandomContentsAsync(PictureparkClient client, string searchString, int limit)
		{
			var request = new ContentSearchRequest() { SearchString = searchString, Limit = limit };
			return await client.Contents.SearchAsync(request);
		}

		public static async Task<string> GetRandomContentIdAsync(PictureparkClient client, string searchString, int limit)
		{
			string contentId = string.Empty;
			ContentSearchRequest request = new ContentSearchRequest() { Limit = limit };

			if (!string.IsNullOrEmpty(searchString))
				request.SearchString = searchString;

			ContentSearchResult result = await client.Contents.SearchAsync(request);

			if (result.Results.Count > 0)
			{
				int randomNumber = new Random().Next(0, result.Results.Count);
				contentId = result.Results.Skip(randomNumber).First().Id;
			}

			return contentId;
		}

		public static async Task<string> GetRandomContentPermissionSetIdAsync(PictureparkClient client, int limit)
		{
			string permissionSetId = string.Empty;
			PermissionSetSearchRequest request = new PermissionSetSearchRequest { Limit = limit };
			PermissionSetSearchResult result = await client.Permissions.SearchContentPermissionSetsAsync(request);

			if (result.Results.Count > 0)
			{
				var randomNumber = new Random().Next(0, result.Results.Count);
				permissionSetId = result.Results.Skip(randomNumber).First().Id;
			}

			return permissionSetId;
		}

		public static async Task<string> GetRandomTransferIdAsync(PictureparkClient client, TransferState? transferState, int limit)
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

			var result = await client.Transfers.SearchAsync(request);

			var transfers = result.Results;

			if (transfers.Count <= 0)
				return transferId;

			var randomNumber = new Random().Next(0, transfers.Count);
			transferId = transfers.Skip(randomNumber).First().Id;

			return transferId;
		}

		public static async Task<string> GetRandomFileTransferIdAsync(PictureparkClient client, int limit)
		{
			string fileTransferId = string.Empty;
			FileTransferSearchRequest request = new FileTransferSearchRequest() { Limit = limit };
			FileTransferSearchResult result = await client.Transfers.SearchFilesAsync(request);

			if (result.Results.Count > 0)
			{
				int randomNumber = new Random().Next(0, result.Results.Count);
				fileTransferId = result.Results.Skip(randomNumber).First().Id;
			}

			return fileTransferId;
		}

		public static async Task<string> GetRandomMetadataPermissionSetIdAsync(PictureparkClient client, int limit)
		{
			string permissionSetId = string.Empty;
			var request = new PermissionSetSearchRequest() { Limit = limit };
			PermissionSetSearchResult result = await client.Permissions.SearchSchemaPermissionSetsAsync(request);

			if (result.Results.Count > 0)
			{
				int randomNumber = new Random().Next(0, result.Results.Count);
				permissionSetId = result.Results.Skip(randomNumber).First().Id;
			}

			return permissionSetId;
		}

		public static async Task<string> GetRandomSchemaIdAsync(PictureparkClient client, int limit)
		{
			string schemaId = string.Empty;
			var request = new SchemaSearchRequest { Limit = limit };
			SchemaSearchResult result = await client.Schemas.SearchAsync(request);

			if (result.Results.Count > 0)
			{
				int randomNumber = new Random().Next(0, result.Results.Count);
				schemaId = result.Results.Skip(randomNumber).First().Id;
			}

			return schemaId;
		}

		public static async Task<string> GetRandomObjectIdAsync(PictureparkClient client, string schemaId, int limit)
		{
			string objectId = string.Empty;

			ListItemSearchRequest request = new ListItemSearchRequest()
			{
				Limit = limit,
				SchemaIds = new List<string> { schemaId }
			};

			var result = await client.ListItems.SearchAsync(request);

			if (result.Results.Count > 0)
			{
				int randomNumber = new Random().Next(0, result.Results.Count);
				objectId = result.Results.Skip(randomNumber).First().Id;
			}

			return objectId;
		}

		public static async Task<string> GetRandomShareIdAsync(PictureparkClient client, ShareType shareType, int limit)
		{
			var shareId = string.Empty;

			var request = new ShareSearchRequest
			{
				Limit = limit,
				Filter = new TermFilter { Field = "shareType", Term = shareType.ToString() }
			};

			var result = await client.Shares.SearchAsync(request);

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
