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

            var process = await Client.Users.InviteManyAsync(usersToCreate);

            await WaitOnBusinessProcessAndAssert(process);

            var searchRes = await Client.Users.SearchAsync(new UserSearchRequest
            {
                Filter = FilterBase.FromExpression<User>(u => u.EmailAddress, usersToCreate.Select(u => u.EmailAddress).ToArray())
            });

            searchRes.Results.Select(e => e.EmailAddress).Should()
                .BeEquivalentTo(usersToCreate.Select(e => e.EmailAddress), "all users should have been invited/created");

            var invitedUserIds = searchRes.Results.Select(u => u.Id).ToArray();

            var reviewProcess = await Client.Users.ReviewManyAsync(new UserReviewRequest
            {
                UserIds = invitedUserIds,
                Reviewed = true
            });

            await WaitOnBusinessProcessAndAssert(reviewProcess);

            var reviewedUsers = await GetUsersByIds(invitedUserIds);

            reviewedUsers.Should().OnlyContain(
                u => u.AuthorizationState == AuthorizationState.Active, "all invited users should be active after review");

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
            Client.Users.DeleteManyAsync(new UserDeactivateRequest { UserIds = _createdUserIds.ToArray() })
                .GetAwaiter().GetResult().Rows.Should().OnlyContain(r => r.Succeeded);

            base.Dispose();
        }

        public async Task<IReadOnlyList<UserDetail>> GetUsersByIds(IEnumerable<string> ids)
        {
            var reviewedUserTasks = ids.Select(async id => await Client.Users.GetAsync(id)).ToArray();

            return await Task.WhenAll(reviewedUserTasks);
        }

        public async Task WaitOnBusinessProcessAndAssert(BusinessProcess businessProcess)
        {
            businessProcess.BusinessProcessScope.Should().Be(BusinessProcessScope.User);

            var waitResult = await Client.BusinessProcesses.WaitForCompletionAsync(businessProcess.Id, TimeSpan.FromSeconds(10));

            waitResult.Finished.Should().BeTrue();
        }
    }
}