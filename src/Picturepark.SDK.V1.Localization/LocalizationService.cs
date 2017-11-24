using DotLiquid;
using NGettext;
using Picturepark.SDK.V1.Contract;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Picturepark.SDK.V1.Localization
{
	public class LocalizationService
	{
		private const string CatalogName = "picturepark";
		private const string FallbackLanguage = "en";

		private static readonly ConcurrentDictionary<string, ICatalog> s_CachedCatalogs = new ConcurrentDictionary<string, ICatalog>();

		public static string ResolveErrorCode(PictureparkException exception, string language) // TODO: LocalizationService: Use language enum instead of string?
		{
			var errorAsString = exception.GetType().Name;
			return ResolveLocalizedText(errorAsString, Hash.FromAnonymousObject(exception), language);
		}

		public static string ResolveLocalizedText(int code, string language)
		{
			return ResolveLocalizedText(code.ToString(), new Dictionary<string, object>(), language);
		}

		public static string ResolveLocalizedText(int code, IDictionary<string, object> additionalData, string language)
		{
			return ResolveLocalizedText(code.ToString(), additionalData, language);
		}

		public static string ResolveLocalizedText(string code, string language)
		{
			return ResolveLocalizedText(code, new Dictionary<string, object>(), language);
		}

		public static string ResolveLocalizedText(string code, IDictionary<string, object> additionalData, string language)
		{
			return ResolveLocalizedText(code, Hash.FromDictionary(additionalData), language);
		}

		private static string ResolveLocalizedText(string code, Hash additionalData, string language)
		{
			var cultureInfo = new CultureInfo(language);
			var catalog = GetCatalog(language);

			// catalog not found, so try to get the base catalog immediately
			if (catalog == null)
			{
				cultureInfo = cultureInfo.Parent;
				catalog = GetCatalog(cultureInfo.TwoLetterISOLanguageName);
			}

			// no catalog, just return the error code as a string
			if (catalog == null)
			{
				return code;
			}

			var value = catalog.GetString(code);

			// try on base catalog if no translation is found
			if (!cultureInfo.IsNeutralCulture && value == code)
			{
				cultureInfo = cultureInfo.Parent;
				catalog = GetCatalog(cultureInfo.TwoLetterISOLanguageName);
				value = catalog.GetString(code);
			}

			// render data into translated string
			return Template.Parse(value).Render(additionalData);
		}

		private static ICatalog GetCatalog(string language)
		{
			ICatalog result;

			if (!s_CachedCatalogs.TryGetValue(language, out result))
			{
				var cultureInfo = new CultureInfo(language);

				// TODO: Extend
				// language == "de" ? Resources.de : Resources.en;
				byte[] resourceStream = new byte[0];
				try
				{
					result = new Catalog(new MemoryStream(resourceStream), new CultureInfo(language));
					s_CachedCatalogs.TryAdd(language, result);
				}
				catch
				{
					throw;
				}
			}

			return result;
		}
	}
}
