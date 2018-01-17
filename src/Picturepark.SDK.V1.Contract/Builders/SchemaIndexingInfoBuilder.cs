using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Serialization;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Contract.Builders
{
    public class SchemaIndexingInfoBuilder<T> : BuilderBase
    {
        private readonly IEnumerable<FieldIndexingInfo> _fields;

        public SchemaIndexingInfoBuilder(IEnumerable<FieldIndexingInfo> fields, IContractResolver contractResolver)
        {
            _fields = fields;
            ContractResolver = contractResolver;
        }

        public SchemaIndexingInfoBuilder()
            : this(new FieldIndexingInfo[0], new CamelCasePropertyNamesContractResolver())
        {
        }

        protected IContractResolver ContractResolver { get; }

        protected IEnumerable<FieldIndexingInfo> Fields => _fields;

        protected virtual IEnumerable<FieldIndexingInfo> CompleteFields => _fields;

        public virtual SchemaIndexingInfoPropertyBuilder<T> AddProperty(Expression<Func<T, object>> propertyExpression)
        {
            var contract = ContractResolver.ResolveContract(typeof(T)) as JsonObjectContract;
            if (contract != null)
            {
                var fields = CompleteFields.ToList();

                var propertyName = PropertyHelper.GetPropertyName(propertyExpression);
                var property = contract.Properties.Single(p => p.UnderlyingName == propertyName);

                var field = fields.SingleOrDefault(f => f.Id == property.PropertyName);
                if (field != null)
                {
                    return new SchemaIndexingInfoPropertyBuilder<T>(Clone(field), Clone(fields), ContractResolver);
                }
                else
                {
                    field = new FieldIndexingInfo { Id = property.PropertyName };
                    return new SchemaIndexingInfoPropertyBuilder<T>(field, Clone(fields), ContractResolver);
                }
            }

            throw new InvalidOperationException();
        }

        public SchemaIndexingInfo Build()
        {
            return new SchemaIndexingInfo
            {
                Fields = CompleteFields.ToList()
            };
        }

        public SchemaIndexingInfoPropertiesBuilder<T> AddProperties(int maxLevels)
        {
            return AddProperties(maxLevels, p => true);
        }

        public SchemaIndexingInfoPropertiesBuilder<T> AddPropertiesAndTagboxes(int maxLevels)
        {
            return AddProperties(maxLevels, p => p.AttributeProvider
                .GetAttributes(true)
                .OfType<PictureparkTagboxAttribute>()
                .Any());
        }

        public SchemaIndexingInfoPropertiesBuilder<T> AddProperties(int maxLevels, Predicate<JsonProperty> predicate)
        {
            var newFields = GetFieldIndexingInfos(typeof(T), maxLevels, predicate);
            return new SchemaIndexingInfoPropertiesBuilder<T>(newFields, Clone(CompleteFields), ContractResolver);
        }

        private List<FieldIndexingInfo> GetFieldIndexingInfos(Type type, int maxLevels, Predicate<JsonProperty> predicate)
        {
            var contract = ContractResolver.ResolveContract(type) as JsonObjectContract;
            if (contract != null)
            {
                return contract.Properties
                    .Where(p => predicate(p) && _fields.All(f => f.Id != p.PropertyName))
                    .Select(p => new FieldIndexingInfo
                    {
                        Id = p.PropertyName,
                        RelatedSchemaIndexing = maxLevels > 0 ?
                            new SchemaIndexingInfo
                            {
                                Fields = GetFieldIndexingInfos(p.PropertyType, maxLevels - 1, predicate)
                            }
                            : null
                    }).ToList();
            }

            return new List<FieldIndexingInfo>();
        }
    }
}
