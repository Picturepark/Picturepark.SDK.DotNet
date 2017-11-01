using System;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.CloudManager
{
	public class CloudManagerClient : IDisposable
	{
		public CloudManagerClient(IPictureparkClientSettings settings)
		{
			Customers = new CustomerClient(settings);
			Services = new ServiceClient(settings);
			Updates = new UpdateClient(settings);
		}

		public CustomerClient Customers { get; }

		public ServiceClient Services { get; }

		public UpdateClient Updates { get; }

		public void Dispose()
		{
		}
	}
}
