using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class PictureparkSearchAttribute : Attribute, IPictureparkAttribute
    {
        public bool Index { get; set; }

        public bool SimpleSearch { get; set; }

        public double Boost { get; set; }
    }
}
