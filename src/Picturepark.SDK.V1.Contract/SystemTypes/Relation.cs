using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Picturepark.SDK.V1.Contract.SystemTypes
{
	public class Relation
	{
		[JsonProperty("_relId")]
		public string RelationId { get; set; }

		[JsonProperty("_relationType")]
		public string RelationType { get; set; }

		[JsonProperty("_targetContext")]
		public TargetContext TargetContext { get; set; }

		[JsonProperty("_targetId")]
		public string TargetId { get; set; }
	}
}
