using Newtonsoft.Json.Converters;

namespace Picturepark.SDK.V1.Conversion
{
    public class DateFieldConverter : IsoDateTimeConverter
    {
        public DateFieldConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }
}