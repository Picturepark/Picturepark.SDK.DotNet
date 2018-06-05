using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Picturepark.Microsite.PressPortal.Services;

namespace Picturepark.Microsite.PressPortal.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (!Url.IsLocalUrl(returnUrl)) returnUrl = "/";

            var props = new AuthenticationProperties
            {
                RedirectUri = returnUrl
            };

            return Challenge(props, "oidc");
        }

        public IActionResult Denied(string returnUrl = null)
        {
            return View();
        }

        public async Task<IActionResult> Info([FromServices]IPictureparkPerRequestClient client)
        {
            if (User.Identity.IsAuthenticated)
            {
                var profile = await client.Profile.GetAsync();
                return View(profile);
            }

            return View(null);
        }

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
    }
}