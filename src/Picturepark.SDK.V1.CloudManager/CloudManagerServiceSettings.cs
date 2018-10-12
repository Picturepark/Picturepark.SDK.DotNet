using System;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.CloudManager
{
    /// <summary>The Picturepark service settings.</summary>
    public class CloudManagerServiceSettings : ICloudManagerServiceSettings
    {
        /// <summary>Initializes a new instance of the <see cref="CloudManagerServiceSettings"/> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        public CloudManagerServiceSettings(string baseUrl)
        {
            BaseUrl = baseUrl;
            HttpTimeout = TimeSpan.FromMinutes(1);
        }

        /// <summary>Initializes a new instance of the <see cref="CloudManagerServiceSettings"/> class.</summary>
        /// <param name="authClient">The authentication client.</param>
        public CloudManagerServiceSettings(IAuthClient authClient)
            : this(authClient.BaseUrl)
        {
            AuthClient = authClient;
        }

        /// <summary>Initializes a new instance of the <see cref="CloudManagerServiceSettings" /> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="authClient">The authentication client.</param>
        public CloudManagerServiceSettings(string baseUrl, IAuthClient authClient)
            : this(baseUrl)
        {
            AuthClient = authClient;
        }

        /// <summary>Gets the server URL of the Picturepark server.</summary>
        public string BaseUrl { get; set; }

        /// <summary>Gets or sets the HTTP timeout.</summary>
        public TimeSpan HttpTimeout { get; set; }

        /// <summary>Gets or sets the <see cref="IAuthClient"/>.</summary>
        public IAuthClient AuthClient { get; set; }
    }
}
