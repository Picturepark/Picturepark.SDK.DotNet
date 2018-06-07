using Microsoft.Extensions.Options;
using Picturepark.Microsite.PressPortal.Configuration;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract.Authentication;
using System;

namespace Picturepark.Microsite.PressPortal.Services
{
    public class PictureparkServiceClientSettings : IPictureparkServiceClientSettings
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
