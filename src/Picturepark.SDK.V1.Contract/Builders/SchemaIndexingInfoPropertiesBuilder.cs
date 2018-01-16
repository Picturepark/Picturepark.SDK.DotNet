using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Serialization;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Contract.Builders
{
    public class SchemaIndexingInfoPropertiesBuilder<T>
        : SchemaIndexingInfoBuilder<T>
    {
        private readonly IEnumerable<FieldIndexingInfo> _newFields;

        public SchemaIndexingInfoPropertiesBuilder(
           IEnumerable<FieldIndexingInfo> newFields,
           IEnumerable<FieldIndexingInfo> fields,
           IContractResolver contractResolver)
           : base(fields, contractResolver)
        {
            _newFields = newFields;
        }

        protected override IEnumerable<FieldIndexingInfo> CompleteFields => Clone(Fields).Concat(_newFields).ToList();

        public SchemaIndexingInfoPropertiesBuilder<T> WithBoost(double boost)
        {
            var fields = Clone(_newFields);
            foreach (var field in fields)
            {
                field.Boost = boost;
            }

            return new SchemaIndexingInfoPropertiesBuilder<T>(fields, Clone(Fields), ContractResolver);
        }

        public SchemaIndexingInfoPropertiesBuilder<T> WithIndex()
        {
            var fields = Clone(_newFields);
            foreach (var field in fields)
            {
                field.Index = true;
            }

            return new SchemaIndexingInfoPropertiesBuilder<T>(fields, Clone(Fields), ContractResolver);
        }

        public SchemaIndexingInfoPropertiesBuilder<T> WithAttributeDefaults()
        {
            var contract = ContractResolver.ResolveContract(typeof(T)) as JsonObjectContract;
            if (contract != null)
            {
                var fields = new List<FieldIndexingInfo>();
                foreach (var field in _newFields.Select(f => Clone(f)))
                {
                    var property = contract.Properties.SingleOrDefault(p => p.PropertyName == field.Id);
                    if (property != null)
                    {
                        var attribute = property.AttributeProvider
                            .GetAttributes(true)
                            .OfType<PictureparkSearchAttribute>()
                            .SingleOrDefault();

                        if (attribute != null)
                        {
                            field.Index = attribute.Index;
                            field.SimpleSearch = attribute.SimpleSearch;
                            field.Boost = attribute.Boost;
                        }
                    }

                    fields.Add(field);
                }

                return new SchemaIndexingInfoPropertiesBuilder<T>(fields, Clone(Fields), ContractResolver);
            }

            throw new InvalidOperationException();
        }

        public override SchemaIndexingInfoPropertyBuilder<T> AddProperty(Expression<Func<T, object>> property)
        {
            var builder = new SchemaIndexingInfoBuilder<T>(CompleteFields, ContractResolver);
            return builder.AddProperty(property);
        }
    }
}
