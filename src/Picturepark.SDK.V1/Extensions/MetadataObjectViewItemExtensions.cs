using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public static class MetadataObjectViewItemExtensions
    {
        public static T ConvertToType<T>(this MetadataObjectViewItem metadataObject, string schemaId)
        {
            return ((JObject)metadataObject.Metadata[schemaId]).ToObject<T>();
        }
    }
}