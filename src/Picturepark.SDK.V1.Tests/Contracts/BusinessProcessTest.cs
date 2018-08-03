using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkSchema(SchemaType.List)]
    public class BusinessProcessTest
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
