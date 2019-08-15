using System;

namespace Picturepark.ServiceProvider.Example.BusinessProcess.Config
{
    public class SampleConfiguration
    {
        public string ApiUrl { get; set; }

        public string CustomerAlias { get; set; }

        public string AccessToken { get; set; }

        public string ServiceProviderId { get; set; }

        public string IntegrationHost { get; set; }

        public int IntegrationPort { get; set; }

        public bool UseSsl { get; set; }

        public string Secret { get; set; }

        public string TriggeringBusinessRuleId { get; set; }

        public int BatchSize { get; set; }

        public TimeSpan InactivityTimeout { get; set; }

        public string OutputDownloadDirectory { get; set; }
    }
}