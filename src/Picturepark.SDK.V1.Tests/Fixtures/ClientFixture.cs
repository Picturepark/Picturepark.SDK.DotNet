using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ClientFixture : IDisposable
    {
        private static readonly ConnectionIssuesHandler s_httpHandler;

        private readonly IPictureparkService _client;
        private readonly TestConfiguration _configuration;

        static ClientFixture()
        {
            s_httpHandler = new ConnectionIssuesHandler(new PictureparkRetryHandler());
        }

        public ClientFixture()
        {
#if NET452
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls;
#endif

            var assemblyDirectory = Path.GetFullPath(Path.GetDirectoryName(typeof(ClientFixture).GetTypeInfo().Assembly.Location));
            ProjectDirectory = Path.GetFullPath(assemblyDirectory + "/../../../");

            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory = Path.GetFullPath(ProjectDirectory + "../");

            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../../../");

            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../../../../");

            if (!Directory.Exists(TempDirectory))
                Directory.CreateDirectory(TempDirectory);

            var configurationJson = File.ReadAllText(ProjectDirectory + "Configuration.json");
            _configuration = JsonConvert.DeserializeObject<TestConfiguration>(configurationJson);

            _client = GetLocalizedPictureparkService("en");
        }

        public string ProjectDirectory { get; }

        public string TempDirectory => ProjectDirectory + "/Temp";

        public string ExampleFilesBasePath => ProjectDirectory + "/ExampleData/Pool";

        public string ExampleSchemaBasePath => ProjectDirectory + "/ExampleData/Schema";

        public TestConfiguration Configuration => _configuration;

        public IPictureparkService Client => _client;

        public Lazy<CustomerInfo> CustomerInfo =>
            new Lazy<CustomerInfo>(() => _client.Info.GetInfoAsync().GetAwaiter().GetResult());

        public string DefaultLanguage => CustomerInfo.Value.LanguageConfiguration.DefaultLanguage;

        public async Task<ContentSearchResult> GetRandomContentsAsync(string searchString, int limit, IReadOnlyList<ContentType> contentTypes = null)
        {
            return await RandomHelper.GetRandomContentsAsync(_client, searchString, limit, contentTypes);
        }

        public async Task<string> GetRandomContentIdAsync(string searchString, int limit)
        {
            return await RandomHelper.GetRandomContentIdAsync(_client, searchString, limit);
        }

        public async Task<string> GetRandomContentPermissionSetIdAsync(int limit)
        {
            return await RandomHelper.GetRandomContentPermissionSetIdAsync(_client, limit);
        }

        public async Task<string> GetRandomSchemaPermissionSetIdAsync(int limit)
        {
            return await RandomHelper.GetRandomSchemaPermissionSetIdAsync(_client, limit);
        }

        public virtual void Dispose()
        {
            _client.Dispose();
        }

        public PictureparkService GetLocalizedPictureparkService(string language)
        {
            var authClient = new AccessTokenAuthClient(_configuration.Server, _configuration.AccessToken, _configuration.CustomerAlias);

            var settings = new PictureparkServiceSettings(authClient)
            {
                DisplayLanguage = language,
                HttpTimeout = TimeSpan.FromMinutes(5)
            };

            var httpClient = new HttpClient(s_httpHandler) { Timeout = settings.HttpTimeout };

            return new PictureparkService(settings, httpClient);
        }
    }
}
