using Picturepark.SDK.V1.ServiceProvider.Contract;
using System;

namespace Picturepark.SDK.V1.ServiceProvider
{
	public class ServiceProviderRequestEventArgs : EventArgs
	{
		public ServiceProviderRequestEventArgs(ServiceProviderMessage message)
		{
			Message = message;
		}

		public ServiceProviderMessage Message { get; }
	}
}
