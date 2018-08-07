namespace Picturepark.SDK.V1.Contract.Attributes
{
    public class PictureparkDateAttribute : PictureparkDateTypeAttribute
    {
        public PictureparkDateAttribute(string pattern = null) : base(pattern)
        {
        }
    }
}