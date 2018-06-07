using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.SystemTypes;

namespace Picturepark.Microsite.PressPortal.Contracts
{
    [PictureparkSchemaType(SchemaType.Struct)]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.keyVisual.usage | translate: language}}")]
    [PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.keyVisual.usage | translate: language}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{data.keyVisual.usage | translate: language}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Detail, TemplateEngine.DotLiquid, "{{data.keyVisual.usage | translate: language}}")]
    [PictureparkNameTranslation("x-default", "Usage Information")]
    [PictureparkNameTranslation("en", "Usage Information")]
    [PictureparkNameTranslation("de", "Nutzungsinformationen")]
    [PictureparkDescriptionTranslation("x-default", "Information about usage of the content used in press releases.")]
    [PictureparkDescriptionTranslation("en", "Information about usage of the content used in press releases.")]
    [PictureparkDescriptionTranslation("de", "Nutzungsinformationen über den Content für Press Releases.")]
    public class KeyVisual : Relation
    {
        [PictureparkNameTranslation("x-default", "Caption")]
        [PictureparkNameTranslation("en", "Caption")]
        [PictureparkNameTranslation("de", "Bildunterschrift")]
        [PictureparkNameTranslation("fr", "Caption")]
        public TranslatedStringDictionary Usage { get; set; }
    }
}
