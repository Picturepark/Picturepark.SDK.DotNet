using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkSchemaType(SchemaType.List)]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.displayLanguageTestItems.value1}}", "de")]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.displayLanguageTestItems.value2}}", "en")]
    public class DisplayLanguageTestItems
    {
        public string Value1 { get; set; }

        public string Value2 { get; set; }
    }
}