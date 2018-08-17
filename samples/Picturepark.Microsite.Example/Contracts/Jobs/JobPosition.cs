using Picturepark.SDK.V1.Contract;
using System.Collections.Generic;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.Microsite.Example.Contracts.Jobs
{
	[PictureparkSchema(SchemaType.List)]
	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.jobPosition.title.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.jobPosition.title.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{data.jobPosition.title.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.Detail, TemplateEngine.DotLiquid, "{{data.jobPosition.title.x-default}}")]
	[PictureparkNameTranslation("x-default", "Job Positions")]
	[PictureparkNameTranslation("fr", "Postes vacants")]
	public class JobPosition : ReferenceObject
	{
		[PictureparkSearch(Index = true, SimpleSearch = true, Boost = 10)]
		public TranslatedStringDictionary Title { get; set; }

		[PictureparkSearch(Index = true, SimpleSearch = true, Boost = 10)]
		public TranslatedStringDictionary Description { get; set; }

		public List<ContactInformation> ContactInformation { get; set; } = new List<ContactInformation>();

		public List<RequiredSkill> RequiredSkills { get; set; } = new List<RequiredSkill>();

		public List<OfficeLocation> OfficeLocations { get; set; } = new List<OfficeLocation>();
	}
}
