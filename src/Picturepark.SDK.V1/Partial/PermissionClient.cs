using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
	public partial class PermissionsClient
    {
		public PermissionsClient(string baseUrl, IAuthClient authClient)
            : base(authClient)
		{
            BaseUrl = baseUrl;
		}
	}
}
