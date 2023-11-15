using System.Net.Http;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Partial;

internal class InternalIngestClient : IngestClient
{
    public InternalIngestClient(IBusinessProcessClient businessProcessClient, IContentClient contentClient, IPictureparkServiceSettings configuration, HttpClient httpClient) : base(
        configuration,
        httpClient)
    {
        BusinessProcessClient = businessProcessClient;
        ContentClient = contentClient;
    }

    public IBusinessProcessClient BusinessProcessClient { get; }

    public IContentClient ContentClient { get; }
}
