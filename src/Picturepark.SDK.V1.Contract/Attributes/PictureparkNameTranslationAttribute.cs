using System;
using System.Collections.Generic;
using System.Text;

namespace Picturepark.SDK.V1.Contract.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
	public class PictureparkNameTranslationAttribute : Attribute, IPictureparkAttribute
	{
		public PictureparkNameTranslationAttribute(string languageAbbreviation, string translation)
		{
			LanguageAbbreviation = languageAbbreviation;
			Translation = translation;
		}

		public string LanguageAbbreviation { get; }

		public string Translation { get; }
	}
}
