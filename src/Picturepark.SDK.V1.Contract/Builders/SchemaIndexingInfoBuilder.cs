using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Serialization;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Contract.Builders
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

        public SchemaIndexingInfoBuilder<T> AddIndexes(Expression<Func<T, object>> expression, int levels, Func<Type, JsonProperty, bool> propertySelector = null)
        {
            // TODO: Should this method be public (dangerous, may create many indexes!)
            var info = CreateFromExpression(expression, f => f.Index = true);
            var path = GetExpressionPath(expression);
            var type = path.Last().Item2;

            info.Item2.Fields = GenerateFieldIndexingInfos(type, levels, propertySelector);

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
            if (_contractResolver.ResolveContract(type) is JsonObjectContract contract)
            {
                foreach (var property in contract.Properties)
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

                            field.RelatedSchemaIndexing = new SchemaIndexingInfo
                            {
                                Fields = GenerateFieldIndexingInfos(propertyType, levels - 1, propertySelector)
                            };
                        }
                    }
                }
            }

            return fields;
        }

        private void ApplyPictureparkSchemaIndexingAttribute(JsonProperty property, FieldIndexingInfo field)
        {
            // TODO: Is ApplyPictureparkSchemaIndexingAttribute needed?
            var attribute = (PictureparkSchemaIndexingAttribute)property.AttributeProvider
                .GetAttributes(typeof(PictureparkSchemaIndexingAttribute), true)
                .SingleOrDefault();

            if (attribute != null)
            {
                field.RelatedSchemaIndexing = Clone(attribute.SchemaIndexingInfo);
            }
        }

        private Tuple<SchemaIndexingInfo, SchemaIndexingInfo> CreateFromExpression(Expression<Func<T, object>> expression, Action<FieldIndexingInfo> fieldTransformator)
        {
            var info = Clone(_schemaIndexingInfo);
            var path = GetExpressionPath(expression);

            var currentInfo = info;
            foreach (var p in path)
            {
                if (currentInfo.Fields == null)
                {
                    currentInfo.Fields = new Collection<FieldIndexingInfo>();
                }

                var field = currentInfo.Fields.SingleOrDefault(f => f.Id == p.Item1) ?? new FieldIndexingInfo
                {
                    Id = p.Item1,
                    RelatedSchemaIndexing = new SchemaIndexingInfo()
                };

                if (!currentInfo.Fields.Contains(field))
                {
                    currentInfo.Fields.Add(field);
                }

                fieldTransformator(field);
                currentInfo = field.RelatedSchemaIndexing;
            }

            return new Tuple<SchemaIndexingInfo, SchemaIndexingInfo>(info, currentInfo);
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
