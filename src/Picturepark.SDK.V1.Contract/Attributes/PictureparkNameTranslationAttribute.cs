using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    /// <summary>
    /// Adds name translations to a field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class PictureparkNameTranslationAttribute : Attribute, IPictureparkAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkNameTranslationAttribute"/> class.
        /// Adds the translations for the default language
        /// </summary>
        /// <param name="translation"></param>
        public PictureparkNameTranslationAttribute(string translation)
        {
            Translation = translation;
        }

        public PictureparkNameTranslationAttribute(string languageAbbreviation, string translation)
        {
            LanguageAbbreviation = languageAbbreviation;
            Translation = translation;
        }

        public string LanguageAbbreviation { get; }

        public string Translation { get; }
    }
}
