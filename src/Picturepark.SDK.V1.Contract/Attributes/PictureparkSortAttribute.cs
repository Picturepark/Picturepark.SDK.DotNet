using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    /// <summary>
    /// Mark element as sortable
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class PictureparkSortAttribute : Attribute, IPictureparkAttribute
    {
    }
}
