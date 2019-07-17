using System;
using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Contract
{
    public sealed class TriggerObject : ITriggerObject
    {
        [JsonProperty("_trigger", NullValueHandling = NullValueHandling.Ignore)]
        public bool Trigger { get; set; }

        [JsonProperty("triggeredOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? TriggeredOn { get; set; }

        [JsonProperty("triggeredBy", NullValueHandling = NullValueHandling.Ignore)]
        public User TriggeredBy { get; set; }
    }
}