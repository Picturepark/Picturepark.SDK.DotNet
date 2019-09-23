using System.Collections.Generic;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class MetadataExtensions
    {
        public static IDictionary<string, object> AsMetadata(this object obj) => Metadata.From(obj);
    }
}