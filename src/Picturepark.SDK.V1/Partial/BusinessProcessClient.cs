using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
	public partial class BusinessProcessesClient
    {
        public BusinessProcessesClient(string baseUrl, IAuthClient authClient) : this(authClient)
        {
            BaseUrl = baseUrl;
        }
    }
}
