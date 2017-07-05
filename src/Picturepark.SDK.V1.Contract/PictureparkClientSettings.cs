using System;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.Contract
{
	/// <summary>The Picturepark client settings.</summary>
	public class PictureparkClientSettings : IPictureparkClientSettings
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PictureparkClientSettings"/> class.
		/// </summary>
		/// <param name="baseUrl">The base URL.</param>
		/// <param name="customerAlias">The customaer alias</param>
		public PictureparkClientSettings(string baseUrl, string customerAlias)
		{
			BaseUrl = baseUrl;
			HttpTimeout = TimeSpan.FromMinutes(1);
			CustomerAlias = customerAlias;
		}

		/// <summary>Initializes a new instance of the <see cref="PictureparkClientSettings"/> class.</summary>
		/// <param name="authClient">The authentication client.</param>
		public PictureparkClientSettings(IAuthClient authClient)
			: this(authClient.BaseUrl, authClient.CustomerAlias)
		{
			AuthClient = authClient;
		}

		/// <summary>Initializes a new instance of the <see cref="PictureparkClientSettings" /> class.</summary>
		/// <param name="baseUrl">The base URL.</param>
		/// <param name="authClient">The authentication client.</param>
		/// <param name="customerAlias">The customer alias</param>
		public PictureparkClientSettings(string baseUrl, IAuthClient authClient, string customerAlias)
			: this(baseUrl, customerAlias)
		{
			AuthClient = authClient;
		}

		/// <summary>Gets the server URL of the Picturepark server.</summary>
		public string BaseUrl { get; set; }

		/// <summary></summary>
		public TimeSpan HttpTimeout { get; set; }

		/// <summary></summary>
		public IAuthClient AuthClient { get; set; }

		public string CustomerAlias { get; set; }
	}
}
