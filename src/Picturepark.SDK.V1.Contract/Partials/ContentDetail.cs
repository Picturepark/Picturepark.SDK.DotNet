using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract
{
    /// <summary>The content detail.</summary>
    public partial class ContentDetail
    {
        /// <summary>Gets the content detail's file metadata.</summary>
        /// <returns>The file metadata.</returns>
        public FileMetadata GetFileMetadata()
        {
            return Content is FileMetadata metadata ? metadata : ((JObject)Content).ToObject<FileMetadata>();
        }

        /// <summary>Creates a typed content item wrapped in a ContentItem container.</summary>
        /// <typeparam name="T">The content item type.</typeparam>
        /// <returns>The content item.</returns>
        public ContentItem<T> AsContentItem<T>()
        {
            var item = Content is T ? (T)Content : ((JObject)Content).ToObject<T>();
            return new ContentItem<T>
            {
                Id = Id,
                Audit = Audit,
                Content = item
            };
        }
    }
}
