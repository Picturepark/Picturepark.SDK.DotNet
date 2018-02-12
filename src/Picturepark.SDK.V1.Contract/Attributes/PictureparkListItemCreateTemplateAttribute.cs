using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PictureparkListItemCreateTemplateAttribute : Attribute, IPictureparkAttribute
    {
        public PictureparkListItemCreateTemplateAttribute(string listItemCreateTemplate)
        {
            ListItemCreateTemplate = listItemCreateTemplate;
        }

        public string ListItemCreateTemplate { get; set; }
    }
}