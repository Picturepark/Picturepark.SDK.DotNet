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
        private readonly PictureparkClient _client;

        public UserTests(UsersFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldSearch()
        {
            /// Act
            var searchResult = await _client.Users.SearchAsync(new UserSearchRequest { Limit = 10 });

            /// Assert
            Assert.True(searchResult.Results.Any());
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldGetUser()
        {
            /// Arrange
            var contentId = await _fixture.GetRandomContentIdAsync(".jpg", 50);
            var content = await _client.Contents.GetAsync(contentId);
            var owner = await _client.Users.GetByOwnerTokenAsync(content.OwnerTokenId);

            /// Act
            var user = await _client.Users.GetAsync(owner.Id);

            /// Assert
            Assert.Equal(owner.Id, user.Id);
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldGetByOwnerToken()
        {
            /// Arrange
            var contentId = await _fixture.GetRandomContentIdAsync(".jpg", 50);
            var content = await _client.Contents.GetAsync(contentId);

            /// Act
            var owner = await _client.Users.GetByOwnerTokenAsync(content.OwnerTokenId);

            /// Assert
            Assert.NotNull(owner);
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldInviteAndReviewUser()
        {
            var userCount = 3;

            await _fixture.CreateAndActivateUsers(userCount);
        }

        [Theory,
         InlineData(1),
         InlineData(3),
         InlineData(10)]
        [Trait("Stack", "Users")]
        public async Task ShouldLockAndUnlockUsers(int count)
        {
            // Arrange
            var activeUsers = await _fixture.CreateAndActivateUsers(count);
            var activeUserIds = activeUsers.Select(u => u.Id).ToArray();

            async Task CheckIfUsersAre(AuthorizationState auth) =>
                (await _client.Users.GetManyAsync(activeUserIds)).Should().OnlyContain(u => u.AuthorizationState == auth);

            // Act
            var lockProcess = await LockUnlockCall(activeUserIds, true);

            // Assert
            await _fixture.WaitOnBusinessProcessAndAssert(lockProcess);
            await CheckIfUsersAre(AuthorizationState.Locked);

            // Act
            var unlockProcess = await LockUnlockCall(activeUserIds, false);

            // Assert
            await _fixture.WaitOnBusinessProcessAndAssert(unlockProcess);
            await CheckIfUsersAre(AuthorizationState.Active);
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldUpdateUser()
        {
            var comment = "We don't like this guy";
            var city = "Aarray";

            // Arrange
            var user = await _fixture.CreateAndActivateUser();

            user.Comment = comment;
            user.Address = user.Address ?? new UserAddress();
            user.Address.City = city;

            // Act
            var updateResponse = await _client.Users.UpdateManyAsync(new[] { user });
            var updatedUser = await _client.Users.GetAsync(user.Id);

            // Assert
            updateResponse.Rows.Should().ContainSingle(r => r.Succeeded);

            updatedUser.Comment.Should().Be(
                comment, "update should have changed the comment field");

            updatedUser.Address.City.Should().Be(
                city, "update should have changed the address city field");
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldFailUserValidationIfTheUserIsAlreadyRegistered()
        {
            // Arrange
            var user = await _fixture.CreateAndActivateUser();
            var newUserImpostor = new UserCreateRequest
            {
                FirstName = "John",
                LastName = "Impostor",
                EmailAddress = user.EmailAddress
            };

            var newUserOk = new UserCreateRequest
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = $"john.doe.{nameof(ShouldFailUserValidationIfTheUserIsAlreadyRegistered)}@test.picturepark.com"
            };

            // Act
            var validated = await _client.Users.ValidateUserInvitesAsync(new[] { newUserImpostor, newUserOk });

            // Assert
            validated.Rows.Should().HaveCount(2)
                .And.ContainSingle(r => r.Succeeded)
                .And.ContainSingle(r => !r.Succeeded)
                    .Which.Error.Should().Contain(newUserImpostor.EmailAddress);
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldReturnMultipleUsersCorrectly()
        {
            // Arrange
            var users = await _fixture.CreateAndActivateUsers(5);

            // Act
            var retrievedUsers = await _client.Users.GetManyAsync(users.Select(u => u.Id));

            // Assert
            retrievedUsers.Should().BeEquivalentTo(users);
        }

        private async Task<BusinessProcess> LockUnlockCall(IEnumerable<string> ids, bool @lock)
        {
            return await _client.Users.LockManyAsync(new UserLockRequest
            {
                Lock = @lock,
                UserIds = ids.ToArray()
            });
        }
    }
}
