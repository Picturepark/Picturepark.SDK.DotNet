using System;
using Picturepark.SDK.V1.Contract;
using System.Net.Http;

namespace Picturepark.SDK.V1
{
    public class PictureparkClient : IDisposable, IPictureparkClient
    {
        private HttpClient _httpClient;

        /// <summary>Initializes a new instance of the <see cref="PictureparkClient"/> class and uses the <see cref="IPictureparkClientSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
        /// <param name="settings">The client settings.</param>
        public PictureparkClient(IPictureparkClientSettings settings)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = settings.HttpTimeout;

            Initialize(settings, _httpClient);
        }

        /// <summary>Initializes a new instance of the <see cref="PictureparkClient"/> class and uses the <see cref="IPictureparkClientSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
        /// <param name="settings">The client settings.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public PictureparkClient(IPictureparkClientSettings settings, HttpClient httpClient)
        {
            Initialize(settings, httpClient);
        }

        public ISchemaClient Schemas { get; private set; }

        public IContentClient Contents { get; private set; }

        public IOutputClient Outputs { get; private set; }

        public IBusinessProcessClient BusinessProcesses { get; private set; }

        public IDocumentHistoryClient DocumentHistory { get; private set; }

        public IJsonSchemaClient JsonSchemas { get; private set; }

        public IListItemClient ListItems { get; private set; }

        public IPermissionClient Permissions { get; private set; }

        public IPublicAccessClient PublicAccess { get; private set; }

        public IShareClient Shares { get; private set; }

        public ITransferClient Transfers { get; private set; }

        public IUserClient Users { get; private set; }

        public IProfileClient Profile { get; private set; }

        public ISchemaTransferClient SchemaTransfer { get; private set; }

        public IServiceProviderClient ServiceProviders { get; private set; }

        public IInfoClient Info { get; private set; }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }

        private void Initialize(IPictureparkClientSettings settings, HttpClient httpClient)
        {
            Outputs = new OutputClient(settings, httpClient);
            Contents = new ContentClient(settings, httpClient);
            BusinessProcesses = new BusinessProcessClient(settings, httpClient);
            DocumentHistory = new DocumentHistoryClient(settings, httpClient);
            JsonSchemas = new JsonSchemaClient(settings, httpClient);
            Permissions = new PermissionClient(settings, httpClient);
            PublicAccess = new PublicAccessClient(settings, httpClient);
            Shares = new ShareClient(settings, httpClient);
            Users = new UserClient(settings, httpClient);
            Info = new InfoClient(settings, httpClient);
            Schemas = new SchemaClient((BusinessProcessClient)BusinessProcesses, (InfoClient)Info, settings, httpClient);
            Transfers = new TransferClient((BusinessProcessClient)BusinessProcesses, settings, httpClient);
            ListItems = new ListItemClient((BusinessProcessClient)BusinessProcesses, settings, httpClient);
            Profile = new ProfileClient(settings, httpClient);
            ServiceProviders = new ServiceProviderClient(settings, httpClient);
            SchemaTransfer = new SchemaTransferClient(settings, httpClient);
        }
    }
}
