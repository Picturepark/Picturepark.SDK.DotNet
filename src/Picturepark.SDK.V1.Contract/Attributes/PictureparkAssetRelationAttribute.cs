using Newtonsoft.Json;
using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class PictureparkAssetRelationAttribute : Attribute, IPictureparkAttribute
	{
		public PictureparkAssetRelationAttribute(string name, string filter)
		{
			Name = name;
            TargetContext = TargetContext.Asset;

            if (!string.IsNullOrEmpty(filter))
				Filter = JsonConvert.DeserializeObject<FilterBase>(filter);
		}

		public TargetContext TargetContext { get; }

		public FilterBase Filter { get; }

		public string Name { get; }
	}
}
