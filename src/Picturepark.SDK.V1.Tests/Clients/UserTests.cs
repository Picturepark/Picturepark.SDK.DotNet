using System;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Picturepark.SDK.V1.Tests.FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class UserTests : IClassFixture<UsersFixture>
    {
        private readonly UsersFixture _fixture;
        private readonly IPictureparkService _client;

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
            var searchResult =
                await _client.User.SearchAsync(new UserSearchRequest { Limit = 10 }).ConfigureAwait(false);

            // Assert
            Assert.True(searchResult.Results.Any());
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldSearchAndAggregateAllTogether()
        {
            // Arrange
            var sharedName = $"Name_{Guid.NewGuid():N}";
            var user1 = await CreateUser(sharedName).ConfigureAwait(false);
            var user2 = await CreateUser(sharedName).ConfigureAwait(false);
            var user3 = await CreateUser().ConfigureAwait(false);

            // Act
            var searchRequest = new UserSearchRequest
            {
                Filter = FilterBase.FromExpression<User>(user => user.Id, user1.Id, user2.Id, user3.Id),
                Aggregators = new[]
                {
                    new TermsAggregator
                        { Field = nameof(User.FirstName).ToLowerCamelCase(), Name = "userNameAggregation" }
                }
            };
            var searchResult = await _client.User.SearchAsync(searchRequest).ConfigureAwait(false);

            // Assert
            searchResult.Results.Should().HaveCount(3).And.Subject.Select(r => r.Id).Should()
                .BeEquivalentTo(user1.Id, user2.Id, user3.Id);
            var aggregationResults = searchResult.AggregationResults.Should().HaveCount(1).And.Subject.First()
                .AggregationResultItems.Should().HaveCount(2).And.Subject;
            aggregationResults.Where(ar => ar.Name == sharedName).Should().HaveCount(1).And.Subject.First().Count
                .Should().Be(2);
            aggregationResults.Where(ar => ar.Name != sharedName).Should().HaveCount(1).And.Subject.First().Count
                .Should().Be(1);
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldGetUser()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 50)
                .ConfigureAwait(false);
            var content = await _client.Content.GetAsync(contentId).ConfigureAwait(false);
            var owner = await _client.User.GetByOwnerTokenAsync(content.OwnerTokenId).ConfigureAwait(false);

            // Act
            var user = await _client.User.GetAsync(owner.Id).ConfigureAwait(false);

            // Assert
            Assert.Equal(owner.Id, user.Id);

            user.Audit.CreatedByUser.Should().BeResolved();
            user.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldGetByOwnerToken()
        {
            // Arrange
            var contentId = await _fixture.GetRandomContentIdAsync("fileMetadata.fileExtension:.jpg", 50)
                .ConfigureAwait(false);
            var content = await _client.Content.GetAsync(contentId).ConfigureAwait(false);

            // Act
            var owner = await _client.User.GetByOwnerTokenAsync(content.OwnerTokenId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(owner);

            owner.Audit.CreatedByUser.Should().BeResolved();
            owner.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldInviteAndReviewUsers()
        {
            var userCount = 3;

            await _fixture.Users.Create(userCount).ConfigureAwait(false);
        }

        [Theory,
         InlineData(1),
         InlineData(3),
         InlineData(10)]
        [Trait("Stack", "Users")]
        public async Task ShouldLockAndUnlockUsers(int count)
        {
            // Arrange
            var activeUsers = await _fixture.Users.Create(count).ConfigureAwait(false);
            var activeUserIds = activeUsers.Select(u => u.Id).ToArray();

            async Task CheckIfUsersAreLocked(bool isLocked) =>
                (await _client.User.GetManyAsync(activeUserIds).ConfigureAwait(false)).Should()
                .OnlyContain(u => u.IsLocked == isLocked);

            // Act
            await LockUnlockCall(activeUserIds, true).ConfigureAwait(false);

            // Assert
            await CheckIfUsersAreLocked(true).ConfigureAwait(false);

            // Act
            await LockUnlockCall(activeUserIds, false).ConfigureAwait(false);

            // Assert
            await CheckIfUsersAreLocked(false).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldUpdateUser()
        {
            var comment = "We don't like this guy";
            var city = "Aarray";

            // Arrange
            var user = await _fixture.Users.Create().ConfigureAwait(false);

            user.Comment = comment;
            user.Address = user.Address ?? new UserAddress();
            user.Address.City = city;

            // Act
            var updatedUserResponse = await _client.User.UpdateAsync(user.Id, user).ConfigureAwait(false);
            var updatedUser = await _client.User.GetAsync(user.Id).ConfigureAwait(false);

            // Assert
            updatedUserResponse.Comment.Should().Be(
                comment, "update should have changed the comment field");

            updatedUserResponse.Address.City.Should().Be(
                city, "update should have changed the address city field");

            updatedUser.Comment.Should().Be(
                comment, "update should have changed the comment field");

            updatedUser.Address.City.Should().Be(
                city, "update should have changed the address city field");

            updatedUserResponse.Audit.CreatedByUser.Should().BeResolved();
            updatedUserResponse.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldReturnMultipleUsersCorrectly()
        {
            // Arrange
            var users = await _fixture.Users.Create(5).ConfigureAwait(false);

            // Act
            var retrievedUsers = await _client.User.GetManyAsync(users.Select(u => u.Id)).ConfigureAwait(false);

            // Assert
            retrievedUsers.Should().BeEquivalentTo(users);

            retrievedUsers.ToList().ForEach(user =>
                {
                    user.Audit.CreatedByUser.Should().BeResolved();
                    user.Audit.ModifiedByUser.Should().BeResolved();
                }
            );
        }

        [Fact]
        [Trait("Stack", "Users")]
        public async Task ShouldCreateSingleUser()
        {
            // Act
            var user = await CreateUser().ConfigureAwait(false);

            // Assert
            user.Should().NotBeNull();
            user.Audit.CreatedByUser.Should().BeResolved();
            user.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]

        [Trait("Stack", "Users")]
        public async Task ShouldAggregateByRoles()
        {
            // Arrange
            var roleAssignTimeout = TimeSpan.FromSeconds(30);

            var roleOneName = $"Role with one user ({nameof(ShouldAggregateByRoles)})";
            var roleTwoName = $"Role with two users ({nameof(ShouldAggregateByRoles)})";

            var users = await _fixture.Users.Create(3).ConfigureAwait(false);
            var roles = await _client.UserRole.CreateManyAsync(new UserRoleCreateManyRequest
            {
                Items = new List<UserRoleCreateRequest>
                {
                    new UserRoleCreateRequest
                    {
                        Names = new TranslatedStringDictionary { ["en"] = roleOneName },
                        RequestId = "one"
                    },
                    new UserRoleCreateRequest
                    {
                        Names = new TranslatedStringDictionary { ["en"] = roleTwoName },
                        RequestId = "two"
                    }
                }
            }).ConfigureAwait(false);

            var roleOneUserId = roles.Rows.Single(r => r.RequestId == "one").Id;
            var roleTwoUsersId = roles.Rows.Single(r => r.RequestId == "two").Id;

            var bpOne = await _client.User.AssignUserRolesAsync(new UserRoleAssignManyRequest
            {
                Operation = UserRoleAssignmentOperationType.Add,
                UserIds = users.Select(u => u.Id).Take(2).ToList(),
                UserRoleIds = new List<string> { roleTwoUsersId }
            }).ConfigureAwait(false);

            await _client.BusinessProcess.WaitForCompletionAsync(bpOne.Id, roleAssignTimeout).ConfigureAwait(false);

            var bpTwo = await _client.User.AssignUserRolesAsync(new UserRoleAssignManyRequest
            {
                Operation = UserRoleAssignmentOperationType.Add,
                UserIds = users.Select(u => u.Id).Take(1).ToList(),
                UserRoleIds = new List<string> { roleOneUserId }
            }).ConfigureAwait(false);

            await _client.BusinessProcess.WaitForCompletionAsync(bpTwo.Id, roleAssignTimeout).ConfigureAwait(false);

            // Act
            var result = await _client.User.SearchAsync(new UserSearchRequest
            {
                Filter = FilterBase.FromExpression<User>(u => u.Id, users.Select(u => u.Id).ToArray()),
                Aggregators = new List<AggregatorBase>
                {
                    new TermsRelationAggregator
                    {
                        DocumentType = TermsRelationAggregatorDocumentType.UserRole,
                        Name = "roles",
                        Field = nameof(UserWithRoles.UserRoleIds).ToLowerCamelCase()
                    }
                }
            }).ConfigureAwait(false);

            // Assert
            result.Results.Should().HaveCount(users.Count);

            var aggregationResult = result.AggregationResults.Should()
                .ContainSingle(aggResult => aggResult.Name == "roles").Which;
            aggregationResult.AggregationResultItems.Count.Should().Be(2);

            aggregationResult.AggregationResultItems.Should()
                .ContainSingle(aggResultItem => aggResultItem.Name == roleOneName)
                .Which.Count.Should().Be(1);

            aggregationResult.AggregationResultItems.Should()
                .ContainSingle(aggResultItem => aggResultItem.Name == roleTwoName)
                .Which.Count.Should().Be(2);
        }

        private async Task LockUnlockCall(IEnumerable<string> ids, bool @lock)
        {
            var lockRequests = ids.Select(async id =>
            {
                await _client.User.LockAsync(id, new UserLockRequest { Lock = @lock }).ConfigureAwait(false);
            });

            await Task.WhenAll(lockRequests).ConfigureAwait(false);
        }

        private async Task<UserDetail> CreateUser(string firstName = null, string lastName = null, string emailAddress = null)
        {
            var request = new UserCreateRequest
            {
                FirstName = firstName ?? $"Test_{System.Guid.NewGuid():N}",
                LastName = lastName ?? $"Use_{System.Guid.NewGuid():N}",
                EmailAddress = emailAddress ?? $"test.user_{System.Guid.NewGuid():N}@test.picturepark.com",
                LanguageCode = "en"
            };

            return await _client.User.CreateAsync(request).ConfigureAwait(false);
        }
    }
}
