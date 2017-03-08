using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class ContentDetailViewItemExtensions
    {
        public static FileMetadata GetFileMetadata(this ContentDetail content)
        {
            return (content.Content as JObject).ToObject<FileMetadata>();
        }
    }
}