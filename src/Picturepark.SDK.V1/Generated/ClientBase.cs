using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
	/// <summary>The base class for all clients.</summary>
	public abstract class ClientBase
	{
		private readonly IPictureparkClientSettings _settings;

		/// <summary>Initializes a new instance of the <see cref="ClientBase" /> class.</summary>
		/// <param name="settings">The client settings.</param>
		protected ClientBase(IPictureparkClientSettings settings)
		{
			_settings = settings;
			BaseUrl = _settings.BaseUrl;
			Alias = _settings.CustomerAlias;
		}

		/// <summary>Gets or sets the base URL.</summary>
		public string BaseUrl { get; protected set; }

		public string Alias { get; protected set; }

		/// <summary>Creates an HTTP client with a bearer authentication token from the IAuthClient.</summary>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The HTTP client.</returns>
		protected async Task<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken)
		{
			var client = new HttpClient();
			client.Timeout = _settings.HttpTimeout;
			client.BaseAddress = new Uri(BaseUrl, UriKind.Absolute);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("Picturepark-CustomerAlias", _settings.CustomerAlias);

			if (_settings.AuthClient != null)
			{
				foreach (var header in await _settings.AuthClient.GetAuthenticationHeadersAsync())
					client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
			}

			return client;
		}
	}
}
