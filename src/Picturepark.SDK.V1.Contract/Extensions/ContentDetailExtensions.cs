using System.Collections.Generic;
using System.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class ContentDetailExtensions
    {
        /// <summary>Creates a typed enumerable of content items wrapped in a ContentItem container.</summary>
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