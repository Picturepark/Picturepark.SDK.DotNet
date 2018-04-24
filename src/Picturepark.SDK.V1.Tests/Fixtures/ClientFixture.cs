using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ClientFixture : IDisposable
    {
        private readonly PictureparkClient _client;
        private readonly TestConfiguration _configuration;

        public ClientFixture()
        {
            ProjectDirectory = Path.GetFullPath(Path.GetDirectoryName(typeof(ClientFixture).GetTypeInfo().Assembly.Location) + "/../../../");

#if NET45
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls;
#endif

            // Fix
            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory += "../";

            if (!Directory.Exists(TempDirectory))
                Directory.CreateDirectory(TempDirectory);

            var configurationJson = File.ReadAllText(ProjectDirectory + "Configuration.json");
            _configuration = JsonConvert.DeserializeObject<TestConfiguration>(configurationJson);

            var authClient = new AccessTokenAuthClient(_configuration.Server, _configuration.AccessToken, _configuration.CustomerAlias);
            _client = new PictureparkClient(new PictureparkClientSettings(authClient));
        }

        public string ProjectDirectory { get; }

        public string TempDirectory => ProjectDirectory + "/Temp";

        public string ExampleFilesBasePath => ProjectDirectory + "/ExampleData/Pool";

        public string ExampleSchemaBasePath => ProjectDirectory + "/ExampleData/Schema";

        public TestConfiguration Configuration => _configuration;

        public PictureparkClient Client => _client;

        public async Task<ContentSearchResult> GetRandomContentsAsync(string searchString, int limit)
        {
            return await RandomHelper.GetRandomContentsAsync(_client, searchString, limit);
        }

        public async Task<string> GetRandomContentIdAsync(string searchString, int limit)
        {
            return await RandomHelper.GetRandomContentIdAsync(_client, searchString, limit);
        }

        public async Task<string> GetRandomContentPermissionSetIdAsync(int limit)
        {
            return await RandomHelper.GetRandomContentPermissionSetIdAsync(_client, limit);
        }

        public async Task<string> GetRandomTransferIdAsync(TransferState? transferState, int limit)
        {
            return await RandomHelper.GetRandomTransferIdAsync(_client, transferState, limit);
        }

        public async Task<string> GetRandomFileTransferIdAsync(int limit)
        {
            return await RandomHelper.GetRandomFileTransferIdAsync(_client, limit);
        }

        public async Task<string> GetRandomMetadataPermissionSetIdAsync(int limit)
        {
            return await RandomHelper.GetRandomMetadataPermissionSetIdAsync(_client, limit);
        }

        public async Task<string> GetRandomSchemaIdAsync(int limit)
        {
            return await RandomHelper.GetRandomSchemaIdAsync(_client, limit);
        }

        public async Task<string> GetRandomObjectIdAsync(string metadataSchemaId, int limit)
        {
            return await RandomHelper.GetRandomObjectIdAsync(_client, metadataSchemaId, limit);
        }

        public async Task<string> GetRandomShareIdAsync(ShareType shareType, int limit)
        {
            return await RandomHelper.GetRandomShareIdAsync(_client, shareType, limit);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
