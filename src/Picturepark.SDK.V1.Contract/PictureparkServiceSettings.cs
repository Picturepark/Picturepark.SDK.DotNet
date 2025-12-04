using System;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.Contract
{
    /// <summary>The Fotoware Alto service settings.</summary>
    public class PictureparkServiceSettings : IPictureparkServiceSettings
    {
        /// <summary>Initializes a new instance of the <see cref="PictureparkServiceSettings"/> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="customerAlias">The customer alias.</param>
        [Obsolete("Use the variant with integrationName parameter.")]
        public PictureparkServiceSettings(string baseUrl, string customerAlias)
        {
            BaseUrl = baseUrl;
            HttpTimeout = TimeSpan.FromMinutes(1);
            CustomerAlias = customerAlias;
        }

        /// <summary>Initializes a new instance of the <see cref="PictureparkServiceSettings"/> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="customerAlias">The customer alias.</param>
        /// <param name="integrationName">The integration name.</param>
        public PictureparkServiceSettings(string baseUrl, string customerAlias, string integrationName)
        {
            if (string.IsNullOrWhiteSpace(integrationName))
                throw new ArgumentException($"Please specify name of your integration in {nameof(integrationName)} parameter");

            BaseUrl = baseUrl;
            HttpTimeout = TimeSpan.FromMinutes(1);
            CustomerAlias = customerAlias;
            IntegrationName = integrationName;
        }

        /// <summary>Initializes a new instance of the <see cref="PictureparkServiceSettings"/> class.</summary>
        /// <param name="authClient">The authentication client.</param>
        [Obsolete("Use the variant with integrationName parameter.")]
        public PictureparkServiceSettings(IAuthClient authClient) : this(authClient.BaseUrl, authClient.CustomerAlias)
        {
            AuthClient = authClient;
        }

        /// <summary>Initializes a new instance of the <see cref="PictureparkServiceSettings"/> class.</summary>
        /// <param name="authClient">The authentication client.</param>
        /// <param name="integrationName">The integration name.</param>
        public PictureparkServiceSettings(IAuthClient authClient, string integrationName)
            : this(authClient.BaseUrl, authClient.CustomerAlias, integrationName)
        {
            AuthClient = authClient;
        }

        /// <summary>Initializes a new instance of the <see cref="PictureparkServiceSettings" /> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="authClient">The authentication client.</param>
        /// <param name="customerAlias">The customer alias.</param>
        [Obsolete("Use the variant with integrationName parameter.")]
        public PictureparkServiceSettings(string baseUrl, IAuthClient authClient, string customerAlias)
            : this(baseUrl, customerAlias)
        {
            AuthClient = authClient;
        }

        /// <summary>Initializes a new instance of the <see cref="PictureparkServiceSettings" /> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="authClient">The authentication client.</param>
        /// <param name="customerAlias">The customer alias.</param>
        /// <param name="integrationName">The integration name.</param>
        public PictureparkServiceSettings(string baseUrl, IAuthClient authClient, string customerAlias, string integrationName)
            : this(baseUrl, customerAlias, integrationName)
        {
            AuthClient = authClient;
        }

        /// <summary>Gets the server URL of the Fotoware Alto server.</summary>
        public string BaseUrl { get; set; }

        /// <summary>Gets or sets the HTTP timeout.</summary>
        public TimeSpan HttpTimeout { get; set; }

        /// <summary>Gets or sets the <see cref="IAuthClient"/>.</summary>
        public IAuthClient AuthClient { get; set; }

        /// <summary>Gets the customer alias.</summary>
        public string CustomerAlias { get; set; }

        /// <summary>Gets or sets the display language.</summary>
        public string DisplayLanguage { get; set; }

        /// <summary>Get the integration name.</summary>
        public string IntegrationName { get; set; }
    }
}
