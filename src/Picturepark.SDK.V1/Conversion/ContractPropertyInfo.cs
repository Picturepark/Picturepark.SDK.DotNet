using System;
using System.Collections.Generic;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Conversion
{
	public class ContractPropertyInfo
	{
		public string Name { get; set; }

		public string TypeName { get; set; }

		public bool IsSimpleType { get; set; }

		public TypeCode SimpleTypeCode { get; set; }

		public bool IsEnum { get; set; }

		public bool IsCustomType { get; set; }

		public bool IsArray { get; set; }

		public bool IsReference { get; set; }

		public string FullName { get; set; }

		public string AssemblyFullName { get; set; }

		public bool IsDictionary { get; set; }

		public List<ContractPropertyInfo> TypeProperties { get; set; } = new List<ContractPropertyInfo>();

		public List<IPictureparkAttribute> PictureparkAttributes { get; set; } = new List<IPictureparkAttribute>();
	}
}
