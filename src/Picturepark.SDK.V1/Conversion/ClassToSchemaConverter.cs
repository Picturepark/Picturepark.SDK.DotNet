using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Attributes.Analyzer;
using Picturepark.SDK.V1.Contract.SystemTypes;

namespace Picturepark.SDK.V1.Conversion
{
    internal class ClassToSchemaConverter
    {
        private readonly string _defaultLanguage;
        private readonly IContractResolver _contractResolver;
        private readonly HashSet<string> _ignoredProperties = new HashSet<string> { "_refId", "_relationType", "_targetDocType", "_targetId", "_sourceDocType" };

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
        /// <param name="schemaDetails">Existing list of schemas. Pass if you need to convert several pocos and they reference the same dependent schemas (used to exclude existing schemas).</param>
        /// <param name="generateRelatedSchemas">Generates related schemas as well. E.g. referenced pocos in lists.</param>
        /// <returns>List of schemas</returns>
        public Task<ICollection<SchemaDetail>> GenerateAsync(Type type, IEnumerable<SchemaDetail> schemaDetails, bool generateRelatedSchemas = true)
        {
            var properties = GetProperties(type);
            if (!generateRelatedSchemas)
            {
                // only generated schema for current type
                properties = new List<ContractTypeInfo> { properties.Single(x => x.Type == type) };
            }

            var schemas = GenerateSchemas(properties, schemaDetails);
            return Task.FromResult(schemas);
        }

        private IReadOnlyList<ContractTypeInfo> GetProperties(Type type)
        {
            var result = new List<ContractTypeInfo>();

            var typesToReflect = new VisitedTypesStack();
            typesToReflect.Push(type);

            while (typesToReflect.HasMore())
            {
                var typeToReflect = typesToReflect.Pop();

                foreach (var knownType in typeToReflect.GetKnownTypes())
                {
                    typesToReflect.Push(knownType);
                }

                var objectContract = _contractResolver.ResolveContract(typeToReflect) as JsonObjectContract;
                if (objectContract == null)
                {
                    continue;
                }

                var contractTypeInfo = new ContractTypeInfo()
                {
                    Type = typeToReflect
                };

                var baseType = typeToReflect.GetTypeInfo().BaseType;
                var parentSchemaId = string.Empty;

                if (baseType != null &&
                    baseType != typeof(object) &&
                    baseType != typeof(Relation) &&
                    baseType != typeof(ReferenceObject))
                {
                    typesToReflect.Push(baseType);
                    contractTypeInfo.Dependencies.Add(baseType);
                    parentSchemaId = ResolveSchemaName(baseType);
                }

                contractTypeInfo.ParentTypeName = parentSchemaId;

                foreach (var property in objectContract.Properties.Where(p => p.DeclaringType == typeToReflect))
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
                        IsOverwritten = typeToReflect.GetTypeInfo().BaseType?.GetRuntimeProperty(property.UnderlyingName) != null
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
                                    propertyInfo.TypeName = propertyGenericArg.Name;
                                    contractTypeInfo.Dependencies.Add(propertyGenericArg);
                                    typesToReflect.Push(propertyGenericArg);
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
                            propertyInfo.TypeName = property.PropertyType.Name;
                            if (typeInfo.GetCustomAttribute<PictureparkReferenceAttribute>() != null)
                            {
                                propertyInfo.IsReference = true;
                            }

                            contractTypeInfo.Dependencies.Add(property.PropertyType);
                            typesToReflect.Push(property.PropertyType);
                        }
                    }

                    propertyInfo.PictureparkAttributes = property.AttributeProvider
                        .GetAttributes(true)
                        .Select(i => i as IPictureparkAttribute)
                        .Where(i => i != null)
                        .ToList();

                    contractTypeInfo.Properties.Add(propertyInfo);
                }

                // dependencies on the same type need to be removed
                contractTypeInfo.Dependencies.Remove(contractTypeInfo.Type);

                result.Add(contractTypeInfo);
            }

            return result;
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
            }
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

        private ICollection<SchemaDetail> GenerateSchemas(IReadOnlyList<ContractTypeInfo> schemaInfos, IEnumerable<SchemaDetail> existingSchemas)
        {
            var result = new List<SchemaDetail>();

            foreach (var schemaInfo in schemaInfos)
            {
                var type = schemaInfo.Type;
                var properties = schemaInfo.Properties;

                // ignore system schemas
                var isSystemSchema = type.GetTypeInfo().GetCustomAttributes(typeof(PictureparkSystemSchemaAttribute), true).Any();
                if (isSystemSchema)
                {
                    continue;
                }

                var schemaId = ResolveSchemaName(type);
                if (existingSchemas.Any(s => s.Id == schemaId))
                {
                    continue;
                }

                var typeAttributes = type.GetTypeInfo()
                    .GetCustomAttributes<PictureparkSchemaAttribute>(true)
                    .ToList();

                if (!typeAttributes.Any())
                    throw new Exception($"No PictureparkSchemaTypeAttribute set on class: {type.Name}");

                if (typeAttributes.Count > 1)
                    throw new Exception($"Multiple schema types not allowed for class: {type.Name}");

                var schemaType = typeAttributes.Single().Type;

                var schema = new SchemaDetail()
                {
                    Id = schemaId,
                    Fields = new List<FieldBase>(),
                    FieldsOverwrite = new List<FieldOverwriteBase>(),
                    ParentSchemaId = schemaInfo.ParentTypeName,
                    Names = new TranslatedStringDictionary { { _defaultLanguage, schemaId } },
                    Descriptions = new TranslatedStringDictionary(),
                    Types = new List<SchemaType> { schemaType },
                    DisplayPatterns = new List<DisplayPattern>()
                };

                if (schemaType == SchemaType.Struct ||
                    schemaType == SchemaType.Content)
                    schema.ViewForAll = true;

                ApplyDisplayPatternAttributes(schema, type);
                ApplyNameTranslationAttributes(schema, type);
                ApplyDescriptionTranslationAttributes(schema, type);

                foreach (var property in properties)
                {
                    if (property.IsOverwritten)
                    {
                        var fieldOverwrite = GetFieldOverwrite(property);
                        schema.FieldsOverwrite.Add(fieldOverwrite);
                    }
                    else
                    {
                        var field = GetField(property);
                        schema.Fields.Add(field);
                    }
                }

                result.Add(schema);
            }

            return result;
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

                throw new InvalidOperationException("Only Tagbox properties can be overriden.");
            }

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

            throw new InvalidOperationException("Only Tagbox properties can be overriden.");
        }

        private FieldBase GetField(ContractPropertyInfo property)
        {
            FieldBase field;

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
                throw new NotSupportedException("Enum types are not supported in Class to Schema conversion");
            }
            else if (property.IsSimpleType)
            {
                if (!Enum.TryParse(property.TypeName, out TypeCode typeCode))
                {
                    throw new Exception($"Parsing to TypeCode enumerated object failed for string value: {property.TypeName}.");
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

            return dateAttribute == null || dateAttribute.ContainsTimePortion
                ? (FieldBase)new FieldDateTime { Index = true, Format = format }
                : new FieldDate { Index = true, Format = format };
        }

        private class VisitedTypesStack
        {
            private readonly HashSet<Type> _seenTypes = new HashSet<Type>();
            private readonly Stack<Type> _backingStack = new Stack<Type>();

            public void Push(Type t)
            {
                if (!_seenTypes.Contains(t))
                {
                    _seenTypes.Add(t);
                    _backingStack.Push(t);
                }
            }

            public Type Pop()
            {
                return _backingStack.Pop();
            }

            public bool HasMore()
            {
                return _backingStack.Count > 0;
            }
        }
    }
}