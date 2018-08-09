using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Attributes.Analyzer;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkSchema(SchemaType.Layer)]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.simpleLayer.name}}")]
    public class SimpleLayer
    {
        [PictureparkSimpleAnalyzer(SimpleSearch = true)]
        public string Name { get; set; }
    }
}
