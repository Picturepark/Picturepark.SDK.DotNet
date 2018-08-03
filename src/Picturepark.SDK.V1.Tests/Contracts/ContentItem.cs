using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkSchema(SchemaType.Content)]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.contentItem.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.contentItem.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Detail, TemplateEngine.DotLiquid, "{{data.contentItem.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{data.contentItem.name}}")]
    public class ContentItem
    {
        public string Name { get; set; }
    }
}
