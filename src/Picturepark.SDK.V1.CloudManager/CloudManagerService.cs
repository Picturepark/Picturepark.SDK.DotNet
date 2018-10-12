using System;
using Picturepark.SDK.V1.Contract;
using System.Net.Http;
using Picturepark.SDK.V1.CloudManager.Contract;
using IServiceProviderClient = Picturepark.SDK.V1.CloudManager.Contract.IServiceProviderClient;

namespace Picturepark.SDK.V1.CloudManager
{
    public class CloudManagerService : IDisposable
    {
        private HttpClient _httpClient;

        /// <summary>Initializes a new instance of the <see cref="CloudManagerService"/> class and uses the <see cref="IPictureparkServiceSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
        /// <param name="settings">The service settings.</param>
        public CloudManagerService(ICloudManagerServiceSettings settings)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = settings.HttpTimeout;

            Initialize(settings, _httpClient);
        }

        /// <summary>Initializes a new instance of the <see cref="CloudManagerService"/> class and uses the <see cref="IPictureparkServiceSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
        /// <param name="settings">The service settings.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public CloudManagerService(ICloudManagerServiceSettings settings, HttpClient httpClient)
        {
            Initialize(settings, httpClient);
        }

        public ICustomerClient Customers { get; private set; }

        public IServiceClient Services { get; private set; }

        public IUpdateClient Updates { get; private set; }

        public ISampleDataClient SampleData { get; private set; }

        public ICloudBackupClient CloudBackup { get; private set; }

        public IServiceProviderClient ServiceProvider { get; private set; }

        public ICustomerServiceProviderClient CustomerServiceProvider { get; private set; }

        public IEnvironmentClient Environment { get; private set; }

        public IEnvironmentProcessClient EnvironmentProcess { get; private set; }

        public IContentBackupClient ContentBackup { get; private set; }

        public IMaintenanceClient MaintenanceClient { get; private set; }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }

        private void Initialize(ICloudManagerServiceSettings settings, HttpClient httpClient)
        {
            Customers = new CustomerClient(settings, httpClient);
            Services = new ServiceClient(settings, httpClient);
            Updates = new UpdateClient(settings, httpClient);
            SampleData = new SampleDataClient(settings, httpClient);
            CloudBackup = new CloudBackupClient(settings, httpClient);
            EnvironmentProcess = new EnvironmentProcessClient(settings, httpClient);
            Environment = new EnvironmentClient(settings, httpClient);
            ServiceProvider = new ServiceProviderClient(settings, httpClient);
            CustomerServiceProvider = new CustomerServiceProviderClient(settings, httpClient);
            ContentBackup = new ContentBackupClient(settings, httpClient);
            MaintenanceClient = new MaintenanceClient(settings, httpClient);
        }
    }
}
