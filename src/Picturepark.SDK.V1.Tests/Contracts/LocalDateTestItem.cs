using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using System;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkSchema(SchemaType.List)]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.localDateTestItem.dateTimeField  | localDate :\"%d.%m.%Y %H:%M:%S\"}}")]
    public class LocalDateTestItem
    {
        public DateTime DateTimeField { get; set; }
    }
}
