using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.Microsite.PressPortal.Configuration
{
    public class AuthenticationConfiguration
    {
        public string Authority { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public List<string> Scopes { get; set; } = new List<string>();

        public string CustomerId { get; set; }

        public string CustomerAlias { get; set; }
    }
}
