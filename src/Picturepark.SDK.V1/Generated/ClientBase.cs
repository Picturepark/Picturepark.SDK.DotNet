using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
    /// <summary>The base class for all clients.</summary>
    public abstract class ClientBase
	{
		private readonly IAuthClient _authClient;

        /// <summary>Initializes a new instance of the <see cref="ClientBase" /> class.</summary>
        /// <param name="authClient">The authentication client.</param>
        protected ClientBase(IAuthClient authClient)
        {
            _authClient = authClient;
            BaseUrl = authClient.BaseUrl;
        }

        /// <summary>Gets or sets the base URL.</summary>
        public string BaseUrl { get; protected set; }

        /// <summary>Creates an HTTP client with a bearer authentication token from the <see cref="IAuthClient"/>.</summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The HTTP client.</returns>
        protected async Task<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken)
		{
			var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl, UriKind.Absolute);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_authClient != null)
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authClient.GetAccessTokenAsync());

            return client;
		}
	}
}
