using System.Collections.Generic;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Conversion
{
    internal class ContractPropertyInfo
    {
        public string Name { get; set; }

        public bool IsOverwritten { get; set; }

        public string TypeName { get; set; }

        public bool IsSimpleType { get; set; }

        public bool IsArray { get; set; }

        public bool IsReference { get; set; }

        public bool IsDictionary { get; set; }

        public bool IsEnum { get; set; }

        public bool IsTrigger { get; set; }

        public IReadOnlyList<IPictureparkAttribute> PictureparkAttributes { get; set; } = new List<IPictureparkAttribute>();
    }
}