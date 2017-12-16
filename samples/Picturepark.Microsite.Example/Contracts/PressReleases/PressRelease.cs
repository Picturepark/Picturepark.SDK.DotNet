using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using System;
using System.Collections.Generic;

namespace Picturepark.Microsite.Example.Contracts
{
	[PictureparkSchemaType(SchemaType.Content)]
	[PictureparkSchemaType(SchemaType.List)]
	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.pressRelease.headline.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.pressRelease.headline.x-default}}")]
	[PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "<table width='90%' > <tbody> <tr> <td> {{data.pressRelease.teaser.x-default}}</td> </tr> <tr> <td><br /></td> </tr> <tr> <td>{{data.pressRelease.text.x-default}}</td> </tr> </tbody> </table>")]
	[PictureparkDisplayPattern(DisplayPatternType.Detail, TemplateEngine.DotLiquid, "<table width='94%' > <tbody> <tr>   <td><h2>Headline: </h2></td> <td><h2> {{data.pressRelease.headline.x-default}}</h2></td> </tr> <tr> <td><h3>Teaser: <h3></td> <td><h3> {{data.pressRelease.teaser.x-default}}<h3></td> </tr> <tr> <td>Text:</td> <td>{{data.pressRelease.text.x-default}}</td> </tr> <tr> <td></td> <td><br /></td> </tr> <tr> <td>Publish Date:&nbsp;</td> <td>{{data.pressRelease.publishDate}}</td> </tr> </tbody> </table>")]
	[PictureparkNameTranslation("x-default", "Press Kit")]
	[PictureparkNameTranslation("en", "Press Kit")]
	[PictureparkNameTranslation("de", "Press Kit")]
	[PictureparkDescriptionTranslation("x-default", "Press Release content to publish from Picturepark Content Platform")]
	[PictureparkDescriptionTranslation("en", "Press Release content to publish from Picturepark Content Platform")]
	[PictureparkDescriptionTranslation("de", "Pressemitteilungen zum direkten Publizieren aus der Picturepark Content Platfrom")]

	public class PressRelease
	{
		[PictureparkSearch(Boost = 50, Index = true, SimpleSearch = true)]
		[PictureparkNameTranslation("x-default", "Headline")]
		public TranslatedStringDictionary Headline { get; set; }

		[PictureparkSearch(Boost = 20, Index = true, SimpleSearch = true)]
		[PictureparkNameTranslation("x-default", "Teaser")]
		public TranslatedStringDictionary Teaser { get; set; }

		[PictureparkString(MultiLine = true)]
		[PictureparkSearch(Boost = 10, Index = true, SimpleSearch = true)]
		[PictureparkNameTranslation("x-default", "Description")]
		public TranslatedStringDictionary Text { get; set; }

		[PictureparkContentRelation("KeyVisual", "")]
		[PictureparkNameTranslation("x-default", "Hero image")]
		[PictureparkNameTranslation("en", "Hero image")]
		[PictureparkNameTranslation("de", "Hero image")]
		[PictureparkNameTranslation("fr", "Clé visuel")]
		public KeyVisual KeyVisual { get; set; }

		[PictureparkContentRelation("KeyVisual", "")]
		[PictureparkNameTranslation("x-default", "Downloads")]
		public List<KeyVisual> Downloads { get; set; } = new List<KeyVisual>();

		[PictureparkNameTranslation("x-default", "Publish date")]
		[PictureparkNameTranslation("en", "Publish date")]
		[PictureparkNameTranslation("de", "Datum der Veröffentlichung")]
		[PictureparkNameTranslation("fr", "Date de publication")]
		public DateTime PublishDate { get; set; }
	}
}
