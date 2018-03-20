using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Contract
{
	public abstract class ReferenceObject : IReferenceObject
	{
		[JsonProperty("_refId", NullValueHandling = NullValueHandling.Ignore)]
		public string RefId { get; set; }
	}
}
