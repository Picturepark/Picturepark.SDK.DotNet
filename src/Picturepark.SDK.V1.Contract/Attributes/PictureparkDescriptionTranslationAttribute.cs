using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class PictureparkDescriptionTranslationAttribute : Attribute, IPictureparkAttribute
    {
        public PictureparkDescriptionTranslationAttribute(string translation)
        {
            Translation = translation;
        }

        public PictureparkDescriptionTranslationAttribute(string languageAbbreviation, string translation)
        {
            LanguageAbbreviation = languageAbbreviation;
            Translation = translation;
        }

        public string LanguageAbbreviation { get; }

        public string Translation { get; }
    }
}
