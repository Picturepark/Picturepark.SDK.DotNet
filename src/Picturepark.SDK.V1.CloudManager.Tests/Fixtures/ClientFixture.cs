using Newtonsoft.Json;
using Picturepark.SDK.V1.CloudManager.Contract;
using System;
using System.IO;
using System.Reflection;

namespace Picturepark.SDK.V1.CloudManager.Tests.Fixtures
{
    public class ClientFixture : IDisposable
    {
        private readonly CloudManagerService _client;
        private readonly TestConfiguration _configuration;

        public ClientFixture()
        {
            ProjectDirectory = Path.GetFullPath(Path.GetDirectoryName(typeof(ClientFixture).GetTypeInfo().Assembly.Location) + "/../../../");

            var assemblyDirectory = Path.GetFullPath(Path.GetDirectoryName(typeof(ClientFixture).GetTypeInfo().Assembly.Location));
            ProjectDirectory = Path.GetFullPath(assemblyDirectory + "/../../../");

            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory = Path.GetFullPath(ProjectDirectory + "../");

            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../../../");

            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../../../../");

            var configurationJson = File.ReadAllText(ProjectDirectory + "Configuration.json");
            _configuration = JsonConvert.DeserializeObject<TestConfiguration>(configurationJson);

            _client = new CloudManagerService(
                new CloudManagerServiceSettings(_configuration.Server)
            );
        }

        public string ProjectDirectory { get; }

        public string TempDirectory => ProjectDirectory + "/Temp";

        public TestConfiguration Configuration => _configuration;

        public CloudManagerService Client => _client;

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
                Secret = randomId,
                BaseUrl = $"http://{randomId}"
            };
        }
    }
}
