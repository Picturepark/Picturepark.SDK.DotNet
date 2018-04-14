using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Picturepark.SDK.V1.Contract.SystemTypes
{
    public class Relation
    {
        [JsonProperty("_relId")]
        public string RelationId { get; set; }

        [JsonProperty("_relationType")]
        public string RelationType { get; set; }

        [JsonProperty("_targetDocType")]
        public string TargetDocType { get; set; }

        [JsonProperty("_targetId")]
        public string TargetId { get; set; }
    }
}
