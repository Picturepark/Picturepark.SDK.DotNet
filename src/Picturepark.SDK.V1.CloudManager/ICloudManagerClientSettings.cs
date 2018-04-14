using System;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.CloudManager
{
    /// <summary>The Picturepark client settings interface.</summary>
    public interface ICloudManagerClientSettings
    {
        /// <summary>Gets the server URL of the Picturepark authentication server.</summary>
        string BaseUrl { get; }

        /// <summary>Gets the HTTP timeout.</summary>
        TimeSpan HttpTimeout { get; }

        /// <summary>Gets the <see cref="IAuthClient"/>.</summary>
        IAuthClient AuthClient { get; }
    }
}