using System;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.CloudManager
{
    /// <summary>The Fotoware Alto service settings interface.</summary>
    public interface ICloudManagerServiceSettings
    {
        /// <summary>Gets the server URL of the Fotoware Alto authentication server.</summary>
        string BaseUrl { get; }

        /// <summary>Gets the HTTP timeout.</summary>
        TimeSpan HttpTimeout { get; }

        /// <summary>Gets the <see cref="IAuthClient"/>.</summary>
        IAuthClient AuthClient { get; }
    }
}