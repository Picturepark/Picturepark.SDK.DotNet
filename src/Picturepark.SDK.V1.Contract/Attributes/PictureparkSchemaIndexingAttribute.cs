using System;
using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract.Providers;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PictureparkSchemaIndexingAttribute : Attribute, IPictureparkAttribute
    {
        public PictureparkSchemaIndexingAttribute(string schemaIndexingInfo)
        {
            if (!string.IsNullOrEmpty(schemaIndexingInfo))
            {
                SchemaIndexingInfo = JsonConvert.DeserializeObject<SchemaIndexingInfo>(schemaIndexingInfo);
            }
        }

        public PictureparkSchemaIndexingAttribute(Type schemaIndexingInfoProvider)
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
