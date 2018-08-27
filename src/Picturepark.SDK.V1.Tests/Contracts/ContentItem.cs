using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkSchema(SchemaType.Content)]
    [PictureparkDisplayPattern(TemplateEngine.DotLiquid, "{{data.contentItem.name}}")]
    public class ContentItem
    {
        public string Name { get; set; }
    }

    [PictureparkSchema(SchemaType.Content)]
    [PictureparkDisplayPattern(TemplateEngine.DotLiquid, "{{data.contentItemWithTagBox.name}}")]
    public class ContentItemWithTagBox
    {
        public string Name { get; set; }

        public SimpleReferenceObject Object { get; set; }
    }
}
