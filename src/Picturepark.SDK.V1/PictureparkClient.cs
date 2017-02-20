using System;
using Picturepark.SDK.V1.Authentication;

namespace Picturepark.SDK.V1
{
	public class PictureparkClient : IDisposable
	{
		public PictureparkClient(string baseUrl, IAuthClient authClient)
		{
			Assets = new AssetClient(baseUrl, authClient);
			BusinessProcesses = new BusinessProcessClient(baseUrl, authClient);
			DocumentHistory = new DocumentHistoryClient(baseUrl, authClient);
			JsonSchemas = new JsonSchemaClient(baseUrl, authClient);
			Permissions = new PermissionClient(baseUrl, authClient);
			PublicAccess = new PublicAccessClient(baseUrl, authClient);
			Shares = new ShareClient(baseUrl, authClient);
			Users = new UserClient(baseUrl, authClient);
			Schemas = new MetadataSchemaClient(BusinessProcesses, authClient);
			Transfers = new TransferClient(BusinessProcesses, authClient);
			MetadataObjects = new MetadataObjectClient(Transfers, authClient);
		}

		public MetadataSchemaClient Schemas { get; }

		public AssetClient Assets { get; }

		public BusinessProcessClient BusinessProcesses { get; }

		public DocumentHistoryClient DocumentHistory { get; }

		public JsonSchemaClient JsonSchemas { get; }

		public MetadataObjectClient MetadataObjects { get; }

		public PermissionClient Permissions { get; }

		public PublicAccessClient PublicAccess { get; }

		public ShareClient Shares { get; }

		public TransferClient Transfers { get; }

		public UserClient Users { get; }

		public void Dispose()
		{
		}
	}
}
