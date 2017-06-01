using System;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
	public class PictureparkClient : IDisposable, IPictureparkClient
	{
		/// <summary>Initializes a new instance of the <see cref="PictureparkClient"/> class and uses the <see cref="IPictureparkClientSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
		/// <param name="settings">The client settings.</param>
		public PictureparkClient(IPictureparkClientSettings settings)
		{
			Outputs = new OutputClient(settings);
			Contents = new ContentClient(settings);
			BusinessProcesses = new BusinessProcessClient(settings);
			DocumentHistory = new DocumentHistoryClient(settings);
			JsonSchemas = new JsonSchemaClient(settings);
			Permissions = new PermissionClient(settings);
			PublicAccess = new PublicAccessClient(settings);
			Shares = new ShareClient(settings);
			Users = new UserClient(settings);
			Schemas = new SchemaClient((BusinessProcessClient)BusinessProcesses, settings);
			Transfers = new TransferClient((BusinessProcessClient)BusinessProcesses, settings);
			ListItems = new ListItemClient((TransferClient)Transfers, settings);
		}

		public ISchemaClient Schemas { get; }

		public IContentClient Contents { get; }

		public IOutputClient Outputs { get; }

		public IBusinessProcessClient BusinessProcesses { get; }

		public IDocumentHistoryClient DocumentHistory { get; }

		public IJsonSchemaClient JsonSchemas { get; }

		public IListItemClient ListItems { get; }

		public IPermissionClient Permissions { get; }

		public IPublicAccessClient PublicAccess { get; }

		public IShareClient Shares { get; }

		public ITransferClient Transfers { get; }

		public IUserClient Users { get; }

		public void Dispose()
		{
		}
	}
}
