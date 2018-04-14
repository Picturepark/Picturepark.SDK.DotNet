using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class PictureparkReferenceAttribute : Attribute, IPictureparkAttribute
    {
    }
}
