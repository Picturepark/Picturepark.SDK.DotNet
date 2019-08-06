using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Contract
{
    public interface IReferenceObject
    {
        [JsonProperty("_refId")]
        string RefId { get; set; }

        [JsonProperty("_refRequestId")]
        string RefRequestId { get; set; }
    }
}
