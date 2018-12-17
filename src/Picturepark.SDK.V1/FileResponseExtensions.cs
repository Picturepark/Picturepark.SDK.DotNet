using Picturepark.SDK.V1.Contract;
using System.IO;
using System.Net.Http;

namespace Picturepark.SDK.V1
{
    public static class FileResponseExtensions
    {
        public static string GetFileName(this FileResponse response)
        {
            string fileName = null;
            if (response.GetResponse() is HttpResponseMessage httpResponse)
            {
                var contentDisposition = httpResponse.Content.Headers.ContentDisposition;
                fileName = contentDisposition != null ? contentDisposition.FileNameStar : Path.GetFileName(httpResponse.RequestMessage.RequestUri.AbsolutePath);
            }

            return fileName;
        }
    }
}
