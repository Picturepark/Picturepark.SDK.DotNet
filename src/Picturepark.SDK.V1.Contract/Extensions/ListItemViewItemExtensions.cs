using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class ListItemViewItemExtensions
    {
        public static T ConvertToType<T>(this ListItemViewItem listItem, string schemaId)
        {
            return ((JObject)listItem.Metadata[schemaId]).ToObject<T>();
        }
    }
}