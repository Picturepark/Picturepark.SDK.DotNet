using Picturepark.SDK.V1;

namespace Picturepark.Microsite.PressPortal.Services
{
    public class PictureparkServiceClient : PictureparkClient, IPictureparkServiceClient
    {
        public PictureparkServiceClient(IPictureparkServiceClientSettings settings) : base(settings)
        {

        }
    }
}
