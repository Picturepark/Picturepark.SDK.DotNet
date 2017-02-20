using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests
{
	public class Configuration
	{
		public string ExampleFilesBasePath { get; set; }

		public string TempPath { get; set; }

		public string ApiBaseUrl { get; set; }

		public string ApiEmail { get; set; }

		public string ApiPassword { get; set; }

		public UserEmail EmailRecipient { get; set; }
	}
}
