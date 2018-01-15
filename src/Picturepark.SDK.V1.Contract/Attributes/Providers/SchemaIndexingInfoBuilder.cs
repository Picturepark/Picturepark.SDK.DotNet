using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Picturepark.SDK.V1.Contract.Attributes.Providers
{
    public class SchemaIndexingInfoBuilder<T> : BuilderBase
    {
        private IEnumerable<FieldIndexingInfo> _fields;

        public SchemaIndexingInfoBuilder(IEnumerable<FieldIndexingInfo> fields)
        {
            _fields = fields;
        }

        public SchemaIndexingInfoBuilder()
            : this(new FieldIndexingInfo[0])
        {
        }

        protected IEnumerable<FieldIndexingInfo> Fields => _fields;

        protected virtual IEnumerable<FieldIndexingInfo> CompleteFields => _fields;

        public virtual SchemaIndexingInfoPropertyBuilder<T> AddProperty(Expression<Func<T, object>> property)
        {
            var fields = CompleteFields;
            var id = PropertyHelper.GetLowerCamelCasePropertyPath(property);

            var field = fields.SingleOrDefault(f => f.Id == id);
            if (field != null)
            {
                return new SchemaIndexingInfoPropertyBuilder<T>(Clone(field), Clone(fields));
            }
            else
            {
                field = new FieldIndexingInfo { Id = id };
                return new SchemaIndexingInfoPropertyBuilder<T>(field, Clone(fields));
            }
        }

        public SchemaIndexingInfoPropertiesBuilder<T> AddProperties()
        {
            var newFields = typeof(T)
                .GetTypeInfo()
                .DeclaredProperties
                .Where(p => _fields.All(f => f.Id != PropertyHelper.ToLowerCamelCase(p.Name)))
                .Select(p => new FieldIndexingInfo
                {
                    Id = PropertyHelper.ToLowerCamelCase(p.Name)
                });

            return new SchemaIndexingInfoPropertiesBuilder<T>(newFields, Clone(CompleteFields));
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
