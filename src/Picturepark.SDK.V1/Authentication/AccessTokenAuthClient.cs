using Picturepark.SDK.V1.Contract.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Authentication
{
	/// <summary>Provides access token based authentication.</summary>
	public class AccessTokenAuthClient : IAuthClient
	{
		private string _accessToken;

		/// <summary>Initializes a new instance of the <see cref="AccessTokenAuthClient" /> class.</summary>
		/// <param name="baseUrl">The API base URL.</param>
		/// <param name="accessToken">The access token.</param>
		/// <param name="customerAlias">The customer alias.</param>
		public AccessTokenAuthClient(string baseUrl, string accessToken, string customerAlias)
		{
			BaseUrl = baseUrl;
			CustomerAlias = customerAlias;

			_accessToken = accessToken;
		}

		/// <summary>Gets the API base URL.</summary>
		public string BaseUrl { get; }

		/// <summary>Gets the customer alias.</summary>
		public string CustomerAlias { get; }

		/// <summary>Gets the authentication headers.</summary>
		/// <returns>The headers.</returns>
		public Task<IDictionary<string, string>> GetAuthenticationHeadersAsync()
		{
			return Task.FromResult<IDictionary<string, string>>(new Dictionary<string, string>
			{
				{ "Authorization", "Bearer " + _accessToken }
			});
		}
	}
}
