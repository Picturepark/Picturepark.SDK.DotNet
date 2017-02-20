using Newtonsoft.Json;
using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class PictureparkSchemaItemAttribute : Attribute, IPictureparkAttribute
	{
		public PictureparkSchemaItemAttribute(string filter)
		{
			if (!string.IsNullOrEmpty(filter))
				Filter = JsonConvert.DeserializeObject<FilterBase>(filter);
		}

		public FilterBase Filter { get; }
	}
}
