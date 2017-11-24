using IdentityModel;
using IdentityModel.Client;
using mshtml;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using IdentityModel.Jwt;
using Microsoft.IdentityModel.Protocols;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens;
using System.Collections.Generic;
using Picturepark.ContentUploader.Views.OidcClient;

namespace Picturepark.ContentUploader.Views
{
    public partial class LoginWebView : Window
    {
        private readonly string _redirectUri;
        private AuthorizeResponse _response = null;

        public event EventHandler<AuthorizeResponse> Completed;

        public LoginWebView(string redirectUri)
        {
            InitializeComponent();

            webView.Navigating += OnNavigating;
            Closed += OnClosed;

            _redirectUri = redirectUri;
        }

        public static async Task<LoginResult> RefreshTokenAsync(OidcSettings settings, string refreshToken)
        {
            var config = await LoadOpenIdConnectConfigurationAsync(settings);
            var tokenClient = new TokenClient(
                config.TokenEndpoint,
                settings.ClientId,
                settings.ClientSecret);

            var provider = JwkNetExtensions.CreateProvider();
            var jwk = provider.ToJsonWebKey();

            var tokenResponse = await tokenClient.RequestRefreshTokenPopAsync(
                refreshToken: refreshToken,
                algorithm: jwk.Alg,
                key: jwk.ToJwkString());

            if (tokenResponse.IsError)
            {
                return new LoginResult { ErrorMessage = tokenResponse.Error };
            }
            else
            {
                return new LoginResult
                {
                    Success = true,
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    IdentityToken = tokenResponse.IdentityToken,
                    AccessTokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn)
                };
            }
        }

        public static async Task<LoginResult> AuthenticateAsync(OidcSettings settings)
        {
            var taskCompletion = new TaskCompletionSource<LoginResult>();

            var nonce = CryptoRandom.CreateUniqueId(32);
            var verifier = CryptoRandom.CreateUniqueId(32);
            var config = await LoadOpenIdConnectConfigurationAsync(settings);

            var login = new LoginWebView(settings.RedirectUri);
            login.Completed += async (o, e) =>
            {
                if (e == null)
                {
                    taskCompletion.TrySetCanceled();
                }
                else
                {
                    try
                    {
                        var result = await ValidateResponseAsync(e, settings, config, nonce, verifier);
                        taskCompletion.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        taskCompletion.SetException(ex);
                    }
                }
            };

            login.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            login.ShowDialog(CreateUrl(settings, config, nonce, verifier));

            return await taskCompletion.Task;
        }

        public void ShowDialog(string url)
        {
            Dispatcher.InvokeAsync(() => webView.Navigate(url));
            ShowDialog();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            if (_response == null)
            {
                Completed?.Invoke(this, null);
            }
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(_redirectUri))
            {
                if (e.Uri.AbsoluteUri.Contains("#"))
                {
                    _response = new AuthorizeResponse(e.Uri.AbsoluteUri);
                }
                else
                {
                    var document = (IHTMLDocument3)((WebBrowser)sender).Document;
                    var inputElements = document.getElementsByTagName("INPUT").OfType<IHTMLElement>();
                    var resultUrl = "?";

                    foreach (var input in inputElements)
                    {
                        resultUrl += input.getAttribute("name") + "=";
                        resultUrl += input.getAttribute("value") + "&";
                    }

                    resultUrl = resultUrl.TrimEnd('&');
                    _response = new AuthorizeResponse(resultUrl);
                }

                e.Cancel = true;
                Completed?.Invoke(this, _response);

                Close();
            }
        }

        private static string CreateUrl(OidcSettings settings, OpenIdConnectConfiguration config, string nonce, string verifier)
        {
            var challenge = verifier.ToCodeChallenge();
            var request = new AuthorizeRequest(config.AuthorizationEndpoint);

            return request.CreateAuthorizeUrl(
                clientId: settings.ClientId,
                responseType: "code id_token",
                scope: settings.Scope,
                redirectUri: settings.RedirectUri,
                nonce: nonce,
                responseMode: OidcConstants.ResponseModes.FormPost,
                acrValues: settings.AcrValues,
                codeChallenge: settings.UsePkce ? challenge : null,
                codeChallengeMethod: settings.UsePkce ? OidcConstants.CodeChallengeMethods.Sha256 : null);
        }

        private static async Task<OpenIdConnectConfiguration> LoadOpenIdConnectConfigurationAsync(OidcSettings settings)
        {
            var discoAddress = settings.Authority + "/.well-known/openid-configuration";

            var manager = new ConfigurationManager<OpenIdConnectConfiguration>(discoAddress);
            return await manager.GetConfigurationAsync();
        }

        private static async Task<LoginResult> ValidateResponseAsync(AuthorizeResponse response, OidcSettings settings, OpenIdConnectConfiguration config, string expectedNonce, string verifier)
        {
            var tokenClaims = ValidateIdentityToken(response.IdentityToken, settings, config);
            if (tokenClaims == null)
            {
                return new LoginResult { ErrorMessage = "Invalid identity token." };
            }

            var nonce = tokenClaims.FirstOrDefault(c => c.Type == JwtClaimTypes.Nonce);
            if (nonce == null || !string.Equals(nonce.Value, expectedNonce, StringComparison.Ordinal))
            {
                return new LoginResult { ErrorMessage = "Inalid nonce." };
            }

            var codeHash = tokenClaims.FirstOrDefault(c => c.Type == JwtClaimTypes.AuthorizationCodeHash);
            if (codeHash == null || ValidateCodeHash(codeHash.Value, response.Code) == false)
            {
                return new LoginResult { ErrorMessage = "Invalid code." };
            }

            var provider = JwkNetExtensions.CreateProvider();
            var jwk = provider.ToJsonWebKey();

            var tokenClient = new TokenClient(
                config.TokenEndpoint,
                settings.ClientId,
                settings.ClientSecret);

            var tokenResponse = await tokenClient.RequestAuthorizationCodePopAsync(
                code: response.Code,
                redirectUri: settings.RedirectUri,
                codeVerifier: settings.UsePkce ? verifier : null,
                algorithm: jwk.Alg,
                key: jwk.ToJwkString());

            if (tokenResponse.IsError)
            {
                return new LoginResult { ErrorMessage = tokenResponse.Error };
            }

            var profileClaims = new List<Claim>();
            if (settings.LoadUserProfile)
            {
                var userInfoClient = new UserInfoClient(
                    new Uri(config.UserInfoEndpoint),
                    tokenResponse.AccessToken);

                var userInfoResponse = await userInfoClient.GetAsync();
                profileClaims = userInfoResponse.GetClaimsIdentity().Claims.ToList();
            }

            var principal = CreatePrincipal(tokenClaims, profileClaims, settings);

            return new LoginResult
            {
                Success = true,
                User = principal,
                IdentityToken = response.IdentityToken,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                AccessTokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn)
            };
        }

        private static List<Claim> ValidateIdentityToken(string identityToken, OidcSettings settings, OpenIdConnectConfiguration config)
        {
            var tokens = new List<X509SecurityToken>(
                from key in config.JsonWebKeySet.Keys
                select new X509SecurityToken(new X509Certificate2(Convert.FromBase64String(key.X5c.First()))));

            var parameter = new TokenValidationParameters
            {
                ValidIssuer = config.Issuer,
                ValidAudience = settings.ClientId,
                IssuerSigningTokens = tokens
            };

            JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();
            try
            {
                var handler = new JwtSecurityTokenHandler();
                return handler.ValidateToken(identityToken, parameter, out var token).Claims.ToList();
            }
            catch
            {
                return null;
            }
        }

        private static ClaimsPrincipal CreatePrincipal(List<Claim> tokenClaims, List<Claim> profileClaims, OidcSettings settings)
        {
            List<Claim> filteredClaims = new List<Claim>(tokenClaims);

            if (settings.FilterClaims)
            {
                filteredClaims = tokenClaims.Where(c => !settings.FilterClaimTypes.Contains(c.Type)).ToList();
            }

            var allClaims = new List<Claim>();
            allClaims.AddRange(filteredClaims);
            allClaims.AddRange(profileClaims);

            var id = new ClaimsIdentity(allClaims.Distinct(new ClaimComparer()), "OIDC");
            return new ClaimsPrincipal(id);
        }

        private static bool ValidateCodeHash(string c_hash, string code)
        {
            using (var sha = SHA256.Create())
            {
                var codeHash = sha.ComputeHash(Encoding.ASCII.GetBytes(code));

                byte[] leftBytes = new byte[16];
                Array.Copy(codeHash, leftBytes, 16);
                var codeHashB64 = Base64Url.Encode(leftBytes);

                return string.Equals(c_hash, codeHashB64, StringComparison.Ordinal);
            }
        }
    }
}