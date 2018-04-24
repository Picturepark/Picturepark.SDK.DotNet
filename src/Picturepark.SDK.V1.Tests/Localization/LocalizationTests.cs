using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Localization;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Localization
{
    public class LocalizationTests
    {
        [Fact]
        [Trait("Stack", "Localization")]
        public void ShouldLocalizeException()
        {
            /// Arrange
            var exception = new CustomerNotFoundException { CustomerId = "TestCustomerId" };

            /// Act
            var textEn = exception.GetLocalizedErrorCode("en");
            var textDe = exception.GetLocalizedErrorCode("de");

            /// Assert
            Assert.False(string.IsNullOrEmpty(textEn));
            Assert.False(string.IsNullOrEmpty(textDe));
        }

        [Fact]
        [Trait("Stack", "Localization")]
        public void ShouldLocalizeTextFromStringCode()
        {
            /// Act
            var text = LocalizationService.GetLocalizedText("CustomerNotFoundException", "en");

            /// Assert
            Assert.False(string.IsNullOrEmpty(text));
        }

        [Fact]
        [Trait("Stack", "Localization")]
        public void ShouldLocalizeTextFromIntegerCode()
        {
            /// Act
            var text = LocalizationService.GetLocalizedText(100304, "en");

            /// Assert
            Assert.False(string.IsNullOrEmpty(text));
        }
    }
}
