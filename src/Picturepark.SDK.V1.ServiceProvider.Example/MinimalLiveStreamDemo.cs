using Picturepark.SDK.V1.ServiceProvider.Contract;
using System;
using System.Reactive.Linq;

namespace Picturepark.SDK.V1.ServiceProvider.Example
{
	public class MinimalLiveStreamDemo : IDisposable
	{
		private ServiceProviderClient _client;

		public void Dispose()
		{
			_client.Dispose();
		}

		public IDisposable Run(Configuration configuration)
		{
			_client = new Picturepark.SDK.V1.ServiceProvider.ServiceProviderClient(configuration);

			////var serviceProviderClient = _client.GetConfigurationClient("http://localhost:8080", "picturepark.admin@acme.xxx", "cGljdHVyZXBhcmsuYWRtaW5AYWNtZS54eHg=");
			////var config = serviceProviderClient.GetConfigurationAsync("clarifyextractor").Result;
			////config = serviceProviderClient.UpdateConfigurationAsync("clarifyextractor", new Contract.ServiceProviderConfigurationUpdateRequest
			////{
			////	CustomerId = "bro",
			////	Configuration = "Blah",
			////	ServiceProviderId = "clarifyextractor"
			////}).Result;

			// using a buffer of 1s and a buffer hold back delay of 3s
			var observer = _client.GetLiveStreamObserver(500, 5000);

			// all handler
			observer.Subscribe(a =>
			{
				Console.WriteLine($"All-Handler: {a.EventArgs.Message.Id} : {a.EventArgs.Message.Timestamp} : {a.EventArgs.Message.Scope}");

				System.Threading.Thread.Sleep(10);

				a.EventArgs.Ack();
			});

			return this;
		}
	}
}
