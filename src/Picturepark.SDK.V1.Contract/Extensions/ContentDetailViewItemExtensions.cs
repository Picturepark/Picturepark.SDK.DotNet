using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class ContentDetailViewItemExtensions
    {
        public static FileMetadata GetFileMetadata(this ContentDetailViewItem content)
        {
            return ((JObject)(content.Metadata as dynamic)[content.ContentSchemaId]).ToObject<FileMetadata>();
        }
    }
}