using System;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.Contract
{
    /// <summary>The Fotoware Alto service settings interface.</summary>
    public interface IPictureparkServiceSettings
    {
        /// <summary>Gets the server URL of the Fotoware Alto authentication server.</summary>
        string BaseUrl { get; }

        /// <summary>Gets the HTTP timeout.</summary>
        TimeSpan HttpTimeout { get; }

        /// <summary>Gets the <see cref="IAuthClient"/>.</summary>
        IAuthClient AuthClient { get; }

        /// <summary>Gets the customer alias.</summary>
        string CustomerAlias { get; }

        /// <summary>Gets the display language.</summary>
        string DisplayLanguage { get; }

        /// <summary>Get the integration name.</summary>
        string IntegrationName { get; }
    }
}