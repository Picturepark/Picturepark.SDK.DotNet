using System.Net.Http;
using Picturepark.SDK.V1.CloudManager.Contract;

namespace Picturepark.SDK.V1.CloudManager;

public partial class CustomerAssetClient
{
    public async System.Threading.Tasks.Task PutLogoAsync(
        string customerId,
        LogoKind type,
        System.IO.Stream body,
        string fileName,
        System.Threading.CancellationToken cancellationToken = default)
    {
        var boundary = System.Guid.NewGuid().ToString();
        var content = new MultipartFormDataContent(boundary);
        content.Headers.Remove("Content-Type");
        content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
        content.Add(new StreamContent(body), "formFile", fileName);
        var contentStream = await content.ReadAsStreamAsync();

        await PutLogoCoreAsync(customerId, type, contentStream, cancellationToken);
    }

    /*partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        if (request.Content is not StreamContent streamContent)
        {
            return;
        }

        if (streamContent.Headers.ContentType.MediaType != "multipart/form-data")
        {
            return;
        }

        var boundary = System.Guid.NewGuid().ToString();
        var content = new MultipartFormDataContent(boundary);
        content.Headers.Remove("Content-Type");
        content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
        content.Add(streamContent, "formFile", "formFile");
        request.Content = content;
    }*/
}