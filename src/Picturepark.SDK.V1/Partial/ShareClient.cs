using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
    public partial class SharesClient
    {
        public SharesClient(string baseUrl, IAuthClient authClient)
            : base(authClient)
        {
            BaseUrl = baseUrl;
        }
    }
}
