using Newtonsoft.Json;
using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class PictureparkContentRelationAttribute : Attribute, IPictureparkAttribute
	{
		public PictureparkContentRelationAttribute(string name, string filter)
		{
			Name = name;
			TargetContext = TargetContext.Content;

			if (!string.IsNullOrEmpty(filter))
				Filter = JsonConvert.DeserializeObject<FilterBase>(filter);
		}

		public TargetContext TargetContext { get; }

		public FilterBase Filter { get; }

		public string Name { get; }
	}
}
