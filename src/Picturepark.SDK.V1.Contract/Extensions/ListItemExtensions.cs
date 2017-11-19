using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class ListItemExtensions
	{
		/// <summary>Converts the content of a list item to the given type.</summary>
		/// <typeparam name="T">The content type.</typeparam>
		/// <param name="listItem">The list item.</param>
		/// <returns>The converted content.</returns>
		public static T ConvertToType<T>(this ListItem listItem)
		{
			return ((JObject)listItem.Content).ToObject<T>();
		}
	}
}