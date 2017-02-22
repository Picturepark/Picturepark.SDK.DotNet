using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Clients;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
	public class DocumentHistoryClient : DocumentHistoryClientBase
    {
        public DocumentHistoryClient(string baseUrl, IAuthClient configuration) : base(configuration)
        {
            BaseUrl = baseUrl;
        }
    }
}
