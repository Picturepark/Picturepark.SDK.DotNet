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
		public PictureparkClientSettings(string baseUrl)
		{
			BaseUrl = baseUrl;
			HttpTimeout = TimeSpan.FromMinutes(1);
		}

		/// <summary>Initializes a new instance of the <see cref="PictureparkClientSettings"/> class.</summary>
		/// <param name="authClient">The authentication client.</param>
		public PictureparkClientSettings(IAuthClient authClient)
			: this(authClient.BaseUrl)
		{
			AuthClient = authClient;
		}

		/// <summary>Initializes a new instance of the <see cref="PictureparkClientSettings" /> class.</summary>
		/// <param name="baseUrl">The base URL.</param>
		/// <param name="authClient">The authentication client.</param>
		public PictureparkClientSettings(string baseUrl, IAuthClient authClient)
			: this(baseUrl)
		{
			AuthClient = authClient;
		}

		/// <summary>Gets the server URL of the Picturepark server.</summary>
		public string BaseUrl { get; set; }

		/// <summary></summary>
		public TimeSpan HttpTimeout { get; set; }

		/// <summary></summary>
		public IAuthClient AuthClient { get; set; }
	}
}
