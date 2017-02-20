using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Interfaces;
using System.Collections.Generic;

namespace Picturepark.SDK.V1.Contract
{
	[PictureparkSystemSchema]
	public class Country : IReference
	{
		public string Name { get; set; }

		public Dictionary<string, string> Names { get; set; }

		public string RegionCode { get; set; }

		public string Alpha2 { get; set; }

		public string Alpha3 { get; set; }

		public string CountryCode { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string refId { get; set; }
	}
}
