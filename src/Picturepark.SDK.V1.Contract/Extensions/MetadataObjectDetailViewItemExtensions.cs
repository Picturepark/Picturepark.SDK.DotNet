using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class MetadataObjectDetailViewItemExtensions
    {
        public static T ConvertToType<T>(this MetadataObjectDetailViewItem metadataObject, string schemaId)
        {
            return ((JObject)metadataObject.Metadata[schemaId]).ToObject<T>();
        }
    }
}