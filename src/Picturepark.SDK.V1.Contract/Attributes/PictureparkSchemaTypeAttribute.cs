using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class PictureparkSchemaTypeAttribute : Attribute, IPictureparkAttribute
	{
		public PictureparkSchemaTypeAttribute(MetadataSchemaType metadataType)
		{
			MetadataType = metadataType;
		}

		public MetadataSchemaType MetadataType { get; }
	}
}
