using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PictureparkTranslatedStringAttribute : Attribute, IPictureparkAttribute
    {
        public bool MultiLine { get; set; }

        public StringRenderingType RenderingType { get; set; }
    }
}
