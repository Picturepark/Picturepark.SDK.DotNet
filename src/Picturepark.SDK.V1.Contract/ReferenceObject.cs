using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Contract
{
    [JsonConverter(typeof(ReferenceObjectConverter))]
    public abstract class ReferenceObject : IReferenceObject
    {
        [JsonProperty("_refId", NullValueHandling = NullValueHandling.Ignore)]
        public string RefId { get; set; }

        [JsonProperty("_refRequestId", NullValueHandling = NullValueHandling.Ignore)]
        public string RefRequestId { get; set; }
    }
}
