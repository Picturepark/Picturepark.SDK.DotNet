using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Picturepark.SDK.V1.CloudManager.Contract;

namespace Picturepark.SDK.V1.CloudManager;

/// <summary>
/// Contains methods to work around the limitations of the NSwag-generated code
/// </summary>
public partial class CustomerAssetClient
{
    public async Task PutLogoAsync(
        string customerId,
        LogoKind type,
        FileParameter file,
        System.Threading.CancellationToken cancellationToken = default)
    {
        using var ms = SerializeFileParameter(file);
        await PutLogoCoreAsync(customerId, type, ms, cancellationToken);
    }

    public async Task PutWatermarkAsync(
        string customerId,
        FileParameter file,
        System.Threading.CancellationToken cancellationToken = default)
    {
        using var ms = SerializeFileParameter(file);
        await PutWatermarkCoreAsync(customerId, ms, cancellationToken);
    }

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        if (request.Content is not StreamContent streamContent || streamContent.Headers.ContentType.MediaType != "multipart/form-data")
            return;

        var stream = streamContent.ReadAsStreamAsync().Result;
        var bf = new BinaryFormatter();
        var file = (FileParameterSerializable)bf.Deserialize(stream);

        var boundary = Guid.NewGuid().ToString();
        var content = new MultipartFormDataContent(boundary);
        content.Headers.Remove("Content-Type");
        content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
        content.Add(new ByteArrayContent(file.File), "formFile", file.FileName);
        request.Content = content;
    }

    private MemoryStream SerializeFileParameter(FileParameter file)
    {
        var bf = new BinaryFormatter();
        var ms = new MemoryStream();
        bf.Serialize(ms, new FileParameterSerializable(file));
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
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