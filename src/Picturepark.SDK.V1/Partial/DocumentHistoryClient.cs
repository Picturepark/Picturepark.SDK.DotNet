using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
	public partial class DocumentHistoryClient
    {
        public DocumentHistoryClient(string baseUrl, IAuthClient authClient) : this(authClient)
        {
            BaseUrl = baseUrl;
        }
    }
}
