using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
	public partial class PublicAccessClient
    {
        public PublicAccessClient(string baseUrl, IAuthClient authClient)
            : base(authClient)
		{
            BaseUrl = baseUrl;
		}
	}
}
