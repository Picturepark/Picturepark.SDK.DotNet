using Newtonsoft.Json;
using Picturepark.SDK.V1.CloudManager.Contract;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace Picturepark.SDK.V1.CloudManager.Tests.Fixtures
{
    public class ClientFixture : IDisposable
    {
        private readonly CloudManagerClient _client;
        private readonly TestConfiguration _configuration;

        public ClientFixture()
        {
            ProjectDirectory = Path.GetFullPath(Path.GetDirectoryName(typeof(ClientFixture).GetTypeInfo().Assembly.Location) + "/../../../");

#if NET452
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls;
#endif

            // Fix
            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory += "../";

            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory = Directory.GetCurrentDirectory() + "/../../../";

            if (!Directory.Exists(TempDirectory))
                Directory.CreateDirectory(TempDirectory);

            var configurationJson = File.ReadAllText(ProjectDirectory + "Configuration.json");
            _configuration = JsonConvert.DeserializeObject<TestConfiguration>(configurationJson);

            _client = new CloudManagerClient(
                new CloudManagerClientSettings(_configuration.Server)
            );
        }

        public string ProjectDirectory { get; }

        public string TempDirectory => ProjectDirectory + "/Temp";

        public TestConfiguration Configuration => _configuration;

        public CloudManagerClient Client => _client;

        public void Dispose()
        {
            _client.Dispose();
        }

        public ServiceProviderCreateRequest CreateSampleProviderRequest()
        {
            var randomId = Guid.NewGuid().ToString();

            return new ServiceProviderCreateRequest()
            {
                ExternalId = $"acme-{randomId}",
                Name = $"ACME 3.14 - {randomId}",
                MessageQueueUser = $"acme-{randomId}",
                MessageQueuePassword = randomId,
                BaseUrl = $"http://{randomId}"
            };
        }
    }
}
