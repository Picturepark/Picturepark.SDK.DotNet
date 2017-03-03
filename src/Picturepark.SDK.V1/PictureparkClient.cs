using System;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
    public class PictureparkClient : IDisposable, IPictureparkClient
    {
        /// <summary>Initializes a new instance of the <see cref="PictureparkClient"/> class and uses the <see cref="IAuthClient.BaseUrl"/> of the <paramref name="authClient"/> as Picturepark server URL.</summary>
        /// <param name="authClient">The authentication client.</param>
        public PictureparkClient(IAuthClient authClient)
            : this(authClient.BaseUrl, authClient)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PictureparkClient"/> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="authClient">The authentication client.</param>
        public PictureparkClient(string baseUrl, IAuthClient authClient)
        {
            Contents = new ContentClient(baseUrl, authClient);
            BusinessProcesses = new BusinessProcessClient(baseUrl, authClient);
            DocumentHistory = new DocumentHistoryClient(baseUrl, authClient);
            JsonSchemas = new JsonSchemaClient(baseUrl, authClient);
            Permissions = new PermissionClient(baseUrl, authClient);
            PublicAccess = new PublicAccessClient(baseUrl, authClient);
            Shares = new ShareClient(baseUrl, authClient);
            Users = new UserClient(baseUrl, authClient);
            Schemas = new SchemaClient((BusinessProcessClient)BusinessProcesses, authClient);
            Transfers = new TransferClient((BusinessProcessClient)BusinessProcesses, authClient);
            ListItems = new ListItemClient((TransferClient)Transfers, authClient);
        }

        public ISchemaClient Schemas { get; }

        public IContentClient Contents { get; }

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
