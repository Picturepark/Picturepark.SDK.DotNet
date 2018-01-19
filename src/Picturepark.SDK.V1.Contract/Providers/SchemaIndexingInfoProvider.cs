using System;
using Picturepark.SDK.V1.Contract.Builders;

namespace Picturepark.SDK.V1.Contract.Providers
{
    public abstract class SchemaIndexingInfoProvider<T> : ISchemaIndexingInfoProvider
    {
        public SchemaIndexingInfo GetSchemaIndexingInfo()
        {
            var builder = (SchemaIndexingInfoBuilder<T>)Activator.CreateInstance(GetType());
            return Setup(builder).Build();
        }

        protected abstract SchemaIndexingInfoBuilder<T> Setup(SchemaIndexingInfoBuilder<T> builder);
    }
}
