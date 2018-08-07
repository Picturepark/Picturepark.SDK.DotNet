namespace Picturepark.SDK.V1.Contract.Attributes
{
    public class PictureparkDateTimeAttribute : PictureparkDateTypeAttribute
    {
        public PictureparkDateTimeAttribute(string pattern = null) : base(pattern)
        {
        }
    }
}