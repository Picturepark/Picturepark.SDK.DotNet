﻿using System;
using System.Collections.Generic;

namespace Picturepark.SDK.V1.Conversion
{
    internal class ContractTypeInfo
    {
        public string ParentTypeName { get; set; }

        public Type Type { get; set; }

        public ICollection<ContractPropertyInfo> Properties { get; } = new List<ContractPropertyInfo>();
    }
}