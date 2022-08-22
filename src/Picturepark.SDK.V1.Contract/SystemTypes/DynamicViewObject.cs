using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract.Attributes;

// ReSharper disable ConvertToStaticClass
namespace Picturepark.SDK.V1.Contract.SystemTypes
{
    /// <summary>
    /// Placeholder to be used with <see cref="PictureparkDynamicViewAttribute"/>
    /// </summary>
    [PictureparkSystemSchema]
    public sealed class DynamicViewObject
    {
        [JsonConstructor]
        internal DynamicViewObject(DynamicViewFieldMetaBase meta) => Meta = meta;

        private DynamicViewObject()
        {
        }

        /// <summary>
        /// Metadata for DynamicViewField.
        /// Will only be returned if Content/ListItem resolve behaviors for resolving DynamicViewFields are used.
        /// </summary>
        [JsonProperty("_meta")]
        public DynamicViewFieldMetaBase Meta { get; }
    }
}
