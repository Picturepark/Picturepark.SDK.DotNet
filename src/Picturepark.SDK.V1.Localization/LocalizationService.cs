using System;
using System.Collections;
using DotLiquid;
using NGettext;
using Picturepark.SDK.V1.Contract;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Linq;
using Template = DotLiquid.Template;

namespace Picturepark.SDK.V1.Localization
{
    /// <summary>Provides methods to translate texts from the Fotoware Alto backend.</summary>
    public class LocalizationService
    {
        private const string CatalogName = "picturepark";
        private const string FallbackLanguage = "en";

        private static readonly ConcurrentDictionary<string, ICatalog> _cachedCatalogs = new ConcurrentDictionary<string, ICatalog>();

        /// <summary>Gets the localized error message for the given exception.</summary>
        /// <param name="exception">The exception.</param>
        /// <param name="language">The language.</param>
        /// <returns>The localized error message.</returns>
        [Obsolete]
        public static string GetLocalizedErrorCode(PictureparkException exception, string language)
        {
            var errorAsString = exception.GetType().Name;
            return GetLocalizedText(errorAsString, language, Hash.FromAnonymousObject(exception));
        }

        /// <summary>Gets the localized text for the given code and language.</summary>
        /// <param name="code">The code to identify the text.</param>
        /// <param name="language">The language.</param>
        /// <returns>The localized text.</returns>
        [Obsolete]
        public static string GetLocalizedText(int code, string language)
        {
            return GetLocalizedText(code.ToString(), new Dictionary<string, object>(), language);
        }

        /// <summary>Gets the localized text for the given code and language.</summary>
        /// <param name="code">The code to identify the text.</param>
        /// <param name="additionalData">The additional placeholder data to use in the text.</param>
        /// <param name="language">The language.</param>
        /// <returns>The localized text.</returns>
        [Obsolete]
        public static string GetLocalizedText(int code, IDictionary<string, object> additionalData, string language)
        {
            return GetLocalizedText(code.ToString(), additionalData, language);
        }

        /// <summary>Gets the localized text for the given code and language.</summary>
        /// <param name="code">The code to identify the text.</param>
        /// <param name="language">The language.</param>
        /// <returns>The localized text.</returns>
        [Obsolete]
        public static string GetLocalizedText(string code, string language)
        {
            return GetLocalizedText(code, new Dictionary<string, object>(), language);
        }

        /// <summary>Gets the localized text for the given code and language.</summary>
        /// <param name="code">The code to identify the text.</param>
        /// <param name="additionalData">The additional placeholder data to use in the text.</param>
        /// <param name="language">The language.</param>
        /// <returns>The localized text.</returns>
        [Obsolete]
        public static string GetLocalizedText(string code, IDictionary<string, object> additionalData, string language)
        {
            return GetLocalizedText(code, language, Hash.FromDictionary(additionalData));
        }

        [Obsolete]
        public static string GetDateTimeLocalizedDisplayValue(string value)
        {
            Liquid.UseRubyDateFormat = true;
            var template = Template.Parse(value);
            return template.Render();
        }

        [Obsolete]
        public static void ReplaceDateTimeLocalizedDisplayValueInObject(object obj)
        {
            var type = obj.GetType();

            if (obj is JObject jObject)
            {
                var tokens = jObject.SelectTokens(".._displayValues").ToList()
                    .Concat(jObject.SelectTokens("..displayValues").ToList());

                foreach (var jToken in tokens.SelectMany(i => i.ToList()))
                {
                    var token = (JProperty)jToken;
                    token.Value = GetDateTimeLocalizedDisplayValue(token.Value.ToString());
                }

                return;
            }

            var properties = type.GetTypeInfo().DeclaredProperties.ToList();

            foreach (var property in properties)
            {
                if (property.Name == "DisplayValues" || property.Name == "DisplayValue")
                {
                    if (property.GetValue(obj) is IDictionary<string, string> displayValues)
                    {
                        foreach (var key in displayValues.Keys.ToList())
                            displayValues[key] = GetDateTimeLocalizedDisplayValue(displayValues[key]);
                    }

                    return;
                }

                if (property.PropertyType == typeof(ICollection<>))
                {
                    var list = (ICollection)property.GetValue(obj, null);
                    foreach (var item in list)
                    {
                        ReplaceDateTimeLocalizedDisplayValueInObject(item);
                    }
                }

                if (property.PropertyType.GetTypeInfo().IsClass)
                {
                    var value = property.GetValue(obj);
                    if (value != null)
                        ReplaceDateTimeLocalizedDisplayValueInObject(value);
                }
            }
        }

        private static string GetLocalizedText(string code, string language, Hash additionalData)
        {
            var catalog = TryFindBestCatalog(language);
            if (catalog == null)
            {
                return code;
            }

            var value = GetLocalizedRawText(catalog, code, language);
            return Template.Parse(value).Render(additionalData);
        }

        private static string GetLocalizedRawText(ICatalog catalog, string code, string language)
        {
            var value = catalog.GetString(code);

            var cultureInfo = new CultureInfo(language);
            if (cultureInfo.IsNeutralCulture == false && value == code)
            {
                cultureInfo = cultureInfo.Parent;

                catalog = TryGetCatalog(cultureInfo.TwoLetterISOLanguageName);
                if (catalog != null)
                {
                    return catalog.GetString(code);
                }
            }

            return value;
        }

        private static ICatalog TryFindBestCatalog(string language)
        {
            var catalog = TryGetCatalog(language);
            if (catalog != null)
            {
                return catalog;
            }

            var cultureInfo = new CultureInfo(language);
            cultureInfo = cultureInfo.Parent;

            return TryGetCatalog(cultureInfo.TwoLetterISOLanguageName);
        }

        private static ICatalog TryGetCatalog(string language)
        {
            ICatalog result;

            if (!_cachedCatalogs.TryGetValue(language, out result))
            {
                var assembly = typeof(LocalizationService).GetTypeInfo().Assembly;
                var resourceName = "Picturepark.SDK.V1.Localization.Languages." + language.ToLowerInvariant() + ".mo";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    result = new Catalog(stream, new CultureInfo(language));
                    _cachedCatalogs.TryAdd(language, result);
                }
            }

            return result;
        }
    }
}
