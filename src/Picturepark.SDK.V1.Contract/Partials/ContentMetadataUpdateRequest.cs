using System;
using System.Linq.Expressions;

namespace Picturepark.SDK.V1.Contract
{
    public partial class ContentMetadataUpdateRequest
    {
        public static ContentMetadataUpdateRequest LayerMergeUpdate<T, TProperty>(Expression<Func<T, TProperty>> property, TProperty newValue)
        {
            return new ContentMetadataUpdateRequest
            {
                LayerSchemasUpdateOptions = UpdateOption.Merge,
                Metadata = Contract.Metadata.Update(property, newValue),
                LayerSchemaIds = new[] { typeof(T).Name }
            };
        }
    }
}