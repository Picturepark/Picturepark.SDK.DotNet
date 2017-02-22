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
            Assets = new AssetsClient(baseUrl, authClient);
            BusinessProcesses = new BusinessProcessesClient(baseUrl, authClient);
            DocumentHistory = new DocumentHistoryClient(baseUrl, authClient);
            JsonSchemas = new JsonSchemasClient(baseUrl, authClient);
            Permissions = new PermissionsClient(baseUrl, authClient);
            PublicAccess = new PublicAccessClient(baseUrl, authClient);
            Shares = new SharesClient(baseUrl, authClient);
            Users = new UsersClient(baseUrl, authClient);
            Schemas = new MetadataSchemasClient((BusinessProcessesClient)BusinessProcesses, authClient);
            Transfers = new TransfersClient((BusinessProcessesClient)BusinessProcesses, authClient);
            MetadataObjects = new MetadataObjectsClient((TransfersClient)Transfers, authClient);
        }

        public IMetadataSchemasClient Schemas { get; }

        public IAssetsClient Assets { get; }

        public IBusinessProcessesClient BusinessProcesses { get; }

        public IDocumentHistoryClient DocumentHistory { get; }

        public IJsonSchemasClient JsonSchemas { get; }

        public IMetadataObjectsClient MetadataObjects { get; }

        public IPermissionsClient Permissions { get; }

        public IPublicAccessClient PublicAccess { get; }

        public ISharesClient Shares { get; }

        public ITransfersClient Transfers { get; }

        public IUsersClient Users { get; }

        public void Dispose()
        {
        }
    }
}
