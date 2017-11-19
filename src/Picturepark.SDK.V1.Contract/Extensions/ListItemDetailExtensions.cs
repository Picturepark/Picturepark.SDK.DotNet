using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class ListItemDetailExtensions
	{
		/// <summary>Converts the content of a list item detail to the given type.</summary>
		/// <typeparam name="T">The content type.</typeparam>
		/// <param name="listItem">The list item detail.</param>
		/// <param name="schemaId">The schema ID.</param>
		/// <returns>The converted content.</returns>
		public static T ConvertToType<T>(this ListItemDetail listItem, string schemaId)
		{
			return ((JObject)listItem.Content).ToObject<T>();
		}
	}
}