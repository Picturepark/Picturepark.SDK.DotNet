using System.Security.Principal;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.Microsite.Example.ViewModel
{
	public class Account
	{
		public UserProfile Profile { get; set; }

		public string AccessToken { get; set; }
	}
}
