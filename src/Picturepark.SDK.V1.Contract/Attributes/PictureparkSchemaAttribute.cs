using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PictureparkSchemaAttribute : Attribute, IPictureparkAttribute
    {
        public PictureparkSchemaAttribute(SchemaType type, string name = null)
        {
            Type = type;
            Name = name;
        }

        public SchemaType Type { get; }

        public string Name { get; }
    }
}
