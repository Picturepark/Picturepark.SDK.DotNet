using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using System.Collections.Generic;

namespace Picturepark.Microsite.Example.Contracts.Jobs
{
	[PictureparkSchema(SchemaType.List)]
	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.requiredSkill.name.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.requiredSkill.name.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{data.requiredSkill.name.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.Detail, TemplateEngine.DotLiquid, "{{data.requiredSkill.name.x-default}}")]
	[PictureparkNameTranslation("x-default", "Required Skill")]
	[PictureparkNameTranslation("fr", "Exigences")]
	public class RequiredSkill : ReferenceObject
	{
		[PictureparkRequired]
		public TranslatedStringDictionary Name { get; set; }

		[PictureparkNameTranslation("x-default", "Description")]
		[PictureparkNameTranslation("en", "Description")]
		[PictureparkNameTranslation("de", "Beschreibung")]
		public TranslatedStringDictionary Description { get; set; }

		[PictureparkNameTranslation("x-default", "Kind")]
		[PictureparkNameTranslation("en", "Kind")]
		[PictureparkNameTranslation("de", "Art")]
		public string Kind { get; set; }

		[PictureparkNameTranslation("x-default", "Solutions")]
		[PictureparkNameTranslation("en", "Solutions")]
		[PictureparkNameTranslation("de", "Lösungen")]
		public List<Solution> Solutions { get; set; } = new List<Solution>();
	}
}
