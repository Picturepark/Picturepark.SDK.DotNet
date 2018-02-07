using System.Linq;
using System.Net;
using Microsoft.Net.Http.Headers;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public static class FileResponseExtensions
    {
        public static string GetFileName(this FileResponse response)
        {
            var disposition = response.Headers["Content-Disposition"].FirstOrDefault();
            if (disposition != null)
            {
                var composition = ContentDispositionHeaderValue.Parse(disposition);
                if (composition != null)
                {
                    return WebUtility.UrlDecode(HeaderUtilities.RemoveQuotes(composition.FileName));
                }
            }

            return null;
        }
    }
}
