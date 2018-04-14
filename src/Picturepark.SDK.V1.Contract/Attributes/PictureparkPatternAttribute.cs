using System;
using System.ComponentModel.DataAnnotations;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PictureparkPatternAttribute : RegularExpressionAttribute, IPictureparkAttribute
    {
        public PictureparkPatternAttribute(string pattern) : base(pattern)
        {
        }
    }
}
