using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class ContentDetailExtensions
	{
		/// <summary>Gets the content detail's file metadata.</summary>
		/// <param name="content">The content detail.</param>
		/// <returns>The file metadata.</returns>
		public static FileMetadata GetFileMetadata(this ContentDetail content)
		{
			return ((JObject)content.Content).ToObject<FileMetadata>();
		}

		/// <summary>Creates a typed content item wrapped in a ContentItem container.</summary>
		/// <typeparam name="T">The content item type.</typeparam>
		/// <param name="content">The content detail.</param>
		/// <returns>The content item.</returns>
		public static ContentItem<T> AsContentItem<T>(this ContentDetail content)
		{
			var item = ((JObject)content.Content).ToObject<T>();
			var contentItem = new ContentItem<T> { Id = content.Id, Audit = content.Audit, Content = item };
			return contentItem;
		}

		/// <summary>Creates a typed enummarable of content items wrapped in a ContentItem container.</summary>
		/// <typeparam name="T">The content item type.</typeparam>
		/// <param name="contents">The content details.</param>
		/// <returns>The enumerable of content item.</returns>
		public static IEnumerable<ContentItem<T>> AsContentItems<T>(this IEnumerable<ContentDetail> contents)
		{
			var items = contents.Select(content => content.AsContentItem<T>());
			return items;
		}
	}
}