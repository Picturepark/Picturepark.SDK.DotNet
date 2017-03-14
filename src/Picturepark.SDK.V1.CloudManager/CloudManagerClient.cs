using System;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.CloudManager
{
	public class CloudManagerClient : IDisposable
	{
		public CloudManagerClient(IPictureparkClientSettings settings)
		{
			Customers = new CustomersClient(settings);
			Services = new ServicesClient(settings);
		}

		public CustomersClient Customers { get; }

		public ServicesClient Services { get; }

		public void Dispose()
		{
		}
	}
}
