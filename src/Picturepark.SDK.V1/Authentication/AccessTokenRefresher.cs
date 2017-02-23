using System;
using System.Threading;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1.Authentication
{
	/// <summary>Periodically refreshes the access token of the given authentication client.</summary>
	public class AccessTokenRefresher : IDisposable
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
			_timer = new Timer(OnRefreshAccessToken, null, refreshInterval, refreshInterval);
		}

		~AccessTokenRefresher()
		{
			Dispose();
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

		private void OnRefreshAccessToken(object state)
		{
			if (_authClient == null)
				throw new ObjectDisposedException("authClient");

			_authClient.RefreshAccessTokenAsync();
		}
	}
}