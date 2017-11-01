using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Picturepark.SDK.V1.Contract.Attributes.Analyzer;
using Picturepark.SDK.V1.Contract.Interfaces;
using Newtonsoft.Json;

namespace Picturepark.SDK.V1.Conversion
{
	public class ClassToSchemaConverter
	{
		private readonly List<string> _ignoredProperties = new List<string> { "refId", "_relId", "_relationType", "_targetContext", "_targetId" };

		/// <summary>
		/// Convert a C# POCO to a picturepark schema definition
		/// </summary>
		/// <param name="type">Type of poco to convert</param>
		/// <param name="generateRelatedSchemas">Generates related schemas as well. E.g. referenced pocos in lists.</param>
		/// <returns>List of schemas</returns>
		public List<SchemaDetail> Generate(Type type, bool generateRelatedSchemas = true)
		{
			var schemaList = new List<SchemaDetail>();
			return Generate(type, schemaList, generateRelatedSchemas);
		}

		/// <summary>
		/// Convert a C# POCO to a picturepark schema definition
		/// </summary>
		/// <param name="type">Type of poco to convert</param>
		/// <param name="schemaList">Existing list of schemas. Pass if you need to convert several pocos and they reference the same dependant schemas (used to exclude existing schemas).</param>
		/// <param name="generateRelatedSchemas">Generates related schemas as well. E.g. referenced pocos in lists.</param>
		/// <returns>List of schemas</returns>
		public List<SchemaDetail> Generate(Type type, List<SchemaDetail> schemaList, bool generateRelatedSchemas = true)
		{
			var classProperties = GetClassProperties(type);

			SchemaCreate(classProperties, type, string.Empty, schemaList, 0, generateRelatedSchemas);

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

			return sortedList;
		}

		private SchemaDetail SchemaCreate(List<ContractPropertyInfo> classProperties, Type contractType, string parentSchemaId, List<SchemaDetail> schemaList, int levelOfCall = 0, bool generateDependencySchema = true)
		{
			var schemaId = contractType.Name;

			var typeAttributes = contractType.GetTypeInfo().GetCustomAttributes(typeof(PictureparkSchemaTypeAttribute), true).Select(i => i as PictureparkSchemaTypeAttribute).ToList();

			if (!typeAttributes.Any())
				throw new Exception("No PictureparkSchemaTypeAttribute set on class: " + contractType.Name);

			var types = typeAttributes.Select(typeAttribute => typeAttribute.SchemaType).ToList();

			var schemaItem = new SchemaDetail
			{
				Id = schemaId,
				Fields = new List<FieldBase>(),
				ParentSchemaId = parentSchemaId,
				Names = new TranslatedStringDictionary { { "x-default", schemaId } },
				Descriptions = new TranslatedStringDictionary(),
				Types = types,
				DisplayPatterns = new List<DisplayPattern>()
			};

			var displayPatternAttributes = contractType.GetTypeInfo().GetCustomAttributes(typeof(PictureparkDisplayPatternAttribute), true).Select(i => i as PictureparkDisplayPatternAttribute).ToList();
			foreach (var displayPatternAttribute in displayPatternAttributes)
			{
				var displayPattern = new DisplayPattern
				{
					DisplayPatternType = displayPatternAttribute.Type,
					Id = displayPatternAttribute.Type.ToString(),
					TemplateEngine = displayPatternAttribute.TemplateEngine,
					Templates = new TranslatedStringDictionary { { "x-default", displayPatternAttribute.DisplayPattern } }
				};
				schemaItem.DisplayPatterns.Add(displayPattern);
			}

			// Assign name translations
			var nameTranslationAttributes = contractType.GetTypeInfo().GetCustomAttributes(typeof(PictureparkNameTranslationAttribute), true).Select(i => i as PictureparkNameTranslationAttribute).ToList();
			foreach (var translationAttribute in nameTranslationAttributes)
			{
				schemaItem.Names[translationAttribute.LanguageAbbreviation] = translationAttribute.Translation;
			}

			// Assign description translations
			var descriptionTranslationAttributes = contractType.GetTypeInfo().GetCustomAttributes(typeof(PictureparkDescriptionTranslationAttribute), true).Select(i => i as PictureparkDescriptionTranslationAttribute).ToList();
			foreach (var translationAttribute in descriptionTranslationAttributes)
			{
				schemaItem.Descriptions[translationAttribute.LanguageAbbreviation] = translationAttribute.Translation;
			}

			var customTypes = classProperties.FindAll(c => c.IsCustomType);

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

					var subLevelOfcall = levelOfCall + 1;
					Type type = Type.GetType($"{customType.FullName}, {customType.AssemblyFullName}");

					// Exclude system schemas from creation
					if (!type.GetTypeInfo().GetCustomAttributes(typeof(PictureparkSystemSchemaAttribute), true).Any())
					{
						var dependencySchema = SchemaCreate(customType.TypeProperties, type, string.Empty, schemaList, subLevelOfcall, generateDependencySchema);

						// the schema can be alredy added as dependency
						if (schemaItem.Dependencies.Any(d => d.Id == referencedSchemaId) == false)
							schemaItem.Dependencies.Add(dependencySchema);

						// the schema can be alredy created
						if (schemaList.Any(d => d.Id == referencedSchemaId) == false && generateDependencySchema)
							schemaList.Add(dependencySchema);
					}
				}
			}

			foreach (var contractPropertyInfo in classProperties)
			{
				var fieldName = contractPropertyInfo.Name;
				var fieldData = GetFieldData(contractPropertyInfo);

				fieldData.Id = fieldName.ToLowerCamelCase();
				fieldData.FieldNamespace = $"{schemaId}_{fieldData.Id}";
				if (!string.IsNullOrEmpty(parentSchemaId))
					fieldData.FieldNamespace = $"{parentSchemaId}_{fieldData.FieldNamespace}";

				if (fieldData.Names == null)
				{
					fieldData.Names = new TranslatedStringDictionary
					{
						["x-default"] = fieldName
					};
				}

				var fieldAnalyzers = contractPropertyInfo.PictureparkAttributes
					.OfType<PictureparkAnalyzerAttribute>()
					.Select(a => a.CreateAnalyzer())
					.ToList();

				if (fieldAnalyzers.Any())
					fieldData.GetType().GetRuntimeProperty("Analyzers").SetValue(fieldData, fieldAnalyzers);

				schemaItem.Fields.Add(fieldData);
			}

			if (generateDependencySchema || levelOfCall == 0)
			{
				// the schema can be alredy created
				if (schemaList.Find(s => s.Id == schemaItem.Id) == null)
					schemaList.Add(schemaItem);
			}

			// Create schemas for all subtypes
			var subtypes = contractType.GetTypeInfo().Assembly.GetTypes().Where(t => t.GetTypeInfo().IsSubclassOf(contractType));
			foreach (var subtype in subtypes)
			{
				SchemaCreate(GetClassProperties(subtype), subtype, schemaId, schemaList);
			}

			return schemaItem;
		}

		private List<ContractPropertyInfo> GetClassProperties(Type objType)
		{
			var contactPropertiesInfo = new List<ContractPropertyInfo>();

			var properties = objType
				.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
				.Where(i => _ignoredProperties.Contains(i.Name) == false)
				.ToList();

			// Exclude properties with JsonIgnore attribute
			properties = properties.Where(property => property.GetCustomAttributes(false).OfType<JsonIgnoreAttribute>().Any() == false).ToList();

			foreach (PropertyInfo property in properties)
			{
				var typeInfo = property.PropertyType.GetTypeInfo();
				var name = property.Name;

				// Check if name is overridden by JsonProperty attribute
				var jsonProperty = property.GetCustomAttributes(false).OfType<JsonPropertyAttribute>().FirstOrDefault();
				if (jsonProperty != null)
					name = jsonProperty.PropertyName;

				// Skip ignored properties
				if (_ignoredProperties.Contains(name))
					continue;

				var propertyInfo = new ContractPropertyInfo()
				{
					Name = name
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
								if (propertyGenericArg.FullName != objType.FullName)
								{
									propertyInfo.TypeProperties.AddRange(
										GetClassProperties(propertyGenericArg));
								}
							}

							propertyInfo.IsArray = true;

							if (property.PropertyType.GenericTypeArguments.Any() && typeof(IReference).GetTypeInfo().IsAssignableFrom(property.PropertyType.GenericTypeArguments.First().GetTypeInfo()))
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

						if (typeof(IReference).GetTypeInfo().IsAssignableFrom(typeInfo))
						{
							propertyInfo.IsReference = true;
						}

						// Check for prevention of an infinite loop
						if (property.PropertyType.FullName != objType.FullName)
						{
							propertyInfo.TypeProperties.AddRange(
								GetClassProperties(property.PropertyType));
						}
					}
				}

				var customAttributes = property.GetCustomAttributes(true);
				var searchAttribute = customAttributes.Where(i => i.GetType().GetTypeInfo().ImplementedInterfaces.Any(j => j == typeof(IPictureparkAttribute))).Select(i => i as IPictureparkAttribute).ToList();
				propertyInfo.PictureparkAttributes = searchAttribute;
				contactPropertiesInfo.Add(propertyInfo);
			}

			return contactPropertiesInfo;
		}

		private void HandleSimpleTypes(PropertyInfo property, ContractPropertyInfo propertyInfo)
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
						// TODO: Find better solution for this
						propertyInfo.TypeName = typeof(Type).GetRuntimeMethod("GetTypeCode", new[] { typeof(Type) })
						.Invoke(null, new object[] { propertyGenericArg }).ToString();
					}
				}
			}
			else
			{
				if (property.GetType() == typeof(DateTimeOffset))
				{
					propertyInfo.TypeName = TypeCode.DateTime.ToString();
				}
				else
				{
					propertyInfo.TypeName = typeof(Type).GetRuntimeMethod("GetTypeCode", new[] { typeof(Type) })
						.Invoke(null, new object[] { property.PropertyType }).ToString();
				}

				propertyInfo.FullName = property.PropertyType.FullName;
				propertyInfo.AssemblyFullName = typeInfo.Assembly.FullName;
			}
		}

		private FieldBase GetFieldData(ContractPropertyInfo contractPropertyInfo)
		{
			FieldBase fieldData = null;

			if (contractPropertyInfo.IsDictionary)
			{
				if (contractPropertyInfo.TypeName == "TranslatedStringDictionary")
				{
					fieldData = new FieldTranslatedString
					{
						Required = false,
						Fixed = false,
						Index = true,
						SimpleSearch = true,
						MultiLine = false,
						Boost = 1,
						Analyzers = new List<AnalyzerBase>
						{
							new LanguageAnalyzer
							{
								SimpleSearch = true
							}
						}
					};
				}
				else if (contractPropertyInfo.IsArray)
				{
					fieldData = new FieldDictionaryArray();
				}
				else
				{
					fieldData = new FieldDictionary();
				}
			}
			else if (contractPropertyInfo.IsEnum)
			{
				Type enumType = Type.GetType($"{contractPropertyInfo.FullName}, {contractPropertyInfo.AssemblyFullName}");
			}
			else if (contractPropertyInfo.IsSimpleType)
			{
				if (!Enum.TryParse(contractPropertyInfo.TypeName, out TypeCode typeCode))
				{
					throw new Exception($"Parsing to TypeCode enumarated object failed for string value: {contractPropertyInfo.TypeName}.");
				}

				if (contractPropertyInfo.IsArray)
				{
					switch (typeCode)
					{
						case TypeCode.String:
							fieldData = new FieldStringArray
							{
								Index = true
							};
							break;
						case TypeCode.DateTime:
							fieldData = new FieldDateTimeArray
							{
								Index = true
							};
							break;
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
							fieldData = new FieldLongArray
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
					var stringInfos = contractPropertyInfo.PictureparkAttributes.OfType<PictureparkStringAttribute>().SingleOrDefault();

					switch (typeCode)
					{
						case TypeCode.String:
							fieldData = new FieldString
							{
								Index = true,
								SimpleSearch = true,
								Boost = 1,
								Analyzers = new List<AnalyzerBase>
								{
									new SimpleAnalyzer
									{
										SimpleSearch = true
									}
								},
								MultiLine = stringInfos?.MultiLine ?? false
							};
							break;
						case TypeCode.DateTime:
							fieldData = new FieldDateTime
							{
								Index = true
							};
							break;
						case TypeCode.Boolean:
							fieldData = new FieldBoolean
							{
								Index = true
							};
							break;
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
							fieldData = new FieldLong
							{
								Index = true
							};
							break;
						case TypeCode.Decimal:
						case TypeCode.Double:
						case TypeCode.Single:
							fieldData = new FieldDecimal
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
				var schemaItemInfos = contractPropertyInfo.PictureparkAttributes.OfType<PictureparkSchemaItemAttribute>().SingleOrDefault();
				var relationInfos = contractPropertyInfo.PictureparkAttributes.OfType<PictureparkContentRelationAttribute>().ToList();
				var maxRecursionInfos = contractPropertyInfo.PictureparkAttributes.OfType<PictureparkMaximumRecursionAttribute>().SingleOrDefault();

				var relationTypes = new List<RelationType>();
				if (relationInfos.Any())
				{
					relationTypes = relationInfos.Select(i => new RelationType
					{
						Id = i.Name,
						Filter = i.Filter,
						TargetContext = i.TargetContext,
						Names = new TranslatedStringDictionary { { "x-default", i.Name } }
					}).ToList();
				}

				if (contractPropertyInfo.IsArray)
				{
					if (relationInfos.Any())
					{
						fieldData = new FieldMultiRelation
						{
							RelationTypes = relationTypes,
							SchemaId = contractPropertyInfo.TypeName,
							Index = true
						};
					}
					else if (contractPropertyInfo.IsReference)
					{
						fieldData = new FieldMultiTagbox
						{
							Index = true,
							SimpleSearch = true,
							SchemaId = contractPropertyInfo.TypeName,
							Filter = schemaItemInfos?.Filter
						};
					}
					else
					{
						fieldData = new FieldMultiFieldset
						{
							Index = true,
							SimpleSearch = true,
							SchemaId = contractPropertyInfo.TypeName
						};
					}
				}
				else
				{
					if (relationInfos.Any())
					{
						fieldData = new FieldSingleRelation
						{
							Index = true,
							SimpleSearch = true,
							RelationTypes = relationTypes,
							SchemaId = contractPropertyInfo.TypeName
						};
					}
					else if (contractPropertyInfo.TypeName == "GeoPoint")
					{
						fieldData = new FieldGeoPoint
						{
							Index = true
						};
					}
					else if (contractPropertyInfo.IsReference)
					{
						fieldData = new FieldSingleTagbox
						{
							Index = true,
							SimpleSearch = true,
							SchemaId = contractPropertyInfo.TypeName,
							Filter = schemaItemInfos?.Filter
						};
					}
					else
					{
						fieldData = new FieldSingleFieldset
						{
							Index = true,
							SimpleSearch = true,
							SchemaId = contractPropertyInfo.TypeName
						};
					}
				}
			}

			if (fieldData == null)
				throw new Exception($"Could not find type for {contractPropertyInfo.Name}");

			foreach (var attribute in contractPropertyInfo.PictureparkAttributes)
			{
				if (attribute is PictureparkSearchAttribute searchAttribute)
				{
					fieldData.Index = searchAttribute.Index;
					fieldData.SimpleSearch = searchAttribute.SimpleSearch;
					if (fieldData.GetType().GetRuntimeProperty("Boost") != null)
					{
						fieldData.GetType().GetRuntimeProperty("Boost").SetValue(fieldData, searchAttribute.Boost);
					}
				}

				if (attribute is PictureparkRequiredAttribute)
				{
					fieldData.Required = true;
				}

				if (attribute is PictureparkMaximumLengthAttribute maxLengthAttribute)
				{
					fieldData.GetType().GetRuntimeProperty("MaximumLength").SetValue(fieldData, maxLengthAttribute.Length);
				}

				if (attribute is PictureparkPatternAttribute patternAttribute)
				{
					fieldData.GetType().GetRuntimeProperty("Pattern").SetValue(fieldData, patternAttribute.Pattern);
				}

				if (attribute is PictureparkNameTranslationAttribute)
				{
					var translationAttribute = attribute as PictureparkNameTranslationAttribute;
					if (fieldData.Names == null)
						fieldData.Names = new TranslatedStringDictionary();

					fieldData.Names[translationAttribute.LanguageAbbreviation] = translationAttribute.Translation;
				}
			}

			return fieldData;
		}

		private bool IsSimpleType(Type type)
		{
			return
				type.GetTypeInfo().IsValueType ||
				type.GetTypeInfo().IsPrimitive ||
				new Type[]
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
