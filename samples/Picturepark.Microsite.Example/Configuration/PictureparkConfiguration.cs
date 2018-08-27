using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.Microsite.Example.Configuration
{
	public class PictureparkConfiguration
	{
		public string ApiBaseUrl { get; set; }

		public string ApplicationBaseUrl { get; set; }

		public string FrontendBaseUrl { get; set; }

		public string AccessToken { get; set; }

		public string CustomerAlias { get; set; }
	}
}
