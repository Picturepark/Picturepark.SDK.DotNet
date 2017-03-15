using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests.Contracts
{
	[PictureparkSchemaType(SchemaType.Content)]
	[PictureparkSchemaType(SchemaType.Layer)]
	[PictureparkSchemaType(SchemaType.List)]
	[PictureparkSchemaType(SchemaType.Struct)]
	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{Data.AllDataTypesContract.StringField}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{Data.AllDataTypesContract.StringField}}")]
	[PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{Data.AllDataTypesContract.StringField}}: {{Data.AllDataTypesContract.IntegerField}}")]

	[
		PictureparkNameTranslation("x-default", "All datatypes"),
		PictureparkNameTranslation("de", "Alle datatypen"),
		PictureparkDescriptionTranslation("x-default", "All datatypes for testing"),
		PictureparkDescriptionTranslation("de", "Alle Datentypen für Testing")
	]

	public class AllDataTypesContract
	{
		[PictureparkNameTranslation("x-default", "Yes or no")]
		public bool BooleanField { get; set; }

		// TODO: How to limit to date?
		public DateTime DateField { get; set; }

		public DateTime DateTimeField { get; set; }

		//// public List<DateTime> DateTimeArrayField { get; set; }

		public decimal DecimalField { get; set; }

		// Correct?
		////public Dictionary<string, string> DictionaryField { get; set; }

		// Correct?
		////public List<Dictionary<string, string>> DictionaryArrayField { get; set; }

		public GeoPoint GeoPointField { get; set; }

		public int IntegerField { get; set; }

		////public List<int> IntegerArrayField { get; set; }

		public SimpleReferenceObject SchemaItemField { get; set; }

		public List<SimpleReferenceObject> SchemaItemsField { get; set; }

		public SimpleObject ObjectField { get; set; }

		public List<SimpleObject> ObjectsField { get; set; }

		public string StringField { get; set; }

		public TranslatedStringDictionary TranslatedStringField { get; set; }

		////public List<string> StringArrayField { get; set; }

		// TODO: Use correct contract
		////public Dictionary<string, string> TranslatedStringField { get; set; }

		[PictureparkContentRelation(
			"RelationName",
			"{ 'Kind': 'TermFilter', 'Field': 'ContentType', Term: 'Bitmap' }"
		)]
		public SimpleRelation RelationField { get; set; }

		// TODO: Use correct contract
		////public List<SimpleRelation> RelationsField { get; set; }
	}

	[PictureparkSchemaType(SchemaType.List)]
	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{Data.SimpleReferenceObject.NameField}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{Data.SimpleReferenceObject.NameField}}")]
	public class SimpleReferenceObject : IReference
	{
		public string NameField { get; set; }

		public string refId { get; set; }
	}

	[PictureparkSchemaType(SchemaType.Struct)]
	public class SimpleObject
	{
		public string Name { get; set; }
	}

	// TODO: Use correct contract
	[PictureparkSchemaType(SchemaType.Struct)]
	public class SimpleRelation
	{
		public string RelationInfo { get; set; }
	}
}
