using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.Microsite.Example.Services
{
	public class PictureparkServiceClient : PictureparkClient, IPictureparkServiceClient
	{
		public PictureparkServiceClient(IPictureparkServiceClientSettings settings) : base(settings)
		{

		}
	}
}
