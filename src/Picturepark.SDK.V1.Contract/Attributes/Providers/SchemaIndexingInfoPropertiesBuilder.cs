using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Picturepark.SDK.V1.Contract.Attributes.Providers
{
    public class SchemaIndexingInfoPropertiesBuilder<T>
        : SchemaIndexingInfoBuilder<T>
    {
        private IEnumerable<FieldIndexingInfo> _newFields;

        public SchemaIndexingInfoPropertiesBuilder(
           IEnumerable<FieldIndexingInfo> newFields, IEnumerable<FieldIndexingInfo> fields)
           : base(fields)
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

            return new SchemaIndexingInfoPropertiesBuilder<T>(fields, Clone(Fields));
        }

        public SchemaIndexingInfoPropertiesBuilder<T> WithIndex()
        {
            var fields = Clone(_newFields);
            foreach (var field in fields)
            {
                field.Index = true;
            }

            return new SchemaIndexingInfoPropertiesBuilder<T>(fields, Clone(Fields));
        }

        public SchemaIndexingInfoPropertiesBuilder<T> WithAttributeDefaults()
        {
            var properties = typeof(T).GetTypeInfo().DeclaredProperties;

            var fields = new List<FieldIndexingInfo>();
            foreach (var field in _newFields.Select(f => Clone(f)))
            {
                var property = properties.SingleOrDefault(p => p.Name.Equals(field.Id, StringComparison.OrdinalIgnoreCase));
                if (property != null)
                {
                    var attribute = property.GetCustomAttribute<PictureparkSearchAttribute>();
                    if (attribute != null)
                    {
                        field.Index = attribute.Index;
                        field.SimpleSearch = attribute.SimpleSearch;
                        field.Boost = attribute.Boost;
                    }
                }

                fields.Add(field);
            }

            return new SchemaIndexingInfoPropertiesBuilder<T>(fields, Clone(Fields));
        }

        public override SchemaIndexingInfoPropertyBuilder<T> AddProperty(Expression<Func<T, object>> property)
        {
            var builder = new SchemaIndexingInfoBuilder<T>(CompleteFields);
            return builder.AddProperty(property);
        }
    }
}
