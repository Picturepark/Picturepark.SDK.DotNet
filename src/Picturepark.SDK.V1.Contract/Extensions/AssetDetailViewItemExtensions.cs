using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class AssetDetailViewItemExtensions
    {
        public static FileMetadata GetFileMetadata(this AssetDetailViewItem asset)
        {
            return ((JObject)(asset.Metadata as dynamic)[asset.ContentMetadataSchemaId]).ToObject<FileMetadata>();
        }
    }
}