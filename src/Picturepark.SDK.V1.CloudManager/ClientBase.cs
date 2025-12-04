using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.CloudManager
{
    /// <summary>The base class for all clients.</summary>
    public abstract class ClientBase
    {
        private readonly ICloudManagerServiceSettings _settings;

        /// <summary>Initializes a new instance of the <see cref="ClientBase" /> class.</summary>
        /// <param name="settings">The client settings.</param>
        protected ClientBase(ICloudManagerServiceSettings settings)
        {
            _settings = settings;
        }

        /// <summary>Gets the base URL of the Fotoware Alto API.</summary>
        public string BaseUrl => _settings.BaseUrl;

        protected async Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken)
        {
            var message = new HttpRequestMessage();
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_settings.AuthClient != null)
            {
                foreach (var header in await _settings.AuthClient.GetAuthenticationHeadersAsync().ConfigureAwait(false))
                    message.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return message;
        }
    }
}
