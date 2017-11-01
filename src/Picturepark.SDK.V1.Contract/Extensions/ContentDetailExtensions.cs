using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class ContentDetailExtensions
	{
		public static FileMetadata GetFileMetadata(this ContentDetail content)
		{
			return ((JObject)content.Content).ToObject<FileMetadata>();
		}

		/// <summary>
		/// Creates a typed content item wrapped in a ContentItem container
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="content">Content detail</param>
		/// <returns></returns>
		public static ContentItem<T> AsContentItem<T>(this ContentDetail content)
		{
			var item = ((JObject)content.Content).ToObject<T>();
			var contentItem = new ContentItem<T> { Id = content.Id, Audit = content.Audit, Content = item };
			return contentItem;
		}

		/// <summary>
		/// Creates a typed enummarable of content items wrapped in a ContentItem container
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="contents">Content details</param>
		/// <returns></returns>
		public static IEnumerable<ContentItem<T>> AsContentItems<T>(this IEnumerable<ContentDetail> contents)
		{
			var items = contents.Select(content => content.AsContentItem<T>());
			return items;
		}
	}
}