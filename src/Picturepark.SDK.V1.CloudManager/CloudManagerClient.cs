using System;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.CloudManager
{
	public class CloudManagerClient : IDisposable
	{
		public CloudManagerClient(string baseUrl, IAuthClient authClient)
		{
			Customers = new CustomersClientBase(authClient) { BaseUrl = baseUrl };
			Services = new ServicesClientBase(authClient) { BaseUrl = baseUrl };
		}

		public CustomersClientBase Customers { get; }

		public ServicesClientBase Services { get; }

		public void Dispose()
		{
		}
	}
}
