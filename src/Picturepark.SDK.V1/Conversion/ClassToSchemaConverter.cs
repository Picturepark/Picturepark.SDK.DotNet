using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Picturepark.SDK.V1.Contract.Attributes.Analyzer;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Picturepark.SDK.V1.Contract.SystemTypes;
using ReferenceObject = Picturepark.SDK.V1.Contract.ReferenceObject;

namespace Picturepark.SDK.V1.Conversion
{
    /// <summary>Converts .NET types to Picturepark schemas.</summary>
    public class ClassToSchemaConverter
    {
        private readonly string _defaultLanguage;
        private readonly IContractResolver _contractResolver;
        private readonly HashSet<string> _ignoredProperties = new HashSet<string> { "_refId", "_relationType", "_targetDocType", "_targetId" };

        public ClassToSchemaConverter(string defaultLanguage)
            : this(new CamelCasePropertyNamesContractResolver())
        {
            _defaultLanguage = defaultLanguage;
        }

        public ClassToSchemaConverter(IContractResolver contractResolver)
        {
            _contractResolver = contractResolver;
        }

        public static string ResolveSchemaName(Type contract)
        {
            return contract.GetTypeInfo().GetCustomAttribute<PictureparkSchemaAttribute>()?.Name ??
                   contract.Name;
        }

        /// <summary>Converts a .NET type and its dependencies to a list of Picturepark schema definitions.</summary>
        /// <param name="type">The type to generate definitions for.</param>
        /// <param name="generateRelatedSchemas">Generates related schemas as well. E.g. referenced pocos in lists.</param>
        /// <returns>List of schemas</returns>
        public Task<ICollection<SchemaDetail>> GenerateAsync(Type type, bool generateRelatedSchemas = true)
        {
            return GenerateAsync(type, new List<SchemaDetail>(), generateRelatedSchemas);
        }

        /// <summary>Converts a .NET type and its dependencies to a list of Picturepark schema definitions.</summary>
        /// <param name="type">The type to generate definitions for.</param>
        /// <param name="schemaDetails">Existing list of schemas. Pass if you need to convert several pocos and they reference the same dependant schemas (used to exclude existing schemas).</param>
        /// <param name="generateRelatedSchemas">Generates related schemas as well. E.g. referenced pocos in lists.</param>
        /// <returns>List of schemas</returns>
        public Task<ICollection<SchemaDetail>> GenerateAsync(Type type, IEnumerable<SchemaDetail> schemaDetails, bool generateRelatedSchemas = true)
        {
            var properties = GetProperties(type);

            var schemaList = schemaDetails.ToList();
            CreateSchemas(type, properties, schemaList, 0, generateRelatedSchemas);

            var sortedList = new List<SchemaDetail>();
            foreach (var schemaItem in schemaList)
            {
                var dependencyList = schemaList.FindAll(s => s.Dependencies.Any(d => d.Id == schemaItem.Id));

                int? index = null;
                if (dependencyList.Any())
                {
                    foreach (var dependency in dependencyList)
                    {
                        var dependencyIndex = sortedList.FindIndex(s => s.Id == dependency.Id);
                        if (dependencyIndex == -1)
                            continue;

                        if (!index.HasValue || index.Value > dependencyIndex)
                            index = dependencyIndex;
                    }
                }

                if (index.HasValue)
                    sortedList.Insert(index.Value, schemaItem);
                else
                    sortedList.Add(schemaItem);
            }

            return Task.FromResult((ICollection<SchemaDetail>)sortedList);
        }

        private SchemaDetail CreateSchemas(Type contractType, List<ContractPropertyInfo> properties, List<SchemaDetail> schemaList, int levelOfCall = 0, bool generateDependencySchema = true)
        {
            var schemaId = ResolveSchemaName(contractType);
            if (schemaList.Any(s => s.Id == schemaId))
                return null;

            // Create schema for base class
            var parentSchemaId = string.Empty;
            var baseType = contractType.GetTypeInfo().BaseType;
            if (baseType != null &&
                baseType != typeof(object) &&
                baseType != typeof(Relation) &&
                baseType != typeof(ReferenceObject))
            {
                CreateSchemas(baseType, GetProperties(baseType), schemaList);
                parentSchemaId = ResolveSchemaName(baseType);
            }

            var typeAttributes = contractType.GetTypeInfo()
                .GetCustomAttributes<PictureparkSchemaAttribute>(true)
                .ToList();

            if (!typeAttributes.Any())
                throw new Exception("No PictureparkSchemaTypeAttribute set on class: " + contractType.Name);

            var types = typeAttributes
                .Select(typeAttribute => typeAttribute.Type)
                .ToList();

            var schemaItem = new SchemaDetail
            {
                Id = schemaId,
                Fields = new List<FieldBase>(),
                FieldsOverwrite = new List<FieldOverwriteBase>(),
                ParentSchemaId = parentSchemaId,
                Names = new TranslatedStringDictionary { { _defaultLanguage, schemaId } },
                Descriptions = new TranslatedStringDictionary(),
                Types = types,
                DisplayPatterns = new List<DisplayPattern>()
            };

            ApplyDisplayPatternAttributes(schemaItem, contractType);
            ApplyNameTranslationAttributes(schemaItem, contractType);
            ApplyDescriptionTranslationAttributes(schemaItem, contractType);

            var customTypes = properties.FindAll(c => c.IsCustomType);
            if (customTypes.Any())
            {
                foreach (var customType in customTypes)
                {
                    var referencedSchemaId = customType.TypeName;
                    if (schemaList.Any(d => d.Id == referencedSchemaId))
                        continue;

                    // Exclusion, if the customType is the contractType (it would create itself again with zero fields)
                    if (customType.FullName == contractType.FullName)
                        continue;

                    var type = Type.GetType($"{customType.FullName}, {customType.AssemblyFullName}");

                    var isSystemSchema = type.GetTypeInfo().GetCustomAttributes(typeof(PictureparkSystemSchemaAttribute), true).Any();
                    if (isSystemSchema == false)
                    {
                        var subLevelOfcall = levelOfCall + 1;
                        var dependencySchema = CreateSchemas(type, customType.TypeProperties, schemaList, subLevelOfcall, generateDependencySchema);

                        // the schema can be alredy added as dependency
                        if (schemaItem.Dependencies.Any(d => d.Id == referencedSchemaId) == false)
                            schemaItem.Dependencies.Add(dependencySchema);

                        // the schema can be alredy created
                        if (schemaList.Any(d => d.Id == referencedSchemaId) == false && generateDependencySchema)
                            schemaList.Add(dependencySchema);
                    }
                }
            }

            foreach (var property in properties)
            {
                if (property.IsOverwritten)
                {
                    var fieldOverwrite = GetFieldOverwrite(property);
                    schemaItem.FieldsOverwrite.Add(fieldOverwrite);
                }
                else
                {
                    var field = GetField(property);
                    schemaItem.Fields.Add(field);
                }
            }

            if (generateDependencySchema || levelOfCall == 0)
            {
                // the schema can be already created
                if (schemaList.Find(s => s.Id == schemaItem.Id) == null)
                    schemaList.Add(schemaItem);
            }

            // Create schemas for all related known types
            foreach (var knownType in contractType.GetKnownTypes())
                CreateSchemas(knownType, GetProperties(knownType), schemaList);

            return schemaItem;
        }

        private FieldOverwriteBase GetFieldOverwrite(ContractPropertyInfo property)
        {
            var tagboxAttribute = property.PictureparkAttributes
                .OfType<PictureparkTagboxAttribute>()
                .SingleOrDefault();

            var listItemCreateTemplateAttribute = property.PictureparkAttributes
                .OfType<PictureparkListItemCreateTemplateAttribute>()
                .SingleOrDefault();

            if (property.IsArray)
            {
                if (property.IsReference)
                {
                    return new FieldOverwriteMultiTagbox
                    {
                        Id = property.Name,
                        Filter = tagboxAttribute?.Filter,
                        Required = property.PictureparkAttributes.OfType<PictureparkRequiredAttribute>().Any(),
                        ListItemCreateTemplate = listItemCreateTemplateAttribute?.ListItemCreateTemplate,
                        OverwriteListItemCreateTemplate = !string.IsNullOrEmpty(listItemCreateTemplateAttribute?.ListItemCreateTemplate)
                    };
                }
                else
                {
                    throw new InvalidOperationException("Only Tagbox properties can be overriden.");
                }
            }
            else
            {
                if (property.IsReference)
                {
                    return new FieldOverwriteSingleTagbox
                    {
                        Id = property.Name,
                        Filter = tagboxAttribute?.Filter,
                        Required = property.PictureparkAttributes.OfType<PictureparkRequiredAttribute>().Any(),
                        ListItemCreateTemplate = listItemCreateTemplateAttribute?.ListItemCreateTemplate,
                        OverwriteListItemCreateTemplate = !string.IsNullOrEmpty(listItemCreateTemplateAttribute?.ListItemCreateTemplate)
                    };
                }
                else
                {
                    throw new InvalidOperationException("Only Tagbox properties can be overriden.");
                }
            }
        }

        private void ApplyDescriptionTranslationAttributes(SchemaDetail schemaDetail, Type type)
        {
            var descriptionTranslationAttributes = type.GetTypeInfo()
                .GetCustomAttributes(typeof(PictureparkDescriptionTranslationAttribute), true)
                .Select(i => i as PictureparkDescriptionTranslationAttribute)
                .ToList();

            foreach (var translationAttribute in descriptionTranslationAttributes)
            {
                var language = string.IsNullOrEmpty(translationAttribute.LanguageAbbreviation)
                    ? _defaultLanguage
                    : translationAttribute.LanguageAbbreviation;
                schemaDetail.Descriptions[language] = translationAttribute.Translation;
            }
        }

        private void ApplyNameTranslationAttributes(SchemaDetail schemaDetail, Type type)
        {
            var nameTranslationAttributes = type.GetTypeInfo()
                .GetCustomAttributes(typeof(PictureparkNameTranslationAttribute), true)
                .Select(i => i as PictureparkNameTranslationAttribute)
                .ToList();

            foreach (var translationAttribute in nameTranslationAttributes)
            {
                var language = string.IsNullOrEmpty(translationAttribute.LanguageAbbreviation)
                    ? _defaultLanguage
                    : translationAttribute.LanguageAbbreviation;
                schemaDetail.Names[language] = translationAttribute.Translation;
            }
        }

        private void ApplyDisplayPatternAttributes(SchemaDetail schemaDetail, Type contractType)
        {
            var displayPatternAttributes = contractType.GetTypeInfo()
                .GetCustomAttributes<PictureparkDisplayPatternAttribute>(true);

            foreach (var displayPatternAttribute in displayPatternAttributes.GroupBy(g => new { g.Type, g.TemplateEngine }))
            {
                if (displayPatternAttribute.GroupBy(x => x.Language).Any(i => i.Count() > 1))
                {
                    throw new InvalidOperationException("Multiple display patterns for the same language are defined.");
                }

                // If no type is specified, set for all the types.
                var types = displayPatternAttribute.Key.Type.HasValue
                    ? new[] { displayPatternAttribute.Key.Type.Value }
                    : Enum.GetValues(typeof(DisplayPatternType)).OfType<DisplayPatternType>();

                foreach (var type in types)
                {
                    schemaDetail.DisplayPatterns.Add(new DisplayPattern
                    {
                        DisplayPatternType = type,
                        TemplateEngine = displayPatternAttribute.Key.TemplateEngine,
                        Templates = new TranslatedStringDictionary(displayPatternAttribute.ToDictionary(x => string.IsNullOrEmpty(x.Language) ? _defaultLanguage : x.Language, x => x.DisplayPattern))
                    });
                }
            }
        }

        private List<ContractPropertyInfo> GetProperties(Type type)
        {
            var contactPropertiesInfo = new List<ContractPropertyInfo>();

            var objectContract = _contractResolver.ResolveContract(type) as JsonObjectContract;
            if (objectContract != null)
            {
                foreach (var property in objectContract.Properties.Where(p => p.DeclaringType == type))
                {
                    var typeInfo = property.PropertyType.GetTypeInfo();
                    var name = property.PropertyName;

                    // Check if name is overridden by JsonProperty attribute
                    var attributes = property.AttributeProvider.GetAttributes(false);
                    var jsonProperty = attributes.OfType<JsonPropertyAttribute>().FirstOrDefault();
                    if (jsonProperty != null)
                        name = jsonProperty.PropertyName;

                    // Skip ignored properties
                    if (_ignoredProperties.Contains(name))
                        continue;

                    var propertyInfo = new ContractPropertyInfo()
                    {
                        Name = name,
                        IsOverwritten = type.GetTypeInfo().BaseType?.GetRuntimeProperty(property.UnderlyingName) != null
                    };

                    if (IsSimpleType(property.PropertyType))
                    {
                        HandleSimpleTypes(property, propertyInfo);
                    }
                    else
                    {
                        // either list or dictionary
                        if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(typeInfo))
                        {
                            if (typeInfo.ImplementedInterfaces.Contains(typeof(IDictionary)) ||
                                (typeInfo.GenericTypeArguments.Any() && typeInfo.GenericTypeArguments.First().GetTypeInfo().ImplementedInterfaces.Contains(typeof(IDictionary))))
                            {
                                propertyInfo.IsArray = typeInfo.ImplementedInterfaces.Contains(typeof(IList));
                                propertyInfo.IsDictionary = true;
                                propertyInfo.TypeName = property.PropertyType.Name;
                            }
                            else
                            {
                                var propertyGenericArg = typeInfo.GenericTypeArguments.First();

                                if (IsSimpleType(propertyGenericArg))
                                {
                                    HandleSimpleTypes(property, propertyInfo);
                                }
                                else
                                {
                                    propertyInfo.IsCustomType = true;
                                    propertyInfo.TypeName = propertyGenericArg.Name;
                                    propertyInfo.FullName = propertyGenericArg.FullName;
                                    propertyInfo.AssemblyFullName = propertyGenericArg.GetTypeInfo().Assembly.FullName;

                                    // Check for prevention of an infinite loop
                                    if (propertyGenericArg.FullName != type.FullName)
                                    {
                                        propertyInfo.TypeProperties.AddRange(
                                            GetProperties(propertyGenericArg));
                                    }
                                }

                                propertyInfo.IsArray = true;

                                if (attributes.OfType<PictureparkReferenceAttribute>().Any() ||
                                    property.PropertyType.GenericTypeArguments.FirstOrDefault().GetTypeInfo().GetCustomAttribute<PictureparkReferenceAttribute>() != null)
                                {
                                    propertyInfo.IsReference = true;
                                }
                            }
                        }
                        else
                        {
                            propertyInfo.IsCustomType = true;
                            propertyInfo.TypeName = property.PropertyType.Name;
                            propertyInfo.FullName = property.PropertyType.FullName;
                            propertyInfo.AssemblyFullName = typeInfo.Assembly.FullName;

                            if (typeInfo.GetCustomAttribute<PictureparkReferenceAttribute>() != null)
                            {
                                propertyInfo.IsReference = true;
                            }

                            // Check for prevention of an infinite loop
                            if (property.PropertyType.FullName != type.FullName)
                            {
                                propertyInfo.TypeProperties.AddRange(
                                    GetProperties(property.PropertyType));
                            }
                        }
                    }

                    propertyInfo.PictureparkAttributes = property.AttributeProvider
                        .GetAttributes(true)
                        .Select(i => i as IPictureparkAttribute)
                        .Where(i => i != null)
                        .ToList();

                    contactPropertiesInfo.Add(propertyInfo);
                }
            }

            return contactPropertiesInfo;
        }

        private void HandleSimpleTypes(JsonProperty property, ContractPropertyInfo propertyInfo)
        {
            var typeInfo = property.PropertyType.GetTypeInfo();
            propertyInfo.IsSimpleType = true;

            // it's a case of: nullable / enum type property
            if (typeInfo.GenericTypeArguments != null && typeInfo.GenericTypeArguments.Length > 0)
            {
                var propertyGenericArg = property.PropertyType.GenericTypeArguments.First();
                var underlyingType = Nullable.GetUnderlyingType(propertyGenericArg);
                propertyGenericArg = underlyingType ?? propertyGenericArg;

                propertyInfo.FullName = propertyGenericArg.FullName;
                propertyInfo.AssemblyFullName = propertyGenericArg.GetTypeInfo().Assembly.FullName;

                if (propertyGenericArg.GetTypeInfo().IsEnum)
                {
                    propertyInfo.IsEnum = true;
                    propertyInfo.TypeName = propertyGenericArg.Name;
                }
                else
                {
                    if (propertyGenericArg == typeof(DateTimeOffset))
                    {
                        propertyInfo.TypeName = TypeCode.DateTime.ToString();
                    }
                    else
                    {
                        // TODO - Check with Rico: Find better solution for this
                        propertyInfo.TypeName = typeof(Type)
                            .GetRuntimeMethod("GetTypeCode", new[] { typeof(Type) })
                            .Invoke(null, new object[] { propertyGenericArg })
                            .ToString();
                    }
                }
            }
            else
            {
                if (property.PropertyType == typeof(DateTimeOffset))
                {
                    propertyInfo.TypeName = TypeCode.DateTime.ToString();
                }
                else
                {
                    propertyInfo.TypeName = typeof(Type)
                        .GetRuntimeMethod("GetTypeCode", new[] { typeof(Type) })
                        .Invoke(null, new object[] { property.PropertyType })
                        .ToString();
                }

                propertyInfo.FullName = property.PropertyType.FullName;
                propertyInfo.AssemblyFullName = typeInfo.Assembly.FullName;
            }
        }

        private FieldBase GetField(ContractPropertyInfo property)
        {
            FieldBase field = null;

            if (property.IsDictionary)
            {
                if (property.TypeName == "TranslatedStringDictionary")
                {
                    field = new FieldTranslatedString
                    {
                        Required = false,
                        Fixed = false,
                        Index = true,
                        SimpleSearch = true,
                        MultiLine = false,
                        Boost = 1,
                        IndexAnalyzers = new List<AnalyzerBase>
                        {
                            new LanguageAnalyzer()
                        },
                        SimpleSearchAnalyzers = new List<AnalyzerBase>
                        {
                            new LanguageAnalyzer()
                        }
                    };
                }
                else if (property.IsArray)
                {
                    field = new FieldDictionaryArray();
                }
                else
                {
                    field = new FieldDictionary();
                }
            }
            else if (property.IsEnum)
            {
                throw new NotSupportedException("Enum types are not supported in Class to Schema convertion");
            }
            else if (property.IsSimpleType)
            {
                if (!Enum.TryParse(property.TypeName, out TypeCode typeCode))
                {
                    throw new Exception($"Parsing to TypeCode enumarated object failed for string value: {property.TypeName}.");
                }

                if (property.IsArray)
                {
                    switch (typeCode)
                    {
                        case TypeCode.String:
                            field = new FieldStringArray
                            {
                                Index = true
                            };
                            break;

                        case TypeCode.DateTime:
                            field = new FieldDateTimeArray
                            {
                                Index = true
                            };
                            break;

                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                            field = new FieldLongArray
                            {
                                Index = true
                            };
                            break;

                        default:
                            throw new Exception($"TypeCode {typeCode} is not supported.");
                    }
                }
                else
                {
                    var stringInfos = property.PictureparkAttributes.OfType<PictureparkStringAttribute>().SingleOrDefault();

                    switch (typeCode)
                    {
                        case TypeCode.String:
                            field = new FieldString
                            {
                                Index = true,
                                SimpleSearch = true,
                                Boost = 1,
                                IndexAnalyzers = new List<AnalyzerBase>
                                {
                                    new SimpleAnalyzer()
                                },
                                SimpleSearchAnalyzers = new List<AnalyzerBase>
                                {
                                    new SimpleAnalyzer()
                                },
                                MultiLine = stringInfos?.MultiLine ?? false
                            };
                            break;
                        case TypeCode.DateTime:
                            field = CreateDateTypeField(property);

                            break;
                        case TypeCode.Boolean:
                            field = new FieldBoolean
                            {
                                Index = true
                            };
                            break;
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                            field = new FieldLong
                            {
                                Index = true
                            };
                            break;
                        case TypeCode.Decimal:
                        case TypeCode.Double:
                        case TypeCode.Single:
                            field = new FieldDecimal
                            {
                                Index = true
                            };
                            break;
                        default:
                            throw new Exception($"TypeCode {typeCode} is not supported.");
                    }
                }
            }
            else
            {
                var schemaIndexingAttribute = property.PictureparkAttributes.OfType<PictureparkSchemaIndexingAttribute>().SingleOrDefault();
                var listItemCreateTemplateAttribute = property.PictureparkAttributes.OfType<PictureparkListItemCreateTemplateAttribute>().SingleOrDefault();
                var tagboxAttributes = property.PictureparkAttributes.OfType<PictureparkTagboxAttribute>().SingleOrDefault();
                var contentRelationAttributes = property.PictureparkAttributes.OfType<PictureparkContentRelationAttribute>().ToList();

                var relationTypes = new List<RelationType>();
                if (contentRelationAttributes.Any())
                {
                    relationTypes = contentRelationAttributes.Select(i => new RelationType
                    {
                        Id = i.Name,
                        Filter = i.Filter,
                        TargetDocType = i.TargetDocType,
                        Names = new TranslatedStringDictionary { { _defaultLanguage, i.Name } }
                    }).ToList();
                }

                if (property.IsArray)
                {
                    if (contentRelationAttributes.Any())
                    {
                        field = new FieldMultiRelation
                        {
                            Index = true,
                            RelationTypes = relationTypes,
                            SchemaId = property.TypeName,
                            SchemaIndexingInfo = schemaIndexingAttribute?.SchemaIndexingInfo
                        };
                    }
                    else if (property.IsReference)
                    {
                        field = new FieldMultiTagbox
                        {
                            Index = true,
                            SimpleSearch = true,
                            SchemaId = property.TypeName,
                            Filter = tagboxAttributes?.Filter,
                            SchemaIndexingInfo = schemaIndexingAttribute?.SchemaIndexingInfo,
                            ListItemCreateTemplate = listItemCreateTemplateAttribute?.ListItemCreateTemplate
                        };
                    }
                    else
                    {
                        field = new FieldMultiFieldset
                        {
                            Index = true,
                            SimpleSearch = true,
                            SchemaId = property.TypeName,
                            SchemaIndexingInfo = schemaIndexingAttribute?.SchemaIndexingInfo
                        };
                    }
                }
                else
                {
                    if (contentRelationAttributes.Any())
                    {
                        field = new FieldSingleRelation
                        {
                            Index = true,
                            SimpleSearch = true,
                            RelationTypes = relationTypes,
                            SchemaId = property.TypeName,
                            SchemaIndexingInfo = schemaIndexingAttribute?.SchemaIndexingInfo
                        };
                    }
                    else if (property.TypeName == "GeoPoint")
                    {
                        field = new FieldGeoPoint
                        {
                            Index = true
                        };
                    }
                    else if (property.IsReference)
                    {
                        field = new FieldSingleTagbox
                        {
                            Index = true,
                            SimpleSearch = true,
                            SchemaId = property.TypeName,
                            Filter = tagboxAttributes?.Filter,
                            SchemaIndexingInfo = schemaIndexingAttribute?.SchemaIndexingInfo,
                            ListItemCreateTemplate = listItemCreateTemplateAttribute?.ListItemCreateTemplate
                        };
                    }
                    else
                    {
                        field = new FieldSingleFieldset
                        {
                            Index = true,
                            SimpleSearch = true,
                            SchemaId = property.TypeName,
                            SchemaIndexingInfo = schemaIndexingAttribute?.SchemaIndexingInfo
                        };
                    }
                }
            }

            if (field == null)
                throw new Exception($"Could not find type for {property.Name}");

            foreach (var attribute in property.PictureparkAttributes)
            {
                if (attribute is PictureparkSearchAttribute searchAttribute)
                {
                    field.Index = searchAttribute.Index;
                    field.SimpleSearch = searchAttribute.SimpleSearch;

                    if (field.GetType().GetRuntimeProperty("Boost") != null)
                    {
                        field.GetType().GetRuntimeProperty("Boost").SetValue(field, searchAttribute.Boost);
                    }
                }

                if (attribute is PictureparkRequiredAttribute)
                {
                    field.Required = true;
                }

                if (attribute is PictureparkMaximumLengthAttribute maxLengthAttribute)
                {
                    field.GetType().GetRuntimeProperty("MaximumLength").SetValue(field, maxLengthAttribute.Length);
                }

                if (attribute is PictureparkPatternAttribute patternAttribute)
                {
                    field.GetType().GetRuntimeProperty("Pattern").SetValue(field, patternAttribute.Pattern);
                }

                if (attribute is PictureparkNameTranslationAttribute nameTranslationAttribute)
                {
                    if (field.Names == null)
                        field.Names = new TranslatedStringDictionary();

                    var language = string.IsNullOrEmpty(nameTranslationAttribute.LanguageAbbreviation)
                        ? _defaultLanguage
                        : nameTranslationAttribute.LanguageAbbreviation;

                    field.Names[language] = nameTranslationAttribute.Translation;
                }

                if (attribute is PictureparkSortAttribute)
                {
                    if (field is FieldSingleRelation || field is FieldMultiRelation)
                    {
                        throw new InvalidOperationException($"Relation property {property.Name} must not be marked as sortable.");
                    }

                    if (field is FieldGeoPoint)
                    {
                        throw new InvalidOperationException($"GeoPoint property {property.Name} must not be marked as sortable.");
                    }

                    field.Sortable = true;
                }
            }

            var fieldName = property.Name;
            field.Id = fieldName.ToLowerCamelCase();

            if (field.Names == null)
            {
                field.Names = new TranslatedStringDictionary
                {
                    [_defaultLanguage] = fieldName
                };
            }

            if (property.PictureparkAttributes.OfType<PictureparkAnalyzerAttribute>().Any(a => !a.Index && !a.SimpleSearch))
            {
                throw new InvalidOperationException(
                    $"Property {property.Name} has invalid analyzer configuration: Specify one or both of {nameof(PictureparkAnalyzerAttribute.Index)}, {nameof(PictureparkAnalyzerAttribute.SimpleSearch)}.");
            }

            var fieldIndexAnalyzers = property.PictureparkAttributes
                .OfType<PictureparkAnalyzerAttribute>()
                .Where(a => a.Index)
                .Select(a => a.CreateAnalyzer())
                .ToList();

            if (fieldIndexAnalyzers.Any())
                field.GetType().GetRuntimeProperty("IndexAnalyzers").SetValue(field, fieldIndexAnalyzers);

            var fieldSimpleSearchAnalyzers = property.PictureparkAttributes
                .OfType<PictureparkAnalyzerAttribute>()
                .Where(a => a.SimpleSearch)
                .Select(a => a.CreateAnalyzer())
                .ToList();

            if (fieldSimpleSearchAnalyzers.Any())
                field.GetType().GetRuntimeProperty("SimpleSearchAnalyzers").SetValue(field, fieldSimpleSearchAnalyzers);

            return field;
        }

        private FieldBase CreateDateTypeField(ContractPropertyInfo property)
        {
            var dateAttribute =
                property.PictureparkAttributes.OfType<PictureparkDateTypeAttribute>().FirstOrDefault();
            var format = dateAttribute?.Format;

            if (dateAttribute == null || dateAttribute.ContainsTimePortion)
                return new FieldDateTime { Index = true, Format = format };
            else
                return new FieldDate { Index = true, Format = format };
        }

        private bool IsSimpleType(Type type)
        {
            return
                type.GetTypeInfo().IsValueType ||
                type.GetTypeInfo().IsPrimitive ||
                new[]
                {
                    typeof(string),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }
    }
}
