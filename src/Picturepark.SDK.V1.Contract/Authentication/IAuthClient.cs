using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Authentication
{
    /// <summary>Retrieves access tokens for authentication.</summary>
    public interface IAuthClient
    {
        /// <summary>Gets the base URL of the Picturepark authentication server.</summary>
        string BaseUrl { get; }

        /// <summary>Gets the access token.</summary>
        /// <returns>The access token.</returns>
        Task<string> GetAccessTokenAsync();

        /// <summary>Refreshes the access token.</summary>
        /// <returns>The task.</returns>
        Task RefreshAccessTokenAsync();
    }
}
