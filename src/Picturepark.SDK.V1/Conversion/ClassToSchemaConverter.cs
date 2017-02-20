using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Picturepark.SDK.V1.Contract.Attributes.Analyzer;
using Picturepark.SDK.V1.Contract.Interfaces;

namespace Picturepark.SDK.V1.Conversion
{
	public class ClassToSchemaConverter
	{
		private List<MetadataSchemaDetailViewItem> _createdSchemasList;

		public ClassToSchemaConverter()
		{
			InitializeTemplates();
		}

		public List<MetadataSchemaDetailViewItem> Generate(Type type, List<MetadataSchemaDetailViewItem> schemaList, bool generateDependencySchema = true)
		{
			_createdSchemasList = schemaList;

			var contractPropertiesInfo = GetProperties(type);

			var schema = SchemaCreate(contractPropertiesInfo, type, string.Empty, 0, generateDependencySchema);

			var sortedList = new List<MetadataSchemaDetailViewItem>();

			foreach (var schemaItem in _createdSchemasList)
			{
				var dependencyList = _createdSchemasList.FindAll(s => s.Dependencies.Any(d => d.Id == schemaItem.Id));

				int? index = null;

				if (dependencyList.Any())
				{
					foreach (var dependency in dependencyList)
					{
						var dependencyIndex = sortedList.FindIndex(s => s.Id == dependency.Id);
						if (dependencyIndex == -1)
							continue;

						if (!index.HasValue || (index.Value > dependencyIndex))
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

		private void InitializeTemplates()
		{
		}

		private MetadataSchemaDetailViewItem SchemaCreate(List<ContractPropertyInfo> contractPropertyInfos, Type contractType, string parentSchemaId, int levelOfCall = 0, bool generateDependencySchema = true)
		{
			var schemaId = contractType.Name;

			var types = new List<MetadataSchemaType>();

			var typeAttributes = contractType.GetTypeInfo().GetCustomAttributes(typeof(PictureparkSchemaTypeAttribute), true).Select(i => i as PictureparkSchemaTypeAttribute).ToList();

			if (!typeAttributes.Any())
				throw new Exception("No PictureparkSchemaTypeAttribute set on class: " + contractType.Name);

			foreach (var typeAttribute in typeAttributes)
			{
				types.Add(typeAttribute.MetadataType);
			}

			var schemaItem = new MetadataSchemaDetailViewItem()
			{
				Id = schemaId,
				Fields = new List<FieldBase> { },
				ParentMetadataSchemaId = parentSchemaId,
				Names = new TranslatedStringDictionary { { "x-default", schemaId } },
				Types = types,
				DisplayPatterns = new List<DisplayPattern>()
			};

			var displayPatternAttributes = contractType.GetTypeInfo().GetCustomAttributes(typeof(PictureparkDisplayPatternAttribute), true).Select(i => i as PictureparkDisplayPatternAttribute).ToList();
			foreach (var displayPatternAttribute in displayPatternAttributes)
			{
				var displayPattern = new DisplayPattern
				{
					DisplayPatternType = displayPatternAttribute.Type,
					Name = displayPatternAttribute.Type.ToString(),
					TemplateEngine = displayPatternAttribute.TemplateEngine,
					Templates = new TranslatedStringDictionary { { "x-default", displayPatternAttribute.DisplayPattern } }
				};
				schemaItem.DisplayPatterns.Add(displayPattern);
			}

			var customTypes = contractPropertyInfos.FindAll(c => c.IsCustomType);

			if (customTypes.Any())
			{
				foreach (var customType in customTypes)
				{
					if (_createdSchemasList.Any(d => d.Id == customType.TypeName))
						continue;

					// Exclusion, if the customType is the contractType (it would create itself again with zero fields)
					if (customType.FullName == contractType.FullName)
						continue;

					var subLevelOfcall = levelOfCall + 1;
					Type type = Type.GetType($"{customType.FullName}, {customType.AssemblyFullName}");

					// Exclude system schemas from creation
					if (!type.GetTypeInfo().GetCustomAttributes(typeof(PictureparkSystemSchemaAttribute), true).Any())
					{
						var dependencySchema = SchemaCreate(customType.TypeProperties, type, string.Empty, subLevelOfcall, generateDependencySchema);

						// the schema can be alredy added as dependency
						if (schemaItem.Dependencies.Any(d => d.Id == customType.TypeName) == false)
							schemaItem.Dependencies.Add(dependencySchema);

						// the schema can be alredy created
						if (_createdSchemasList.Any(d => d.Id == customType.TypeName) == false && generateDependencySchema)
							_createdSchemasList.Add(dependencySchema);
					}
				}
			}

			foreach (var contractPropertyInfo in contractPropertyInfos)
			{
				var fieldName = contractPropertyInfo.Name;
				var fieldData = GetFieldData(contractPropertyInfo);

				fieldData.Id = fieldName;
				fieldData.FieldNamespace = $"{schemaId}_{fieldName}";
				if (!string.IsNullOrEmpty(parentSchemaId))
					fieldData.FieldNamespace = $"{parentSchemaId}_{fieldData.FieldNamespace}";

				// TODO: Use customer defined languages and add Attribute for Translations
				if (fieldData.Names == null)
					fieldData.Names = new TranslatedStringDictionary();

				fieldData.Names["en"] = fieldName;
				fieldData.Names["fr"] = fieldName;
				fieldData.Names["de"] = fieldName;

				var fieldAnalyzers = contractPropertyInfo.PictureparkAttributes
					.OfType<PictureparkAnalyzerAttribute>()
					.Select(a => a.CreateAnalyzer())
					.ToList();

				if (fieldAnalyzers.Any())
					fieldData.GetType().GetRuntimeProperty("Analyzers").SetValue(fieldData, fieldAnalyzers);

				schemaItem.Fields.Add(fieldData);
			}

			if (generateDependencySchema || (!generateDependencySchema && levelOfCall == 0))
			{
				// the schema can be alredy created
				if (_createdSchemasList.Find(s => s.Id == schemaItem.Id) == null)
					_createdSchemasList.Add(schemaItem);
			}

			// Create schemas for all subtypes
			var subtypes = contractType.GetTypeInfo().Assembly.GetTypes().Where(t => t.GetTypeInfo().IsSubclassOf(contractType));
			foreach (var subtype in subtypes)
			{
				SchemaCreate(GetProperties(subtype), subtype, schemaId);
			}

			return schemaItem;
		}

		private List<ContractPropertyInfo> GetProperties(Type objType)
		{
			var contactPropertiesInfo = new List<ContractPropertyInfo>();

			PropertyInfo[] properties = objType
				.GetRuntimeProperties()
				.Where(i => i.Name != "refId")
				.ToArray();

			foreach (PropertyInfo property in properties)
			{
				var contactPropertyInfo = new ContractPropertyInfo();

				if (IsSimpleType(property.PropertyType))
				{
					contactPropertyInfo = HandleSimpleTypes(property);
				}
				else
				{
					// either list or dictionary
					if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(property.PropertyType.GetTypeInfo()))
					{
						if (property.PropertyType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IDictionary)) ||
							(property.PropertyType.GetTypeInfo().GenericTypeArguments.Any() && property.PropertyType.GetTypeInfo().GenericTypeArguments.First().GetTypeInfo().ImplementedInterfaces.Contains(typeof(IDictionary)) == true))
						{
							contactPropertyInfo.IsArray = property.PropertyType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IList));
							contactPropertyInfo.Name = property.Name;
							contactPropertyInfo.IsDictionary = true;
							contactPropertyInfo.TypeName = property.PropertyType.Name;
						}
						else
						{
							var propertyGenericArg = property.PropertyType.GetTypeInfo().GenericTypeArguments.First();

							if (IsSimpleType(propertyGenericArg))
							{
								contactPropertyInfo = HandleSimpleTypes(property);
							}
							else
							{
								contactPropertyInfo.IsCustomType = true;
								contactPropertyInfo.Name = property.Name;
								contactPropertyInfo.TypeName = propertyGenericArg.Name;
								contactPropertyInfo.FullName = propertyGenericArg.FullName;
								contactPropertyInfo.AssemblyFullName = propertyGenericArg.GetTypeInfo().Assembly.FullName;

								// Check for prevention of an infinite loop
								if (propertyGenericArg.FullName != objType.FullName)
								{
									contactPropertyInfo.TypeProperties.AddRange(
										GetProperties(propertyGenericArg));
								}
							}

							contactPropertyInfo.IsArray = true;

							if (property.PropertyType.GenericTypeArguments.Any() && typeof(IReference).GetTypeInfo().IsAssignableFrom(property.PropertyType.GenericTypeArguments.First().GetTypeInfo()))
							{
								contactPropertyInfo.IsReference = true;
							}
						}
					}
					else
					{
						contactPropertyInfo.IsCustomType = true;
						contactPropertyInfo.Name = property.Name;
						contactPropertyInfo.TypeName = property.PropertyType.Name;
						contactPropertyInfo.FullName = property.PropertyType.FullName;
						contactPropertyInfo.AssemblyFullName = property.PropertyType.GetTypeInfo().Assembly.FullName;

						if (typeof(IReference).GetTypeInfo().IsAssignableFrom(property.PropertyType.GetTypeInfo()))
						{
							contactPropertyInfo.IsReference = true;
						}

						// Check for prevention of an infinite loop
						if (property.PropertyType.FullName != objType.FullName)
						{
							contactPropertyInfo.TypeProperties.AddRange(
								GetProperties(property.PropertyType));
						}
					}
				}

				var customAttributes = property.GetCustomAttributes(true);
				var searchAttribute = customAttributes.Where(i => i.GetType().GetTypeInfo().ImplementedInterfaces.Any(j => j == typeof(IPictureparkAttribute))).Select(i => i as IPictureparkAttribute).ToList();
				contactPropertyInfo.PictureparkAttributes = searchAttribute;
				contactPropertiesInfo.Add(contactPropertyInfo);
			}

			return contactPropertiesInfo;
		}

		private ContractPropertyInfo HandleSimpleTypes(PropertyInfo property)
		{
			var contactPropertyInfo = new ContractPropertyInfo()
			{
				Name = property.Name,
				IsSimpleType = true
			};

			// it's a case of: nullable / enum type property
			if (property.PropertyType.GetTypeInfo().GenericTypeArguments != null && property.PropertyType.GetTypeInfo().GenericTypeArguments.Length > 0)
			{
				var propertyGenericArg = property.PropertyType.GenericTypeArguments.First();
				var underlyingType = Nullable.GetUnderlyingType(propertyGenericArg);
				propertyGenericArg = underlyingType ?? propertyGenericArg;

				contactPropertyInfo.FullName = propertyGenericArg.FullName;
				contactPropertyInfo.AssemblyFullName = propertyGenericArg.GetTypeInfo().Assembly.FullName;

				if (propertyGenericArg.GetTypeInfo().IsEnum)
				{
					contactPropertyInfo.IsEnum = true;
					contactPropertyInfo.TypeName = propertyGenericArg.Name;
				}
				else
				{
					if (propertyGenericArg == typeof(DateTimeOffset))
					{
						contactPropertyInfo.TypeName = TypeCode.DateTime.ToString();
					}
					else
					{
						// TODO: Find better solution for this
						contactPropertyInfo.TypeName = typeof(Type).GetRuntimeMethod("GetTypeCode", new[] { typeof(Type) })
						.Invoke(null, new object[] { propertyGenericArg }).ToString();
					}
				}
			}
			else
			{
				if (property.GetType() == typeof(DateTimeOffset))
				{
					contactPropertyInfo.TypeName = TypeCode.DateTime.ToString();
				}
				else
				{
					contactPropertyInfo.TypeName = typeof(Type).GetRuntimeMethod("GetTypeCode", new[] { typeof(Type) })
						.Invoke(null, new object[] { property.PropertyType }).ToString();
				}

				contactPropertyInfo.FullName = property.PropertyType.FullName;
				contactPropertyInfo.AssemblyFullName = property.PropertyType.GetTypeInfo().Assembly.FullName;
			}

			return contactPropertyInfo;
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
				TypeCode typeCode;
				if (!Enum.TryParse<TypeCode>(contractPropertyInfo.TypeName, out typeCode))
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
							fieldData = new FieldIntegerArray
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
					var stringInfos = contractPropertyInfo.PictureparkAttributes.Where(i => i is PictureparkStringAttribute).Select(i => i as PictureparkStringAttribute).SingleOrDefault();

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
								MultiLine = stringInfos != null ? stringInfos.MultiLine : false
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
							fieldData = new FieldInteger
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
				var schemaItemInfos = contractPropertyInfo.PictureparkAttributes.Where(i => i is PictureparkSchemaItemAttribute).Select(i => i as PictureparkSchemaItemAttribute).SingleOrDefault();
				var relationInfos = contractPropertyInfo.PictureparkAttributes.Where(i => i is PictureparkAssetRelationAttribute).Select(i => i as PictureparkAssetRelationAttribute);
				var maxRecursionInfos = contractPropertyInfo.PictureparkAttributes.Where(i => i is PictureparkMaximumRecursionAttribute).Select(i => i as PictureparkMaximumRecursionAttribute).SingleOrDefault();
				List<RelationType> relationTypes = new List<RelationType>();
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
						fieldData = new FieldRelations
						{
							MaxRecursion = maxRecursionInfos != null ? maxRecursionInfos.MaxRecursion : 1,
							RelationTypes = relationTypes,
							MetadataSchemaId = contractPropertyInfo.TypeName,
							Index = true
						};
					}
					else if (contractPropertyInfo.IsReference)
					{
						fieldData = new FieldSchemaItems
						{
							Index = true,
							MaxRecursion = maxRecursionInfos != null ? maxRecursionInfos.MaxRecursion : 1,
							SimpleSearch = true,
							MetadataSchemaId = contractPropertyInfo.TypeName,
							Filter = schemaItemInfos?.Filter
						};
					}
					else
					{
						fieldData = new FieldObjects
						{
							Index = true,
							MaxRecursion = maxRecursionInfos != null ? maxRecursionInfos.MaxRecursion : 1,
							SimpleSearch = true,
							MetadataSchemaId = contractPropertyInfo.TypeName
						};
					}
				}
				else
				{
					if (relationInfos.Any())
					{
						fieldData = new FieldRelation
						{
							Index = true,
							SimpleSearch = true,
							RelationTypes = relationTypes,
							MaxRecursion = maxRecursionInfos != null ? maxRecursionInfos.MaxRecursion : 1,
							MetadataSchemaId = contractPropertyInfo.TypeName
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
						fieldData = new FieldSchemaItem
						{
							Index = true,
							SimpleSearch = true,
							MaxRecursion = maxRecursionInfos != null ? maxRecursionInfos.MaxRecursion : 1,
							MetadataSchemaId = contractPropertyInfo.TypeName,
							Filter = schemaItemInfos?.Filter
						};
					}
					else
					{
						fieldData = new FieldObject
						{
							Index = true,
							SimpleSearch = true,
							MaxRecursion = maxRecursionInfos != null ? maxRecursionInfos.MaxRecursion : 1,
							MetadataSchemaId = contractPropertyInfo.TypeName
						};
					}
				}
			}

			foreach (var attribute in contractPropertyInfo.PictureparkAttributes)
			{
				if (attribute is PictureparkSearchAttribute)
				{
					var searchAttribute = attribute as PictureparkSearchAttribute;
					fieldData.Index = searchAttribute.Index;
					fieldData.SimpleSearch = searchAttribute.SimpleSearch;
					fieldData.Boost = searchAttribute.Boost;
				}

				if (attribute is PictureparkRequiredAttribute)
				{
					fieldData.Required = true;
				}

				if (attribute is PictureparkMaximumLengthAttribute)
				{
					fieldData.GetType().GetRuntimeProperty("MaximumLength").SetValue(fieldData, ((PictureparkMaximumLengthAttribute)attribute).Length);
				}

				if (attribute is PictureparkPatternAttribute)
				{
					fieldData.GetType().GetRuntimeProperty("Pattern").SetValue(fieldData, ((PictureparkPatternAttribute)attribute).Pattern);
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
