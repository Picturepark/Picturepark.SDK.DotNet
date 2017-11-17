using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using System.Reflection;

namespace Picturepark.SDK.V1
{
	/// <summary>The base class for all clients.</summary>
	public abstract class ClientBase
	{
		private readonly IPictureparkClientSettings _settings;
		private Lazy<Newtonsoft.Json.JsonSerializerSettings> _jsonSettings;

		/// <summary>Initializes a new instance of the <see cref="ClientBase" /> class.</summary>
		/// <param name="settings">The client settings.</param>
		protected ClientBase(IPictureparkClientSettings settings)
		{
			_settings = settings;

			GetType().GetRuntimeProperty("BaseUrl").SetValue(this, _settings.BaseUrl);
			Alias = _settings.CustomerAlias;

			_jsonSettings = new Lazy<Newtonsoft.Json.JsonSerializerSettings>(() =>
			{
				var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
				{
					Converters = new Newtonsoft.Json.JsonConverter[]
					{
						new JsonExceptionConverter()
					}
				};
				return jsonSettings;
			});
		}

		public string Alias { get; protected set; }

		protected async Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken)
		{
			var message = new HttpRequestMessage();
			message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			message.Headers.TryAddWithoutValidation("Picturepark-CustomerAlias", _settings.CustomerAlias);

			if (_settings.AuthClient != null)
			{
				foreach (var header in await _settings.AuthClient.GetAuthenticationHeadersAsync())
					message.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			return message;
		}
	}
}
