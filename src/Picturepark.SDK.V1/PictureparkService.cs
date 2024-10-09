using System;
using Picturepark.SDK.V1.Contract;
using System.Net.Http;
using System.Net.Http.Headers;
using Picturepark.SDK.V1.Partial;

namespace Picturepark.SDK.V1
{
    public class PictureparkService : IPictureparkService
    {
        private HttpClient _httpClient;

        /// <summary>Initializes a new instance of the <see cref="PictureparkService"/> class and uses the <see cref="IPictureparkServiceSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
        /// <param name="settings">The service settings.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public PictureparkService(IPictureparkServiceSettings settings, HttpClient httpClient = null)
        {
            var version = typeof(PictureparkService).Assembly.GetName().Version.ToString();
            httpClient ??= _httpClient = new HttpClient(new PictureparkRetryHandler())
            {
                Timeout = settings.HttpTimeout,
                DefaultRequestHeaders = { UserAgent = { new ProductInfoHeaderValue("Picturepark.SDK.V1", version) } }
            };

            if (settings.IntegrationName != null)
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"({settings.IntegrationName})"));

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

        [Obsolete("Transfer API is deprecated and will be removed in future release. Please use Ingest API for new projects and consider switching existing code to use Ingest API as well.")]
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

        public IXmpMappingClient XmpMapping { get; private set; }

        public INotificationClient Notification { get; private set; }

        public ITemplateClient Template { get; private set; }

        public IStatisticClient Statistics { get; private set; }

        public IConversionPresetTemplateClient ConversionPresetTemplate { get; private set; }

        public IIngestClient Ingest { get; private set; }

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
            #pragma warning disable CS0618 // Type or member is obsolete
            Transfer = new TransferClient(BusinessProcess, settings, httpClient);
            #pragma warning restore CS0618 // Type or member is obsolete
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
            XmpMapping = new XmpMappingClient(settings, httpClient);
            Notification = new NotificationClient(settings, httpClient);
            Template = new TemplateClient(settings, httpClient);
            Statistics = new StatisticClient(settings, httpClient);
            ConversionPresetTemplate = new ConversionPresetTemplateClient(settings, httpClient);
            Ingest = new InternalIngestClient(BusinessProcess, Content, settings, httpClient);
        }
    }
}
