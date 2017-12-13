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

			/// Assert
			var textEn = exception.GetLocalizedErrorCode("en");
			var textDe = exception.GetLocalizedErrorCode("de");

			/// Act
			Assert.False(string.IsNullOrEmpty(textEn));
			Assert.False(string.IsNullOrEmpty(textDe));
		}

		[Fact]
		[Trait("Stack", "Localization")]
		public void ShouldLocalizeTextFromStringCode()
		{
			/// Assert
			var text = LocalizationService.GetLocalizedText("CustomerNotFoundException", "en");

			/// Act
			Assert.False(string.IsNullOrEmpty(text));
		}

		[Fact]
		[Trait("Stack", "Localization")]
		public void ShouldLocalizeTextFromIntegerCode()
		{
			/// Assert
			var text = LocalizationService.GetLocalizedText(100304, "en");

			/// Act
			Assert.False(string.IsNullOrEmpty(text));
		}
	}
}
