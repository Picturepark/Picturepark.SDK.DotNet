using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class UsersFixture : ClientFixture
    {
        private readonly ConcurrentQueue<string> _createdUserIds = new ConcurrentQueue<string>();

        public async Task<IReadOnlyList<UserDetail>> CreateAndActivateUsers(int userCount)
        {
            var usersToCreate = Enumerable.Range(0, userCount).Select(_ =>
                new UserCreateRequest
                {
                    FirstName = "Test",
                    LastName = "User",
                    EmailAddress = $"test.user_{Guid.NewGuid()}@test.picturepark.com",
                    LanguageCode = "en"
                }).ToArray();

            var userCreationTasks = usersToCreate.Select(async userRequest =>
            {
                var user = await Client.Users.CreateAsync(userRequest);
                await Client.Users.InviteAsync(user.Id);
                return user;
            });

            var createdUsers = await Task.WhenAll(userCreationTasks);

            var searchRes = await Client.Users.SearchAsync(new UserSearchRequest
            {
                Filter = FilterBase.FromExpression<User>(u => u.EmailAddress, usersToCreate.Select(u => u.EmailAddress).ToArray())
            });

            searchRes.Results.Select(e => e.EmailAddress).Should()
                .BeEquivalentTo(usersToCreate.Select(e => e.EmailAddress), "all users should have been invited/created");

            foreach (var createdUser in createdUsers)
            {
                await Client.Users.ReviewAsync(createdUser.Id, new UserReviewRequest
                {
                    Reviewed = true
                });
            }

            var createdUserIds = createdUsers.Select(u => u.Id);
            var reviewedUsers = (await Client.Users.GetManyAsync(createdUserIds)).ToArray();

            reviewedUsers.Should()
                .HaveCount(createdUsers.Length, "all previously created users should have been retrieved");

            reviewedUsers.Should().OnlyContain(
                u => u.AuthorizationState == AuthorizationState.Reviewed, "all invited users should be reviewed after review");

            foreach (var reviewedUser in reviewedUsers)
            {
                _createdUserIds.Enqueue(reviewedUser.Id);
            }

            return reviewedUsers;
        }

        public async Task<UserDetail> CreateAndActivateUser()
        {
            return (await CreateAndActivateUsers(1)).Single();
        }

        public override void Dispose()
        {
            if (!_createdUserIds.IsEmpty)
            {
                foreach (var createdUserId in _createdUserIds)
                {
                    Client.Users.DeleteAsync(createdUserId, new UserDeleteRequest()).GetAwaiter().GetResult();
                }
            }

            base.Dispose();
        }

        public async Task WaitOnBusinessProcessAndAssert(BusinessProcess businessProcess)
        {
            businessProcess.BusinessProcessScope.Should().Be(BusinessProcessScope.User);

            var waitResult = await Client.BusinessProcesses.WaitForCompletionAsync(businessProcess.Id, TimeSpan.FromSeconds(10));

            waitResult.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);
        }
    }
}