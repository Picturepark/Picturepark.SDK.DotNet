using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Picturepark.Microsite.Example.Configuration;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.Microsite.Example.Services
{
	public class PictureparkServiceClientSettings: IPictureparkServiceClientSettings
	{
		public PictureparkServiceClientSettings(IOptions<PictureparkConfiguration> config)
		{
			var auth = new AccessTokenAuthClient(config.Value.BaseUrl, config.Value.AccessToken, config.Value.CustomerAlias);
			AuthClient = auth;
			BaseUrl = config.Value.BaseUrl;
			CustomerAlias = config.Value.CustomerAlias;
			HttpTimeout = TimeSpan.FromMinutes(10);
		}

		public string BaseUrl { get; }

		public TimeSpan HttpTimeout { get; }

		public IAuthClient AuthClient { get; }

		public string CustomerAlias { get; }
	}
}
