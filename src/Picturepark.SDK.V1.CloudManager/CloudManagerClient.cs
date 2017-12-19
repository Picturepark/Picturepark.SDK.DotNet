using System;
using Picturepark.SDK.V1.Contract;
using System.Net.Http;

namespace Picturepark.SDK.V1.CloudManager
{
	public class CloudManagerClient : IDisposable
	{
		private HttpClient _httpClient;

		/// <summary>Initializes a new instance of the <see cref="CloudManagerClient"/> class and uses the <see cref="IPictureparkClientSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
		/// <param name="settings">The client settings.</param>
		public CloudManagerClient(ICloudManagerClientSettings settings)
		{
			_httpClient = new HttpClient();
			_httpClient.Timeout = settings.HttpTimeout;

			Initialize(settings, _httpClient);
		}

		/// <summary>Initializes a new instance of the <see cref="CloudManagerClient"/> class and uses the <see cref="IPictureparkClientSettings.BaseUrl"/> of the <paramref name="settings"/> as Picturepark server URL.</summary>
		/// <param name="settings">The client settings.</param>
		/// <param name="httpClient">The HTTP client.</param>
		public CloudManagerClient(ICloudManagerClientSettings settings, HttpClient httpClient)
		{
			Initialize(settings, httpClient);
		}

		public CustomerClient Customers { get; private set; }

		public ServiceClient Services { get; private set; }

		public UpdateClient Updates { get; private set; }

		public SampleDataClient SampleData { get; private set; }

		public void Dispose()
		{
			if (_httpClient != null)
			{
				_httpClient.Dispose();
				_httpClient = null;
			}
		}

		private void Initialize(ICloudManagerClientSettings settings, HttpClient httpClient)
		{
			Customers = new CustomerClient(settings, httpClient);
			Services = new ServiceClient(settings, httpClient);
			Updates = new UpdateClient(settings, httpClient);
			SampleData = new SampleDataClient(settings, httpClient);
		}
	}
}
