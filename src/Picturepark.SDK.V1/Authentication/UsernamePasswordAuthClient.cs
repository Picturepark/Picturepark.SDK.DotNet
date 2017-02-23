using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.Authentication
{
    /// <summary>Authenticates the clients with a username and password.</summary>
    public class UsernamePasswordAuthClient : IAuthClient
    {
        private readonly object _lock = new object();

        private string _username;
        private string _password;

        private Task _accessTokenTask;
        private Task _refreshTokenTask;

        /// <summary>Initializes a new instance of the <see cref="UsernamePasswordAuthClient" /> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        public UsernamePasswordAuthClient(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        /// <summary>Initializes a new instance of the <see cref="UsernamePasswordAuthClient" /> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public UsernamePasswordAuthClient(string baseUrl, string username, string password)
            : this(baseUrl)
        {
            _username = username;
            _password = password;
        }

        /// <summary>Gets the base URL.</summary>
        public string BaseUrl { get; }

        /// <summary>Gets the currently loaded access token.</summary>
        public string AccessToken { get; private set; }

        /// <summary>Gets the currently loaded refresh token.</summary>
        public string RefreshToken { get; private set; }

        /// <summary>Gets the currently loaded token expiry token.</summary>
        public DateTime TokenExpiryTime { get; private set; }

        /// <summary>Retrieves the access token for the given username and password.</summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The task.</returns>
        public async Task LoginAsync(string username, string password)
        {
            lock (_lock)
            {
                _username = username;
                _password = password;
            }

            await LoadAccessTokenAsync(true);
        }

        /// <summary>Gets the access token.</summary>
        /// <returns>The access token.</returns>
        public async Task<string> GetAccessTokenAsync()
        {
            if (AccessToken == null || TokenExpiryTime < DateTime.Now)
            {
                if (RefreshToken != null)
                    await RefreshAccessTokenAsync();
                else
                    await LoadAccessTokenAsync(false);
            }

            return AccessToken;
        }

        private Task RefreshAccessTokenAsync()
        {
            lock (_lock)
            {
                if (_refreshTokenTask != null)
                    return _refreshTokenTask;

                _refreshTokenTask = RefreshAccessTokenInternalAsync();
                return _refreshTokenTask;
            }
        }

        private async Task RefreshAccessTokenInternalAsync()
        {
            try
            {
                var response = await GetTokenAsync("refresh_token", RefreshToken, null, null, "Picturepark.Application");
                lock (_lock)
                {
                    AccessToken = response.AccessToken;
                    RefreshToken = response.RefreshToken;
                    TokenExpiryTime = DateTime.Now.AddSeconds(response.ExpiresIn - 60);
                    _refreshTokenTask = null;
                }
            }
            catch
            {
                await LoadAccessTokenAsync(true);
            }
        }

        private Task LoadAccessTokenAsync(bool force)
        {
            lock (_lock)
            {
                if (_username == null || _password == null)
                    throw new InvalidOperationException("Access token could not be retrieved because the username or password is not set.");

                if (_accessTokenTask != null && !force)
                    return _accessTokenTask;

                _accessTokenTask = LoadAccessTokenInternalAsync();
                return _accessTokenTask;
            }
        }

        private async Task LoadAccessTokenInternalAsync()
        {
            Task<TokenResponse> getAccessTokenTask;
            lock (_lock)
                getAccessTokenTask = GetTokenAsync("password", null, _username, _password, "Picturepark.Application");

            var response = await getAccessTokenTask;
            lock (_lock)
            {
                AccessToken = response.AccessToken;
                RefreshToken = response.RefreshToken;
                TokenExpiryTime = DateTime.Now.AddSeconds(response.ExpiresIn - 60);
                _accessTokenTask = null;
            }
        }

        private async Task<TokenResponse> GetTokenAsync(string grantType, string refreshToken, string username, string password, string clientId)
        {
            var url = string.Format("{0}/{1}", BaseUrl, "token");
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    var content = new List<KeyValuePair<string, string>>();
                    content.Add(new KeyValuePair<string, string>("grant_type", grantType));

                    if (refreshToken != null)
                        content.Add(new KeyValuePair<string, string>("refresh_token", refreshToken));

                    if (username != null)
                        content.Add(new KeyValuePair<string, string>("username", username));

                    if (password != null)
                        content.Add(new KeyValuePair<string, string>("password", password));

                    if (clientId != null)
                        content.Add(new KeyValuePair<string, string>("client_id", clientId));

                    request.Content = new FormUrlEncodedContent(content);
                    request.Method = new HttpMethod("POST");
                    request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();

                        var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        return JsonConvert.DeserializeObject<TokenResponse>(responseData);
                    }
                }
            }
        }
    }
}
