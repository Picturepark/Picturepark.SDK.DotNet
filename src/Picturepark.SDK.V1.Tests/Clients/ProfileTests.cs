using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class ProfileTests : IClassFixture<ClientFixture>
    {
        private readonly PictureparkClient _client;

        public ProfileTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Profile")]
        public async Task ShouldGetProfile()
        {
            // Act
            var profile = await _client.Profile.GetAsync().ConfigureAwait(false);

            // Assert
            profile.Should().NotBeNull();
            profile.Id.Should().NotBeNullOrEmpty();
            profile.UserRights.Should().NotBeNull().And.Subject.Should().Contain(UserRight.ManageCollections);
        }

        [Fact]
        [Trait("Stack", "Profile")]
        public async Task ShouldUpdateProfile()
        {
            // Arrange
            var profile = await _client.Profile.GetAsync().ConfigureAwait(false);

            // Act
            var updateRequest = new UserProfileUpdateRequest
            {
                Id = profile.Id,
                Address = profile.Address,
                EmailAddress = profile.EmailAddress,
                FirstName = profile.FirstName + "1",
                LanguageCode = profile.LanguageCode,
                LastName = profile.LastName
            };

            var updatedProfile = await _client.Profile.UpdateAsync(updateRequest).ConfigureAwait(false);

            // Assert
            Assert.Equal(profile.FirstName + "1", updatedProfile.FirstName);
        }
    }
}
