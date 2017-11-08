using System;
using Picturepark.SDK.V1.Contract;
using System.Net.Http;

namespace Picturepark.SDK.V1.CloudManager
{
	public class CloudManagerClient : IDisposable
	{
		private HttpClient _httpClient;

		public CloudManagerClient(IPictureparkClientSettings settings)
		{
			_httpClient = new HttpClient();
			Initialize(settings, _httpClient);
		}

		public CloudManagerClient(IPictureparkClientSettings settings, HttpClient httpClient)
		{
			Initialize(settings, httpClient);
		}

		public CustomerClient Customers { get; private set; }

		public ServiceClient Services { get; private set; }

		public UpdateClient Updates { get; private set; }

		public void Dispose()
		{
			if (_httpClient != null)
			{
				_httpClient.Dispose();
				_httpClient = null;
			}
		}

		private void Initialize(IPictureparkClientSettings settings, HttpClient httpClient)
		{
			Customers = new CustomerClient(settings, httpClient);
			Services = new ServiceClient(settings, httpClient);
			Updates = new UpdateClient(settings, httpClient);
		}
	}
}
