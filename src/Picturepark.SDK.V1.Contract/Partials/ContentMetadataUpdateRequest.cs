using System;
using System.Linq.Expressions;

namespace Picturepark.SDK.V1.Contract
{
    public partial class ContentMetadataUpdateRequest
    {
        /// <summary>
        /// Creates a new <see cref="ContentMetadataUpdateRequest"/> to update one property (field) in one layer on a content.
        /// </summary>
        /// <typeparam name="T">Layer type</typeparam>
        /// <typeparam name="TProperty">Property type</typeparam>
        /// <param name="property">The property on the object (schema field) you want to update</param>
        /// <param name="newValue">New value you want to update the schema field to</param>
        /// <returns>Update request to update the field</returns>
        /// <remarks>Layer name (schema ID) will be determined by schema ID defined in PictureparkSchemaAttribute on the layer class (if applied) or the name of the type.
        /// Anonymous classes are naturally not supported.</remarks>
        public static ContentMetadataUpdateRequest LayerMergeUpdate<T, TProperty>(Expression<Func<T, TProperty>> property, TProperty newValue)
        {
            return new ContentMetadataUpdateRequest
            {
                LayerSchemasUpdateOptions = UpdateOption.Merge,
                Metadata = Contract.Metadata.Update(property, newValue),
                LayerSchemaIds = new[] { Contract.Metadata.ResolveSchemaId(typeof(T)) }
            };
        }
    }
}