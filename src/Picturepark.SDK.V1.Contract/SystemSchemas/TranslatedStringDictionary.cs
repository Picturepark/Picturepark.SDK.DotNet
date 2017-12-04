using Picturepark.SDK.V1.Contract.Attributes;
using System.Collections.Generic;
using System.Globalization;

namespace Picturepark.SDK.V1.Contract
{
	[PictureparkSystemSchema]
	public partial class TranslatedStringDictionary
	{
		public TranslatedStringDictionary()
		{
		}

		public TranslatedStringDictionary(int capacity)
			: base(capacity)
		{
		}

		public TranslatedStringDictionary(IDictionary<string, string> dictionary)
		{
			if (dictionary == null)
			{
				return;
			}

			foreach (var item in dictionary)
			{
				Add(item.Key, item.Value);
			}
		}

		/// <summary>Get a translation; fallback to x-default if specified language does not exist.</summary>
		/// <param name="twoLetterIsoLanguageName">The language to retrieve; fallback to CultureInfo.CurrentCulture.TwoLetterISOLanguageName if not specified.</param>
		/// <returns>The translated string.</returns>
		public string GetTranslation(string twoLetterIsoLanguageName = null)
		{
			var languageName = twoLetterIsoLanguageName ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
			return ContainsKey(languageName) ? this[languageName] : this["x-default"];
		}
	}
}
