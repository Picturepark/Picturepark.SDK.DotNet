using System;
using System.ComponentModel.DataAnnotations;

namespace Picturepark.SDK.V1.Contract.Attributes
{
	[AttributeUsage(AttributeTargets.All)]
	public class PictureparkMaximumLengthAttribute : MaxLengthAttribute, IPictureparkAttribute
	{
	}
}
