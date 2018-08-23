using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using System.Collections.Generic;

namespace Picturepark.Microsite.Example.Contracts.Jobs
{
	[PictureparkSchema(SchemaType.Content)]
	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.jobsAtPicturepark.headline.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.jobsAtPicturepark.headline.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{data.jobsAtPicturepark.headline.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.Detail, TemplateEngine.DotLiquid, "{{data.jobsAtPicturepark.contentId}}<br /><h3>{{data.jobsAtPicturepark.headline.x-default}}</h3><br />{{data.jobsAtPicturepark.headline.x-default}}<br /><br />{{data.jobsAtPicturepark.text.x-default}}")]
	[PictureparkNameTranslation("x-default", "Job Posts")]
	[PictureparkNameTranslation("fr", "Job Posts")]
	[PictureparkDescriptionTranslation("x-default", "Content to publish to Picturepark Jobs Website")]
	[PictureparkDescriptionTranslation("fr", "Contenu à publier sur le site de Picturepark")]
	public class JobsAtPicturepark
	{
		[PictureparkSearch(Boost = 50, Index = true, SimpleSearch = true)]
		public TranslatedStringDictionary Introduction { get; set; }

		[PictureparkSearch(Boost = 50, Index = true, SimpleSearch = true)]
		public TranslatedStringDictionary Headline { get; set; }

		[PictureparkString(MultiLine = true)]
		[PictureparkSearch(Boost = 10, Index = true, SimpleSearch = true)]
		public TranslatedStringDictionary Text { get; set; }

		[PictureparkNameTranslation("x-default", "Job Positions")]
		[PictureparkNameTranslation("en", "Job Positions")]
		[PictureparkNameTranslation("de", "Stellenangebote")]
		[PictureparkNameTranslation("fr", "Poste Vacants")]
		public List<JobPosition> Jobs { get; set; } = new List<JobPosition>();

		[JsonIgnore]
		[PictureparkNameTranslation("x-default", "Application Form")]
		[PictureparkNameTranslation("en", "Application Form")]
		[PictureparkNameTranslation("de", "Application Form")]
		[PictureparkNameTranslation("fr", "Application Form")]
		public string ContentId { get; set; }
	}
}
