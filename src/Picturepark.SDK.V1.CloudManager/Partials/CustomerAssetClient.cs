using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using Picturepark.SDK.V1.CloudManager.Contract;

namespace Picturepark.SDK.V1.CloudManager;

public partial class CustomerAssetClient
{
    public async System.Threading.Tasks.Task PutLogoAsync(
        string customerId,
        LogoKind type,
        FileParameter file,
        System.Threading.CancellationToken cancellationToken = default)
    {
        var bf = new BinaryFormatter();
        using var ms = new MemoryStream();
        bf.Serialize(ms, new FileParameterSerializable(file));

        /*var boundary = System.Guid.NewGuid().ToString();
        var content = new MultipartFormDataContent(boundary);
        content.Headers.Remove("Content-Type");
        content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
        content.Add(new StreamContent(body), "formFile", fileName);
        var contentStream = await content.ReadAsStreamAsync();*/

        await PutLogoCoreAsync(customerId, type, ms, cancellationToken);
    }

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        if (request.Content is not StreamContent streamContent)
        {
            return;
        }

        if (streamContent.Headers.ContentType.MediaType != "multipart/form-data")
        {
            return;
        }

        var stream = streamContent.ReadAsStreamAsync().Result;
        var binForm = new BinaryFormatter();
        var file = (FileParameterSerializable)binForm.Deserialize(stream);

        var boundary = System.Guid.NewGuid().ToString();
        var content = new MultipartFormDataContent(boundary);
        content.Headers.Remove("Content-Type");
        content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
        content.Add(new ByteArrayContent(file.File), "formFile", file.FileName);
        request.Content = content;
    }

    [Serializable]
    private class FileParameterSerializable
    {
        public FileParameterSerializable(FileParameter fileParameter)
        {
            FileName = fileParameter.FileName;

            using var br = new BinaryReader(fileParameter.Data);
            File = br.ReadBytes((int)fileParameter.Data.Length);
        }

        public string FileName { get; set; }

        public byte[] File { get; set; }
    }
}