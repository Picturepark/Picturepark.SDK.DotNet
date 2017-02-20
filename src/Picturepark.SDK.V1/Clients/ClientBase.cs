using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Authentication;

namespace Picturepark.SDK.V1
{
	public abstract class ClientBase
	{
		private readonly IAuthClient _authClient;

	    protected ClientBase(IAuthClient configuration)
		{
			_authClient = configuration;
		}

		protected async Task<HttpClient> CreateHttpClientAsync(CancellationToken ct)
		{
			var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_authClient != null)
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authClient.GetAccessTokenAsync());

            return client;
		}
	}
}
