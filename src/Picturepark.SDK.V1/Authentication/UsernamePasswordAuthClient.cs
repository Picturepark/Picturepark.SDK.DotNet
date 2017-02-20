using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Authentication
{
    /// <summary>Authenticates the clients with a username and password.</summary>
    public class UsernamePasswordAuthClient : IAuthClient
    {
        private readonly object _lock = new object();

        private string _username;
        private string _password;

        private string _accessToken;
        private string _refreshToken;

        private Task _accessTokenTask;
        private Task _refreshTokenTask;

        /// <summary>Initializes a new instance of the <see cref="UsernamePasswordAuthClient"/> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        public UsernamePasswordAuthClient(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        /// <summary>Initializes a new instance of the <see cref="UsernamePasswordAuthClient"/> class.</summary>
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
            if (_accessToken == null)
                await LoadAccessTokenAsync(false);

            return _accessToken;
        }

        /// <summary>Refreshes the access token.</summary>
        /// <returns>The task.</returns>
        public Task RefreshAccessTokenAsync()
        {
            lock (_lock)
            {
                if (_refreshTokenTask != null)
                    return _refreshTokenTask;

                _refreshTokenTask = RefreshTokenInternalAsync();
                return _refreshTokenTask;
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
                _accessToken = response.AccessToken;
                _refreshToken = response.RefreshToken;
                _accessTokenTask = null;
            }
        }

        private async Task RefreshTokenInternalAsync()
        {
            var response = await GetTokenAsync("refresh_token", _refreshToken, null, null, "Picturepark.Application");
            lock (_lock)
            {
                _accessToken = response.AccessToken;
                _refreshToken = response.RefreshToken;
                _refreshTokenTask = null;
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
