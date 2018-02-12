using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.SystemTypes;
using System;
using System.Collections.Generic;

namespace Picturepark.SDK.V1.Tests.Contracts
{
	[PictureparkSchemaType(SchemaType.Content)]
	[PictureparkSchemaType(SchemaType.Layer)]
	[PictureparkSchemaType(SchemaType.List)]
	[PictureparkSchemaType(SchemaType.Struct)]
	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.allDataTypesContract.stringField}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.allDataTypesContract.stringField}}")]
	[PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{data.allDataTypesContract.stringField}}: {{data.allDataTypesContract.integerField}}")]
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

		public SimpleReferenceObject SingleTagboxField { get; set; }

		public List<SimpleReferenceObject> MultiTagboxField { get; set; }

		public SimpleObject SingleFieldsetField { get; set; }

		public List<SimpleObject> MultiFieldsetField { get; set; }

		public string StringField { get; set; }

		public TranslatedStringDictionary TranslatedStringField { get; set; }

		////public List<string> StringArrayField { get; set; }

		[PictureparkContentRelation(
			"RelationName",
			"{ 'kind': 'TermFilter', 'field': 'contentType', term: 'Bitmap' }"
		)]
		public SimpleRelation RelationField { get; set; }

		[PictureparkContentRelation(
			"RelationsName",
			"{ 'kind': 'TermFilter', 'field': 'contentType', Term: 'bitmap' }"
		)]
		public List<SimpleRelation> RelationsField { get; set; }
	}

	[PictureparkReference]
	[PictureparkSchemaType(SchemaType.List)]
	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.simpleReferenceObject.nameField}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.simpleReferenceObject.nameField}}")]
	public class SimpleReferenceObject : ReferenceObject
	{
		public string NameField { get; set; }
	}

	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.simpleObject.name}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.simpleObject.name}}")]
	[PictureparkSchemaType(SchemaType.Struct)]
	public class SimpleObject
	{
		public string Name { get; set; }

		public NestedSimpleObject Nested { get; set; }
	}

	[PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.nestedSimpleObject.name}}")]
	[PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.nestedSimpleObject.name}}")]
	[PictureparkSchemaType(SchemaType.Struct)]
	public class NestedSimpleObject
	{
		public string Name { get; set; }
	}

	[PictureparkSchemaType(SchemaType.Struct)]
	public class SimpleRelation : Relation
	{
		public string RelationInfo { get; set; }
	}
}
