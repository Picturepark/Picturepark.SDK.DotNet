using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Serialization;

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

        public SchemaIndexingInfoPropertiesBuilder<T> AddProperties()
        {
            var contract = ContractResolver.ResolveContract(typeof(T)) as JsonObjectContract;
            if (contract != null)
            {
                var newFields = contract.Properties
                    .Where(p => _fields.All(f => f.Id != p.PropertyName))
                    .Select(p => new FieldIndexingInfo
                    {
                        Id = p.PropertyName
                    });

                return new SchemaIndexingInfoPropertiesBuilder<T>(newFields, Clone(CompleteFields), ContractResolver);
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
    }
}
