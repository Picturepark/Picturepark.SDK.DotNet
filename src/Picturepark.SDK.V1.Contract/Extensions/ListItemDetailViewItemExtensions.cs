using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class ListItemDetailViewItemExtensions
    {
        public static T ConvertToType<T>(this ListItemDetailViewItem listItem, string schemaId)
        {
            return ((JObject)listItem.Metadata[schemaId]).ToObject<T>();
        }
    }
}