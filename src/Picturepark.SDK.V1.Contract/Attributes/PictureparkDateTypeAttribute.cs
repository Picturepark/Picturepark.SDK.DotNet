using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public abstract class PictureparkDateTypeAttribute : Attribute, IPictureparkAttribute
    {
        protected PictureparkDateTypeAttribute(string pattern = null)
        {
            Pattern = pattern;
        }

        public string Pattern { get; set; }

        public bool ContainsTimePortion => this is PictureparkDateTimeAttribute;
    }
}