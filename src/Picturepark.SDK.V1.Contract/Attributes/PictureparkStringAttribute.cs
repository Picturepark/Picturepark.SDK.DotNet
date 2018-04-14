using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    public class PictureparkStringAttribute : Attribute, IPictureparkAttribute
    {
        public bool MultiLine { get; set; }
    }
}
