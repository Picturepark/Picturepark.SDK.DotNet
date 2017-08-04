using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class ListItemDetailExtensions
	{
		public static T ConvertToType<T>(this ListItemDetail listItem, string schemaId)
		{
			return (listItem.Content as JObject).ToObject<T>();
		}
	}
}