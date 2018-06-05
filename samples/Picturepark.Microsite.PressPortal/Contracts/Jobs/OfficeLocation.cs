using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.Microsite.PressPortal.Contracts.Jobs
{
    [PictureparkSchemaType(SchemaType.List)]
    [PictureparkSchemaType(SchemaType.Struct)]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.officeLocation.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.officeLocation.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{data.officeLocation.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Detail, TemplateEngine.DotLiquid, "{{data.officeLocation.name}}")]
    [PictureparkNameTranslation("x-default", "Office Locations")]
    [PictureparkNameTranslation("fr", "Bases opérationnelles à travers")]
    public class OfficeLocation : ReferenceObject
    {
        public string Name { get; set; }

        [PictureparkNameTranslation("x-default", "Country")]
        [PictureparkNameTranslation("en", "Country")]
        [PictureparkNameTranslation("de", "Land")]
        public Country Country { get; set; }

        [PictureparkNameTranslation("x-default", "Kind")]
        [PictureparkNameTranslation("en", "Kind")]
        [PictureparkNameTranslation("de", "Art")]
        public string Kind { get; set; }
    }
}
