using log4net;
using Picturepark.SDK.V1.ServiceProvider;
using System;

namespace Picturepark.SDK.V1.ServiceProvider.Example
{
	public class Program
	{
		private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Main(string[] args)
		{
			s_logger.Debug("Service Provider Sample");

			Acme();
		}

		private static void Acme()
		{
			////var configuration = new Configuration()
			////{
			////	Host = "10.1.8.226", //// RabbitMQ on devnext
			////	Port = "5672",
			////	ServiceProviderId = "acme",
			////	NodeId = Environment.MachineName,
			////	User = "acme",
			////	Password = "123456"
			////};

			var configuration = new Configuration()
			{
				Host = "localhost",
				Port = "5672",
				ServiceProviderId = "clarifyextractor",
				NodeId = Environment.MachineName,
				User = "clarify",
				Password = "123456"
			};

			using (IDisposable d1 = new MinimalLiveStreamDemo().Run(configuration))
			{
				Console.ReadKey();
			}
		}
	}
}
