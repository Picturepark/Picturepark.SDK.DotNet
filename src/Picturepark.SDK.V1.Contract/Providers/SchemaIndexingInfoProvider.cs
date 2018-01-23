using System;
using Picturepark.SDK.V1.Contract.Builders;

namespace Picturepark.SDK.V1.Contract.Providers
{
    public abstract class SchemaIndexingInfoProvider<T> : ISchemaIndexingInfoProvider
    {
        public SchemaIndexingInfo GetSchemaIndexingInfo()
        {
            var builder = CreateBuilder();
            return Setup(builder).Build();
        }

        protected virtual SchemaIndexingInfoBuilder<T> CreateBuilder()
        {
            return new SchemaIndexingInfoBuilder<T>();
        }

        protected abstract SchemaIndexingInfoBuilder<T> Setup(SchemaIndexingInfoBuilder<T> builder);
    }
}
