using System;
using Picturepark.SDK.V1.ServiceProvider;

namespace Picturepark.ServiceProvider.Example
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Acme();
		}

		private static void Acme()
		{
			var configuration = new Configuration()
			{
				Host = "localhost",
				Port = "5672",
				ServiceProviderId = "acme",
				NodeId = Environment.MachineName,
				User = "acme",
				Password = "123456"
			};

			using (IDisposable d1 = new MinimalLiveStreamDemo().Run(configuration))
			{
				Console.ReadKey();
			}
		}
	}
}
