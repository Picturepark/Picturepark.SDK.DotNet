using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Authentication
{
	internal class TokenResponse
	{
		[DataMember(Name = "access_token", EmitDefaultValue = false)]
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }

		[JsonProperty("token_type")]
		public string TokenType { get; set; }

		[JsonProperty("expires_in")]
		public int ExpiresIn { get; set; }

		[JsonProperty("refresh_token")]
		public string RefreshToken { get; set; }

		[JsonProperty("as:client_id")]
		public string AsclientId { get; set; }

		[JsonProperty(".issued")]
		public string Issued { get; set; }

		[JsonProperty(".expires")]
		public string Expires { get; set; }
	}
}
