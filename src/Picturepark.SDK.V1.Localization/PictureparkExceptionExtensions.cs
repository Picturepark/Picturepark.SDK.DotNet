using System;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Localization
{
    public static class PictureparkExceptionExtensions
    {
        /// <summary>Gets the localized error message for the given exception.</summary>
        /// <param name="exception">The exception.</param>
        /// <param name="language">The language.</param>
        /// <returns>The localized error message.</returns>
        [Obsolete]
        public static string GetLocalizedErrorCode(this PictureparkException exception, string language)
        {
            return LocalizationService.GetLocalizedErrorCode(exception, language);
        }
    }
}
