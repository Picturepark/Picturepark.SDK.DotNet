using System;
using Picturepark.SDK.V1.Contract.Attributes.Providers;

namespace Picturepark.SDK.V1.Contract.Attributes
{
	public class PictureparkSchemaIndexingAttribute
	{
		public PictureparkSchemaIndexingAttribute(string name, string schemaIndexingInfo)
		{
			if (!string.IsNullOrEmpty(schemaIndexingInfo))
			{
				SchemaIndexingInfo = SchemaIndexingInfo.FromJson(schemaIndexingInfo);
			}
		}

		public PictureparkSchemaIndexingAttribute(string name, Type schemaIndexingInfoProvider)
		{
			var provider = Activator.CreateInstance(schemaIndexingInfoProvider);
			if (provider is ISchemaIndexingInfoProvider)
			{
				SchemaIndexingInfo = ((ISchemaIndexingInfoProvider)Activator.CreateInstance(schemaIndexingInfoProvider)).GetSchemaIndexingInfo();
			}
			else
			{
				throw new ArgumentException("The parameter schemaIndexingInfoProvider is not of type ISchemaIndexingInfoProvider.");
			}
		}

		public SchemaIndexingInfo SchemaIndexingInfo { get; }
	}
}
