using Picturepark.SDK.V1.Contract.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Authentication
{
	public class AccessTokenAuthClient : IAuthClient
	{
		public AccessTokenAuthClient(string baseUrl, string accessToken, string customerAlias)
		{
			BaseUrl = baseUrl;
			AccessToken = accessToken;
			CustomerAlias = customerAlias;
		}

		public string BaseUrl { get; }

		public string CustomerAlias { get; }

		private string AccessToken { get; }

		public Task<IDictionary<string, string>> GetAuthenticationHeadersAsync()
		{
			return Task.FromResult<IDictionary<string, string>>(new Dictionary<string, string>
			{
				{ "Authorization", "Bearer " + AccessToken }
			});
		}
	}
}
