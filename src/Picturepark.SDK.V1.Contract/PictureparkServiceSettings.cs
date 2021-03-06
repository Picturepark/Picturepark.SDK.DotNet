﻿using System;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.Contract
{
    /// <summary>The Picturepark service settings.</summary>
    public class PictureparkServiceSettings : IPictureparkServiceSettings
    {
        /// <summary>Initializes a new instance of the <see cref="PictureparkServiceSettings"/> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="customerAlias">The customaer alias.</param>
        public PictureparkServiceSettings(string baseUrl, string customerAlias)
        {
            BaseUrl = baseUrl;
            HttpTimeout = TimeSpan.FromMinutes(1);
            CustomerAlias = customerAlias;
        }

        /// <summary>Initializes a new instance of the <see cref="PictureparkServiceSettings"/> class.</summary>
        /// <param name="authClient">The authentication client.</param>
        public PictureparkServiceSettings(IAuthClient authClient)
            : this(authClient.BaseUrl, authClient.CustomerAlias)
        {
            AuthClient = authClient;
        }

        /// <summary>Initializes a new instance of the <see cref="PictureparkServiceSettings" /> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="authClient">The authentication client.</param>
        /// <param name="customerAlias">The customer alias.</param>
        public PictureparkServiceSettings(string baseUrl, IAuthClient authClient, string customerAlias)
            : this(baseUrl, customerAlias)
        {
            AuthClient = authClient;
        }

        /// <summary>Gets the server URL of the Picturepark server.</summary>
        public string BaseUrl { get; set; }

        /// <summary>Gets or sets the HTTP timeout.</summary>
        public TimeSpan HttpTimeout { get; set; }

        /// <summary>Gets or sets the <see cref="IAuthClient"/>.</summary>
        public IAuthClient AuthClient { get; set; }

        /// <summary>Gets the customer alias.</summary>
        public string CustomerAlias { get; set; }

        /// <summary>Gets or sets the display language.</summary>
        public string DisplayLanguage { get; set; }
    }
}
