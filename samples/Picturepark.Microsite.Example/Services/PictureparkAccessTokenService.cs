using Picturepark.SDK.V1;

namespace Picturepark.Microsite.Example.Services
{
    public class PictureparkAccessTokenService : PictureparkService, IPictureparkAccessTokenService
	{
		public PictureparkAccessTokenService(IPictureparkAccessTokenServiceSettings settings) : base(settings)
		{

		}
	}
}
