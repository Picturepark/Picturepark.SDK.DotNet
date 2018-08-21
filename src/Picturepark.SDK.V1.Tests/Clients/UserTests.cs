using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Contract;
using System.Linq;
using FluentAssertions;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class UserTests : IClassFixture<UsersFixture>
    {
        private readonly UsersFixture _fixture;
        private readonly IPictureparkClient _client;

        public UserTests(UsersFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldSearch()
        {
            // Act
            var searchResult = await _client.Users.SearchAsync(new UserSearchRequest { Limit = 10 }).ConfigureAwait(false);

            // Assert
            Assert.True(searchResult.Results.Any());
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldGetUser()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync(".jpg", 50).ConfigureAwait(false);
            var content = await _client.Contents.GetAsync(contentId).ConfigureAwait(false);
            var owner = await _client.Users.GetByOwnerTokenAsync(content.OwnerTokenId).ConfigureAwait(false);

            // Act
            var user = await _client.Users.GetAsync(owner.Id).ConfigureAwait(false);

            // Assert
            Assert.Equal(owner.Id, user.Id);
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldGetByOwnerToken()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync(".jpg", 50).ConfigureAwait(false);
            var content = await _client.Contents.GetAsync(contentId).ConfigureAwait(false);

            // Act
            var owner = await _client.Users.GetByOwnerTokenAsync(content.OwnerTokenId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(owner);
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldInviteAndReviewUsers()
        {
            var userCount = 3;

            await _fixture.CreateAndActivateUsers(userCount).ConfigureAwait(false);
        }

        [Theory,
         InlineData(1),
         InlineData(3),
         InlineData(10)]
        [Trait("Stack", "Users")]
        public async Task ShouldLockAndUnlockUsers(int count)
        {
            // Arrange
            var activeUsers = await _fixture.CreateAndActivateUsers(count).ConfigureAwait(false);
            var activeUserIds = activeUsers.Select(u => u.Id).ToArray();

            async Task CheckIfUsersAre(AuthorizationState auth) =>
                (await _client.Users.GetManyAsync(activeUserIds).ConfigureAwait(false)).Should().OnlyContain(u => u.AuthorizationState == auth);

            // Act
            await LockUnlockCall(activeUserIds, true).ConfigureAwait(false);

            // Assert
            await CheckIfUsersAre(AuthorizationState.Locked).ConfigureAwait(false);

            // Act
            await LockUnlockCall(activeUserIds, false).ConfigureAwait(false);

            // Assert
            await CheckIfUsersAre(AuthorizationState.Reviewed).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldUpdateUser()
        {
            var comment = "We don't like this guy";
            var city = "Aarray";

            // Arrange
            var user = await _fixture.CreateAndActivateUser().ConfigureAwait(false);

            user.Comment = comment;
            user.Address = user.Address ?? new UserAddress();
            user.Address.City = city;

            // Act
            var updatedUserResponse = await _client.Users.UpdateAsync(user.Id, user).ConfigureAwait(false);
            var updatedUser = await _client.Users.GetAsync(user.Id).ConfigureAwait(false);

            // Assert
            updatedUserResponse.Comment.Should().Be(
                comment, "update should have changed the comment field");

            updatedUserResponse.Address.City.Should().Be(
                city, "update should have changed the address city field");

            updatedUser.Comment.Should().Be(
                comment, "update should have changed the comment field");

            updatedUser.Address.City.Should().Be(
                city, "update should have changed the address city field");
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldReturnMultipleUsersCorrectly()
        {
            // Arrange
            var users = await _fixture.CreateAndActivateUsers(5).ConfigureAwait(false);

            // Act
            var retrievedUsers = await _client.Users.GetManyAsync(users.Select(u => u.Id)).ConfigureAwait(false);

            // Assert
            retrievedUsers.Should().BeEquivalentTo(users);
        }

        private async Task LockUnlockCall(IEnumerable<string> ids, bool @lock)
        {
            var lockRequests = ids.Select(async id =>
            {
                await _client.Users.LockAsync(id, new UserLockRequest { Lock = @lock }).ConfigureAwait(false);
            });

            await Task.WhenAll(lockRequests).ConfigureAwait(false);
        }
    }
}
