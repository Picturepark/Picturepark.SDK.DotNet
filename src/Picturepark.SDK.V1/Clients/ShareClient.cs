using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Clients;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
    public class ShareClient : SharesClientBase
    {
        public ShareClient(string baseUrl, IAuthClient authClient)
            : base(authClient)
        {
            BaseUrl = baseUrl;
        }
    }
}
