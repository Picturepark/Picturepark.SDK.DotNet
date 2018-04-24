using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Builders
{
    public class SchemaIndexingInfoBuilder<T> : BuilderBase
    {
        private readonly SchemaIndexingInfo _schemaIndexingInfo;
        private readonly IContractResolver _contractResolver;

        public SchemaIndexingInfoBuilder()
            : this(new CamelCasePropertyNamesContractResolver(), new SchemaIndexingInfo())
        {
        }

        public SchemaIndexingInfoBuilder(IContractResolver contractResolver)
            : this(contractResolver, new SchemaIndexingInfo())
        {
        }

        public SchemaIndexingInfoBuilder(IContractResolver contractResolver, SchemaIndexingInfo schemaIndexingInfo)
        {
            _schemaIndexingInfo = schemaIndexingInfo;
            _contractResolver = contractResolver;
        }

        public SchemaIndexingInfo Build()
        {
            return _schemaIndexingInfo;
        }

        public SchemaIndexingInfoBuilder<T> AddIndex(Expression<Func<T, object>> expression)
        {
            var info = CreateFromExpression(expression, f => f.Index = true);
            return new SchemaIndexingInfoBuilder<T>(_contractResolver, info.Item1);
        }

        public SchemaIndexingInfoBuilder<T> AddIndexWithSimpleSearch(Expression<Func<T, object>> expression, double boost = 1)
        {
            var info = CreateFromExpression(expression, f =>
            {
                f.Index = true;
                f.SimpleSearch = true;
                f.Boost = boost;
            });

            return new SchemaIndexingInfoBuilder<T>(_contractResolver, info.Item1);
        }

        public SchemaIndexingInfoBuilder<T> AddIndexes(int levels, Func<Type, JsonProperty, bool> propertySelector = null)
        {
            return new SchemaIndexingInfoBuilder<T>(_contractResolver, new SchemaIndexingInfo
            {
                Fields = GenerateFieldIndexingInfos(typeof(T), levels, propertySelector)
            });
        }

        public SchemaIndexingInfoBuilder<T> AddDefaultIndexes(int levels, Func<Type, JsonProperty, bool> propertySelector = null)
        {
            return AddIndexes(levels, (type, property) =>
            {
                var searchAttribute = property.AttributeProvider
                    .GetAttributes(true)
                    .OfType<PictureparkSearchAttribute>()
                    .SingleOrDefault();

                if (searchAttribute != null && searchAttribute.Index)
                {
                    return propertySelector?.Invoke(type, property) != false;
                }

                return false;
            });
        }

        public SchemaIndexingInfoBuilder<T> AddIndexes(Expression<Func<T, object>> expression, int levels, Func<Type, JsonProperty, bool> propertySelector = null)
        {
            // TODO: Should this method be public (dangerous, may create many indexes!)
            var info = CreateFromExpression(expression, f => f.Index = true);
            var path = GetExpressionPath(expression);
            var type = path.Last().Item2;

            var fields = GenerateFieldIndexingInfos(type, levels, propertySelector);
            if (fields != null && fields.Any())
            {
                info.Item2.RelatedSchemaIndexing = new SchemaIndexingInfo
                {
                    Fields = GenerateFieldIndexingInfos(type, levels, propertySelector)
                };
            }

            return new SchemaIndexingInfoBuilder<T>(_contractResolver, info.Item1);
        }

        public SchemaIndexingInfoBuilder<T> AddDefaultIndexes(
            Expression<Func<T, object>> expression, int levels, Func<Type, JsonProperty, bool> propertySelector = null)
        {
            return AddIndexes(expression, levels, (type, property) =>
            {
                var searchAttribute = property.AttributeProvider
                    .GetAttributes(true)
                    .OfType<PictureparkSearchAttribute>()
                    .SingleOrDefault();

                if (searchAttribute != null && searchAttribute.Index)
                {
                    return propertySelector?.Invoke(type, property) != false;
                }

                return false;
            });
        }

        private ICollection<FieldIndexingInfo> GenerateFieldIndexingInfos(Type type, int levels, Func<Type, JsonProperty, bool> propertySelector)
        {
            var fields = new Collection<FieldIndexingInfo>();

            foreach (var property in GetTypePropertiesWithInheritance(type))
            {
                if (propertySelector?.Invoke(type, property) != false)
                {
                    var searchAttribute = property.AttributeProvider
                        .GetAttributes(true)
                        .OfType<PictureparkSearchAttribute>()
                        .SingleOrDefault();

                    var field = new FieldIndexingInfo
                    {
                        Id = property.PropertyName,
                        SimpleSearch = searchAttribute?.SimpleSearch ?? false,
                        Index = searchAttribute?.Index ?? false,
                        Boost = searchAttribute?.Boost ?? 0.0,
                    };
                    fields.Add(field);

                    ApplyPictureparkSchemaIndexingAttribute(property, field);

                    if (levels > 0)
                    {
                        var propertyType = property.PropertyType.GenericTypeArguments.Any()
                            ? property.PropertyType.GenericTypeArguments.First()
                            : property.PropertyType;

                        var subFields = GenerateFieldIndexingInfos(propertyType, levels - 1, propertySelector);
                        if (subFields != null && subFields.Any())
                        {
                            field.RelatedSchemaIndexing = new SchemaIndexingInfo
                            {
                                Fields = subFields
                            };
                        }
                    }
                }
            }

            return fields;
        }

        private IEnumerable<JsonProperty> GetTypePropertiesWithInheritance(Type type)
        {
            var properties = GetTypeProperties(type, new JsonProperty[0]).ToList();

            foreach (var knownTypeAttribute in type.GetTypeInfo().GetCustomAttributes<KnownTypeAttribute>())
            {
                if (knownTypeAttribute.Type != null)
                {
                    properties.AddRange(GetTypeProperties(knownTypeAttribute.Type, properties));
                }
                else if (knownTypeAttribute.MethodName != null)
                {
                    var method = type.GetRuntimeMethod(knownTypeAttribute.MethodName, new Type[0]);
                    if (method != null)
                    {
                        var knownTypes = (IEnumerable<Type>)method.Invoke(null, new object[0]);
                        foreach (var knownType in knownTypes)
                        {
                            properties.AddRange(GetTypeProperties(knownType, properties));
                        }
                    }
                }
            }

            return properties;
        }

        private IEnumerable<JsonProperty> GetTypeProperties(Type type, IEnumerable<JsonProperty> excludedProperties)
        {
            if (_contractResolver.ResolveContract(type) is JsonObjectContract contract)
            {
                return contract.Properties
                    .Where(p => excludedProperties.All(ep => ep.PropertyName != p.PropertyName))
                    .ToList();
            }

            return new JsonProperty[] { };
        }

        private void ApplyPictureparkSchemaIndexingAttribute(JsonProperty property, FieldIndexingInfo field)
        {
            // TODO: Is ApplyPictureparkSchemaIndexingAttribute needed?
            var attribute = (PictureparkSchemaIndexingAttribute)property.AttributeProvider
                .GetAttributes(typeof(PictureparkSchemaIndexingAttribute), true)
                .SingleOrDefault();

            if (attribute != null)
            {
                var relatedSchemaIndexing = Clone(attribute.SchemaIndexingInfo);
                if (relatedSchemaIndexing.Fields != null && relatedSchemaIndexing.Fields.Any())
                {
                    field.RelatedSchemaIndexing = relatedSchemaIndexing;
                }
            }
        }

        private Tuple<SchemaIndexingInfo, FieldIndexingInfo> CreateFromExpression(Expression<Func<T, object>> expression, Action<FieldIndexingInfo> fieldTransformator)
        {
            var rootInfo = Clone(_schemaIndexingInfo);
            var path = GetExpressionPath(expression);

            FieldIndexingInfo field = null;
            foreach (var segment in path)
            {
                var info = field == null ? rootInfo : field.RelatedSchemaIndexing = new SchemaIndexingInfo();
                if (info.Fields == null)
                {
                    info.Fields = new Collection<FieldIndexingInfo>();
                }

                field = info.Fields.SingleOrDefault(f => f.Id == segment.Item1) ?? new FieldIndexingInfo
                {
                    Id = segment.Item1
                };

                if (!info.Fields.Contains(field))
                {
                    info.Fields.Add(field);
                }

                fieldTransformator(field);
            }

            return new Tuple<SchemaIndexingInfo, FieldIndexingInfo>(rootInfo, field);
        }

        private Tuple<string, Type>[] GetExpressionPath(Expression expression)
        {
            if (expression is LambdaExpression lambdaExpression)
            {
                return GetExpressionPath(lambdaExpression.Body);
            }
            else if (expression is MemberExpression memberExpression)
            {
                if (memberExpression.Expression != null)
                {
                    return GetExpressionPath(memberExpression.Expression)
                        .Concat(new[]
                        {
                            new Tuple<string, Type>(
                                GetJsonPropertyName(memberExpression.Member.DeclaringType, memberExpression.Member.Name),
                                memberExpression.Type)
                        })
                        .ToArray();
                }
            }
            else if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name == "Select")
                {
                    var pathExpression = methodCallExpression.Arguments.First();
                    var selectExpression = methodCallExpression.Arguments.Last();

                    return GetExpressionPath(pathExpression)
                        .Concat(GetExpressionPath(selectExpression))
                        .ToArray();
                }
                else if (methodCallExpression.Method.Name == "OfType")
                {
                    var pathExpression = methodCallExpression.Arguments.First();
                    return GetExpressionPath(pathExpression);
                }
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                if (expression.NodeType == ExpressionType.Convert)
                {
                    return GetExpressionPath(unaryExpression.Operand);
                }
            }

            return new Tuple<string, Type>[0];
        }

        private string GetJsonPropertyName(Type type, string property)
        {
            if (_contractResolver.ResolveContract(type) is JsonObjectContract contract)
            {
                return contract.Properties.Single(p => p.UnderlyingName == property).PropertyName;
            }

            throw new InvalidOperationException("The type '" + type.FullName + "' has no JsonObjectContract.");
        }
    }
}
