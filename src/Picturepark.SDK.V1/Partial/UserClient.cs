using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
	public partial class UsersClient
    {
		public UsersClient(string baseUrl, IAuthClient authClient)
            : base(authClient)
		{
		    BaseUrl = baseUrl;
		}
	}
}
