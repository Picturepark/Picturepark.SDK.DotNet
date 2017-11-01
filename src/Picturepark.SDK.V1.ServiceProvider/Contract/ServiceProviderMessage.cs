using System;
using System.Collections.Generic;
using System.Text;

namespace Picturepark.SDK.V1.ServiceProvider.Contract
{
	public class ServiceProviderMessage
	{
		public string ServiceProviderId { get; set; }

		public string MessageName { get; set; }

		public dynamic Variables { get; set; }

		public DateTime Timestamp { get; set; }

		public string BusinessProcessId { get; set; }

		public string Id { get; set; }

		public string CorrelationId { get; set; }

		public string ContractVersion { get; set; }

		public string CustomerId { get; set; }

		public string TraceJobId { get; set; }

		public int Retries { get; set; }

		public int Priority { get; set; }
	}
}
