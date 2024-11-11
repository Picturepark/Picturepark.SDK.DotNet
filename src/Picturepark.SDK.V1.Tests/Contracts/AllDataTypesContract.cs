using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.SystemTypes;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract.Providers;
using Picturepark.SDK.V1.Conversion;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkSchema(SchemaType.Layer)]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.allDataTypesContract.stringField}}")]
    [PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.allDataTypesContract.stringField}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{data.allDataTypesContract.stringField}}: {{data.allDataTypesContract.integerField}}")]
    [
        PictureparkNameTranslation("All datatypes"),
        PictureparkNameTranslation("de", "Alle datatypen"),
        PictureparkDescriptionTranslation("All datatypes for testing"),
        PictureparkDescriptionTranslation("de", "Alle Datentypen für Testing")
    ]
    public class AllDataTypesContract
    {
        [PictureparkNameTranslation("Yes or no")]
        public bool? BooleanField { get; set; }

        [PictureparkDate]
        [JsonConverter(typeof(DateFieldConverter))]
        public DateTime? DateField { get; set; }

        [PictureparkDateTime]
        public DateTime? DateTimeField { get; set; }

        public decimal? DecimalField { get; set; }

        public GeoPoint GeoPointField { get; set; }

        public int? IntegerField { get; set; }

        [PictureparkSchemaIndexing("{ 'includeNameDisplayValueInFilters': true, fields: [ { 'id': 'nameField', 'index': true, 'simpleSearch': true, 'boost': 1 } ] }")]
        public SimpleReferenceObject SingleTagboxField { get; set; }

        [PictureparkSchemaIndexing("{ 'includeNameDisplayValueInFilters': true, fields: [ { 'id': 'nameField', 'index': true, 'simpleSearch': true, 'boost': 1 } ] }")]
        public List<SimpleReferenceObject> MultiTagboxField { get; set; }

        public SimpleObject SingleFieldsetField { get; set; }

        public List<SimpleObject> MultiFieldsetField { get; set; }

        public string StringField { get; set; }

        public TranslatedStringDictionary TranslatedStringField { get; set; }

        [PictureparkContentRelation(
            "RelationName",
            "{ 'kind': 'TermFilter', 'field': 'contentType', term: 'Bitmap' }"
        )]
        [PictureparkRelationUiSettings(ItemFieldViewMode.ThumbSmall, maxListRows: 6, maxThumbRows: 3)]
        public SimpleRelation RelationField { get; set; }

        [PictureparkContentRelation(
            "RelationsName",
            "{ 'kind': 'TermFilter', 'field': 'contentType', Term: 'bitmap' }"
        )]
        public List<SimpleRelation> RelationsField { get; set; }

        [PictureparkDynamicView("{ 'kind': 'TermFilter', 'field': 'contentType', term: 'Bitmap' }")]
        [PictureparkDynamicViewUiSettings(ItemFieldViewMode.ThumbMedium, maxListRows: 5, maxThumbRows: 3, showRelatedContentOnDownload: true)]
        public DynamicViewObject DynamicViewField { get; set; }

        [PictureparkTreeView(levelProvider: typeof(TreeViewLevelProvider))]
        public TreeViewObject TreeViewObject { get; set; }
    }

    [PictureparkReference]
    [PictureparkSchema(SchemaType.List)]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.simpleReferenceObject.nameField}}")]
    [PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.simpleReferenceObject.nameField}}")]
    public class SimpleReferenceObject : ReferenceObject
    {
        public string NameField { get; set; }
    }

    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.simpleObject.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.simpleObject.name}}")]
    [PictureparkSchema(SchemaType.Struct)]
    public class SimpleObject
    {
        public string Name { get; set; }

        public NestedSimpleObject Nested { get; set; }
    }

    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.nestedSimpleObject.name}}")]
    [PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.nestedSimpleObject.name}}")]
    [PictureparkSchema(SchemaType.Struct)]
    public class NestedSimpleObject
    {
        public string Name { get; set; }
    }

    [PictureparkSchema(SchemaType.Struct)]
    public class SimpleRelation : Relation
    {
        public string RelationInfo { get; set; }
    }

    public class TreeViewLevelProvider : ITreeViewLevelProvider
    {
        public IReadOnlyList<TreeLevelItem> GetTreeLevels() =>
        [
            new()
            {
                FieldId = nameof(AllDataTypesContract.SingleTagboxField).ToLowerCamelCase(),
                Levels = Array.Empty<TreeLevelItem>(),
                MaxRecursions = 0
            }
        ];
    }
}
