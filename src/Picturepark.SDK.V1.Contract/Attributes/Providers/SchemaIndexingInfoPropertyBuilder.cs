using System.Collections.Generic;
using System.Linq;

namespace Picturepark.SDK.V1.Contract.Attributes.Providers
{
    public class SchemaIndexingInfoPropertyBuilder<T>
        : SchemaIndexingInfoBuilder<T>
    {
        private FieldIndexingInfo _field;

        public SchemaIndexingInfoPropertyBuilder(
            FieldIndexingInfo field, IEnumerable<FieldIndexingInfo> fields)
            : base(fields)
        {
            _field = field;
        }

        protected override IEnumerable<FieldIndexingInfo> CompleteFields
        {
            get
            {
                var fields = base.CompleteFields;

                var oldField = fields.SingleOrDefault(f => f.Id == _field.Id);
                if (oldField != null)
                {
                    return Clone(Replace(fields, oldField, _field));
                }
                else
                {
                    return Clone(fields).Concat(new FieldIndexingInfo[] { _field }).ToList();
                }
            }
        }

        public SchemaIndexingInfoPropertyBuilder<T> WithBoost(double boost)
        {
            var field = Clone(_field);
            field.Boost = boost;

            return new SchemaIndexingInfoPropertyBuilder<T>(field, Clone(Fields));
        }

        public SchemaIndexingInfoPropertyBuilder<T> WithIndex()
        {
            var field = Clone(_field);
            field.Index = true;

            return new SchemaIndexingInfoPropertyBuilder<T>(field, Clone(Fields));
        }
    }
}
