using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Clients;

namespace Picturepark.SDK.V1
{
	public class UserClient : UsersClientBase
    {
		public UserClient(string baseUrl, IAuthClient authClient)
            : base(authClient)
		{
		    BaseUrl = baseUrl;
		}
	}
}
