using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class MetadataObjectViewItemExtensions
    {
        public static T ConvertToType<T>(this MetadataObjectViewItem metadataObject, string schemaId)
        {
            return ((JObject)metadataObject.Metadata[schemaId]).ToObject<T>();
        }
    }
}