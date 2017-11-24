using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Contract
{
	public interface IReferenceObject
	{
		[JsonProperty("refId")]
		string RefId { get; set; }
	}
}
