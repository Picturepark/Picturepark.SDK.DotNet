using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Contract
{
	[PictureparkSystemSchema]
	[PictureparkReference]
	public class Country : ReferenceObject
	{
		public string Name { get; set; }

		public TranslatedStringDictionary Names { get; set; }

		public string RegionCode { get; set; }

		public string Alpha2 { get; set; }

		public string Alpha3 { get; set; }

		public string CountryCode { get; set; }
	}
}
