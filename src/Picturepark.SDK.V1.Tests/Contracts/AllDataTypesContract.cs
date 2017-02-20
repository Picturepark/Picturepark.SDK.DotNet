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
	[PictureparkSchemaType(MetadataSchemaType.AssetContent)]
	[PictureparkSchemaType(MetadataSchemaType.AssetLayer)]
	[PictureparkSchemaType(MetadataSchemaType.MetadataContent)]
	[PictureparkSchemaType(MetadataSchemaType.MetadataLayer)]
	[PictureparkSchemaType(MetadataSchemaType.Struct)]
	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{Metadata.AllDataTypesContract.StringField}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{Metadata.AllDataTypesContract.StringField}}")]
	[PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{Metadata.AllDataTypesContract.StringField}}: {{Metadata.AllDataTypesContract.IntegerField}}")]

	public class AllDataTypesContract
	{
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

		[PictureparkAssetRelation(
			"RelationName",
			"{ 'Kind': 'TermFilter', 'Field': 'AssetType', Term: 'Bitmap' }"
		)]
		public SimpleRelation RelationField { get; set; }

		// TODO: Use correct contract
		////public List<SimpleRelation> RelationsField { get; set; }
	}

	[PictureparkSchemaType(MetadataSchemaType.MetadataContent)]
	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{Metadata.SimpleReferenceObject.NameField}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{Metadata.SimpleReferenceObject.NameField}}")]
	public class SimpleReferenceObject : IReference
	{
		public string NameField { get; set; }

		public string refId { get; set; }
	}

	[PictureparkSchemaType(MetadataSchemaType.Struct)]
	public class SimpleObject
	{
		public string Name { get; set; }
	}

	// TODO: Use correct contract
	[PictureparkSchemaType(MetadataSchemaType.Struct)]
	public class SimpleRelation
	{
		public string RelationInfo { get; set; }
	}
}
