using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkSchemaType(SchemaType.List)]
    public class Tag
    {
        public string Name { get; set; }
    }
}
