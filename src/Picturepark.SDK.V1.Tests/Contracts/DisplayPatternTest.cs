using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkSchema(SchemaType.Content)]
    [PictureparkDisplayPattern(TemplateEngine.DotLiquid, "{{data.displayPatternTest.name}}")]
    public class DisplayPatternTest
    {
        public string Name { get; set; }
    }
}
