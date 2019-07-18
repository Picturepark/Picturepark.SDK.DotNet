using System;
using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Contract
{
    public interface ITriggerObject
    {
        [JsonProperty("_trigger")]
        bool Trigger { get; set; }

        [JsonProperty("triggeredOn")]
        DateTime? TriggeredOn { get; set; }

        [JsonProperty("triggeredBy")]
        User TriggeredBy { get; set; }
    }
}