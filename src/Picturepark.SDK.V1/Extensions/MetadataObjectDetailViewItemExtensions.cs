using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public static class MetadataObjectDetailViewItemExtensions
    {
        public static T ConvertToType<T>(this MetadataObjectDetailViewItem metadataObject, string schemaId)
        {
            return ((JObject)metadataObject.Metadata[schemaId]).ToObject<T>();
        }
    }
}