using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Helpers;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ClientFixture : IDisposable
    {
        private static readonly ConnectionIssuesHandler s_httpHandler;

        private readonly Lazy<CustomerInfo> _customerInfo;
        private readonly ConcurrentQueue<string> _createdUserIds = new ConcurrentQueue<string>();

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

            Configuration = JsonConvert.DeserializeObject<TestConfiguration>(configurationJson);
            Client = GetLocalizedPictureparkService("en");
            _customerInfo = new Lazy<CustomerInfo>(() => Client.Info.GetInfoAsync().GetAwaiter().GetResult());

            ContentPermissions = new ContentPermissionSetsEntityCreator(Client);
            Users = new UsersEntityCreator(Client);
        }

        public string ProjectDirectory { get; }

        public string TempDirectory => ProjectDirectory + "/Temp";

        public string ExampleFilesBasePath => ProjectDirectory + "/ExampleData/Pool";

        public string ExampleSchemaBasePath => ProjectDirectory + "/ExampleData/Schema";

        public TestConfiguration Configuration { get; }

        public IPictureparkService Client { get; }

        public CustomerInfo CustomerInfo => _customerInfo.Value;

        public string DefaultLanguage => CustomerInfo.LanguageConfiguration.DefaultLanguage;

        public ContentPermissionSetsEntityCreator ContentPermissions { get; }

        public UsersEntityCreator Users { get; }

        public async Task<ContentSearchResult> GetRandomContentsAsync(string searchString, int limit, IReadOnlyList<ContentType> contentTypes = null)
        {
            return await RandomHelper.GetRandomContentsAsync(Client, searchString, limit, contentTypes);
        }

        public async Task<string> GetRandomContentIdAsync(string searchString, int limit)
        {
            return await RandomHelper.GetRandomContentIdAsync(Client, searchString, limit);
        }

        public virtual void Dispose()
        {
            if (!_createdUserIds.IsEmpty)
            {
                foreach (var createdUserId in _createdUserIds)
                {
                    Client.User.DeleteAsync(createdUserId, new UserDeleteRequest()).GetAwaiter().GetResult();
                }
            }

            ContentPermissions.Dispose();
            Users.Dispose();
            Client.Dispose();
        }

        public PictureparkService GetLocalizedPictureparkService(string language)
        {
            var authClient = new AccessTokenAuthClient(Configuration.Server, Configuration.AccessToken, Configuration.CustomerAlias);

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
