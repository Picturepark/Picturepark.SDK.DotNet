using Picturepark.SDK.V1;

namespace Picturepark.Microsite.PressPortal.Services
{
    public class PictureparkPerRequestClient : PictureparkClient, IPictureparkPerRequestClient
    {
        public PictureparkPerRequestClient(IPictureparkPerRequestClientSettings settings) : base(settings)
        {
        }
    }
}
