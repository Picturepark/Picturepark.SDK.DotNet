﻿using System;
using Picturepark.SDK.V1.Contract;
using System.Net.Http;
using Picturepark.SDK.V1.CloudManager.Contract;
using IOutputFormatClient = Picturepark.SDK.V1.CloudManager.Contract.IOutputFormatClient;

namespace Picturepark.SDK.V1.CloudManager
{
    public class CloudManagerService : IDisposable
    {
        private HttpClient _httpClient;

        /// <summary>Initializes a new instance of the <see cref="CloudManagerService"/> class and uses the <see cref="IPictureparkServiceSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
        /// <param name="settings">The service settings.</param>
        public CloudManagerService(ICloudManagerServiceSettings settings)
        {
            _httpClient = new HttpClient { Timeout = settings.HttpTimeout };

            Initialize(settings, _httpClient);
        }

        /// <summary>Initializes a new instance of the <see cref="CloudManagerService"/> class and uses the <see cref="IPictureparkServiceSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
        /// <param name="settings">The service settings.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public CloudManagerService(ICloudManagerServiceSettings settings, HttpClient httpClient)
        {
            Initialize(settings, httpClient);
        }

        public ICustomerClient Customer { get; private set; }

        public IServiceClient Service { get; private set; }

        public IUpdateClient Update { get; private set; }

        public ICloudBackupClient CloudBackup { get; private set; }

        public ICustomerServiceProviderClient CustomerServiceProvider { get; private set; }

        public IEnvironmentClient Environment { get; private set; }

        public IEnvironmentProcessClient EnvironmentProcess { get; private set; }

        public IMaintenanceClient Maintenance { get; private set; }

        public IGlobalConfigurationClient GlobalConfiguration { get; private set; }

        public IOutputFormatClient OutputFormat { get; private set; }

        public ICustomerAssetClient CustomerAsset { get; private set; }

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
            Customer = new CustomerClient(settings, httpClient);
            Service = new ServiceClient(settings, httpClient);
            Update = new UpdateClient(settings, httpClient);
            CloudBackup = new CloudBackupClient(settings, httpClient);
            EnvironmentProcess = new EnvironmentProcessClient(settings, httpClient);
            Environment = new EnvironmentClient(settings, httpClient);
            CustomerServiceProvider = new CustomerServiceProviderClient(settings, httpClient);
            Maintenance = new MaintenanceClient(settings, httpClient);
            GlobalConfiguration = new GlobalConfigurationClient(settings, httpClient);
            OutputFormat = new OutputFormatClient(settings, httpClient);
            CustomerAsset = new CustomerAssetClient(settings, httpClient);
        }
    }
}
