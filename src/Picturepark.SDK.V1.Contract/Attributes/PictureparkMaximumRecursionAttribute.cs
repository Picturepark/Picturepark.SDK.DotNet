using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
	public class PictureparkMaximumRecursionAttribute : Attribute, IPictureparkAttribute
	{
		public int MaxRecursion { get; set; }
	}
}
