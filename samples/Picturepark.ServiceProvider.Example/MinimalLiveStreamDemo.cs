using System;
using Picturepark.SDK.V1.ServiceProvider;

namespace Picturepark.ServiceProvider.Example
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
			_client = new ServiceProviderClient(configuration);

			// using a buffer of 1s and a buffer hold back delay of 5s
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
