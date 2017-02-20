using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public static class AssetDetailViewItemExtensions
    {
        public static FileMetadata GetFileMetadata(this AssetDetailViewItem asset)
        {
            return ((JObject)(asset.Metadata as dynamic)[asset.ContentMetadataSchemaId]).ToObject<FileMetadata>();
        }
    }
}