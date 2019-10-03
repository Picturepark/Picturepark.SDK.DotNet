using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PictureparkSchemaAttribute : Attribute, IPictureparkAttribute
    {
        public PictureparkSchemaAttribute(SchemaType type, string id = null)
        {
            Type = type;
            Id = id;
        }

        public SchemaType Type { get; }

        public string Id { get; }
    }
}
