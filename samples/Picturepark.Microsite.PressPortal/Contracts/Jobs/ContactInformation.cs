using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.Microsite.PressPortal.Contracts.Jobs
{
    [PictureparkSchemaType(SchemaType.List)]
    [PictureparkSchemaType(SchemaType.Struct)]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.contactInformation.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.ContactInformation.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{data.contactInformation.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Detail, TemplateEngine.DotLiquid, "{{data.contactInformation.name}}")]
    [PictureparkNameTranslation("x-default", "Contact Information")]
    [PictureparkNameTranslation("fr", "Données de contact")]
    public class ContactInformation
    {
        public string Name { get; set; }

        public string Email { get; set; }

        [PictureparkNameTranslation("x-default", "Adress")]
        [PictureparkNameTranslation("en", "Adress")]
        [PictureparkNameTranslation("de", "Adresse")]
        public string Adress { get; set; }
    }
}
