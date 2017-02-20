using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class PictureparkDisplayPatternAttribute : Attribute, IPictureparkAttribute
	{
		public PictureparkDisplayPatternAttribute(DisplayPatternType type, TemplateEngine templateEngine, string displayPattern)
		{
			Type = type;
			DisplayPattern = displayPattern;
			TemplateEngine = templateEngine;
		}

		public DisplayPatternType Type { get; }

		public TemplateEngine TemplateEngine { get; }

		public string DisplayPattern { get; }
	}
}
