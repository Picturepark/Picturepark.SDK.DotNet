using System;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.Authentication
{
	/// <summary>Periodically refreshes the access token of the given authentication client.</summary>
	public class AccessTokenRefresher : IAuthClient, IDisposable
	{
		private readonly object _lock = new object();
		private readonly IAuthClient _authClient;
		private Timer _timer;

		/// <summary>Initializes a new instance of the <see cref="AccessTokenRefresher" /> class.</summary>
		/// <param name="authClient">The authentication client.</param>
		/// <exception cref="ArgumentNullException"><paramref name="authClient"/> is <see langword="null"/></exception>
		public AccessTokenRefresher(IAuthClient authClient)
			: this(authClient, TimeSpan.FromMinutes(10))
		{
		}

        /// <summary>Initializes a new instance of the <see cref="AccessTokenRefresher" /> class.</summary>
        /// <param name="authClient">The authentication client.</param>
        /// <param name="refreshInterval">The refresh interval.</param>
        /// <exception cref="ArgumentNullException"><paramref name="authClient"/> is <see langword="null"/></exception>
        public AccessTokenRefresher(IAuthClient authClient, TimeSpan refreshInterval)
		{
			if (authClient == null)
				throw new ArgumentNullException(nameof(authClient));

			_authClient = authClient;
			_timer = new Timer(OnRefreshAccesToken, null, refreshInterval, refreshInterval);
		}

		~AccessTokenRefresher()
		{
			Dispose();
		}

        /// <summary>Gets the base URL of the Picturepark authentication server.</summary>
        public string BaseUrl => _authClient.BaseUrl;

        /// <summary>Gets the access token.</summary>
        /// <returns>The access token.</returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public Task<string> GetAccessTokenAsync()
		{
			if (_authClient == null)
				throw new ObjectDisposedException("authClient");

			return _authClient.GetAccessTokenAsync();
		}

		/// <summary>Refreshes the access token.</summary>
		/// <returns>The task.</returns>
		/// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
		public Task RefreshAccessTokenAsync()
		{
			if (_authClient == null)
				throw new ObjectDisposedException("authClient");

			return _authClient.RefreshAccessTokenAsync();
		}

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
		{
			if (_timer != null)
			{
				lock (_lock)
				{
					if (_timer != null)
					{
						_timer.Dispose();
						_timer = null;
					}
				}
			}

			GC.SuppressFinalize(this);
		}

		private void OnRefreshAccesToken(object state)
		{
			if (_authClient == null)
				throw new ObjectDisposedException("authClient");

			_authClient.RefreshAccessTokenAsync();
		}
	}
}