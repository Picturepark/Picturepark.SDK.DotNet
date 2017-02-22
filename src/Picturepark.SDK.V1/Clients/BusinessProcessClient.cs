using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Clients;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
	public class BusinessProcessClient : BusinessProcessesClientBase
    {
        public BusinessProcessClient(string baseUrl, IAuthClient configuration) : base(configuration)
        {
            BaseUrl = baseUrl;
        }
    }
}
