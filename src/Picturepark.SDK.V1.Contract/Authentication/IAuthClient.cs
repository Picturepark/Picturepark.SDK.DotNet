﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Authentication
{
    /// <summary>Retrieves access tokens for authentication.</summary>
    public interface IAuthClient
    {
        /// <summary>Gets the server URL of the Fotoware Alto authentication server.</summary>
        string BaseUrl { get; }

        /// <summary>Gets the customer alias.</summary>
        string CustomerAlias { get; }

        /// <summary>Gets the authentication headers.</summary>
        /// <returns>The headers.</returns>
        Task<IDictionary<string, string>> GetAuthenticationHeadersAsync();
    }
}
