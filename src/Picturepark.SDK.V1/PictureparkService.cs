using Picturepark.SDK.V1.Contract;
using System.Net.Http;

namespace Picturepark.SDK.V1
{
    public class PictureparkService : IPictureparkService
    {
        private HttpClient _httpClient;

        /// <summary>Initializes a new instance of the <see cref="PictureparkService"/> class and uses the <see cref="IPictureparkServiceSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
        /// <param name="settings">The service settings.</param>
        public PictureparkService(IPictureparkServiceSettings settings)
        {
            _httpClient = new HttpClient(new PictureparkRetryHandler()) { Timeout = settings.HttpTimeout };

            Initialize(settings, _httpClient);
        }

        /// <summary>Initializes a new instance of the <see cref="PictureparkService"/> class and uses the <see cref="IPictureparkServiceSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
        /// <param name="settings">The service settings.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public PictureparkService(IPictureparkServiceSettings settings, HttpClient httpClient)
        {
            Initialize(settings, httpClient);
        }

        public ISchemaClient Schema { get; private set; }

        public IContentClient Content { get; private set; }

        public IOutputClient Output { get; private set; }

        public IOutputFormatClient OutputFormat { get; private set; }

        public IBusinessProcessClient BusinessProcess { get; private set; }

        public IDocumentHistoryClient DocumentHistory { get; private set; }

        public IJsonSchemaClient JsonSchema { get; private set; }

        public IListItemClient ListItem { get; private set; }

        public ILiveStreamClient LiveStream { get; private set; }

        public IContentPermissionSetClient ContentPermissionSet { get; private set; }

        public ISchemaPermissionSetClient SchemaPermissionSet { get; private set; }

        public IShareClient Share { get; private set; }

        public ITransferClient Transfer { get; private set; }

        public IUserClient User { get; private set; }

        public IUserRoleClient UserRole { get; private set; }

        public IProfileClient Profile { get; private set; }

        public ISchemaTransferClient SchemaTransfer { get; private set; }

        public IInfoClient Info { get; private set; }

        public IChannelClient Channel { get; private set; }

        public IBusinessRuleClient BusinessRule { get; private set; }

        public IDisplayValueClient DisplayValue { get; private set; }

        public IMetadataClient Metadata { get; private set; }

        public IIdentityProviderClient IdentityProvider { get; private set; }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }

        private void Initialize(IPictureparkServiceSettings settings, HttpClient httpClient)
        {
            Output = new OutputClient(settings, httpClient);
            BusinessProcess = new BusinessProcessClient(settings, httpClient);
            DocumentHistory = new DocumentHistoryClient(settings, httpClient);
            JsonSchema = new JsonSchemaClient(settings, httpClient);
            ContentPermissionSet = new ContentPermissionSetClient(settings, httpClient);
            SchemaPermissionSet = new SchemaPermissionSetClient(settings, httpClient);
            Share = new ShareClient(settings, httpClient);
            User = new UserClient(settings, httpClient);
            UserRole = new UserRoleClient(settings, httpClient);
            Info = new InfoClient(settings, httpClient);
            Schema = new SchemaClient(Info, BusinessProcess, settings, httpClient);
            Transfer = new TransferClient(BusinessProcess, settings, httpClient);
            ListItem = new ListItemClient(BusinessProcess, settings, httpClient);
            LiveStream = new LiveStreamClient(settings, httpClient);
            Content = new ContentClient(BusinessProcess, settings, httpClient);
            Profile = new ProfileClient(settings, httpClient);
            SchemaTransfer = new SchemaTransferClient(settings, httpClient);
            Channel = new ChannelClient(settings, httpClient);
            BusinessRule = new BusinessRuleClient(settings, httpClient);
            OutputFormat = new OutputFormatClient(BusinessProcess, settings, httpClient);
            DisplayValue = new DisplayValueClient(settings, httpClient);
            Metadata = new MetadataClient(settings, httpClient);
            IdentityProvider = new IdentityProviderClient(settings, httpClient);
        }
    }
}
