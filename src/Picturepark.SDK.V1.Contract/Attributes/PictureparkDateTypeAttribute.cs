using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public abstract class PictureparkDateTypeAttribute : Attribute, IPictureparkAttribute
    {
        protected PictureparkDateTypeAttribute(string format = null)
        {
            Format = format;
        }

        public string Format { get; set; }

        public bool ContainsTimePortion => this is PictureparkDateTimeAttribute;
    }
}