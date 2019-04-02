using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Helpers
{
    public class UsersEntityCreator : EntityCreatorBase<UserDetail>
    {
        public UsersEntityCreator(IPictureparkService client) : base(client)
        {
        }

        public override async Task<IReadOnlyList<UserDetail>> Create(int userCount)
        {
            var usersToCreate = Enumerable.Range(0, userCount).Select(_ =>
                new UserCreateRequest
                {
                    FirstName = "Test",
                    LastName = "User",
                    EmailAddress = $"test.user_{Guid.NewGuid()}@test.picturepark.com",
                    LanguageCode = "en"
                }).ToArray();

            var userCreationTasks = usersToCreate.Select(userRequest => Client.User.CreateAsync(userRequest));

            var createdUsers = await Task.WhenAll(userCreationTasks);

            var searchRes = await Client.User.SearchAsync(new UserSearchRequest
            {
                Filter = FilterBase.FromExpression<User>(u => u.EmailAddress, usersToCreate.Select(u => u.EmailAddress).ToArray())
            });

            searchRes.Results.Select(e => e.EmailAddress).Should()
                .BeEquivalentTo(usersToCreate.Select(e => e.EmailAddress), "all users should have been created");

            foreach (var createdUser in createdUsers)
            {
                await Client.User.ReviewAsync(createdUser.Id, new UserReviewRequest
                {
                    Reviewed = true
                });
            }

            var createdUserIds = createdUsers.Select(u => u.Id);
            var reviewedUsers = (await Client.User.GetManyAsync(createdUserIds)).ToArray();

            reviewedUsers.Should()
                .HaveCount(createdUsers.Length, "all previously created users should have been retrieved");

            reviewedUsers.Should().OnlyContain(
                u => u.AuthorizationState == AuthorizationState.Reviewed, "all invited users should be reviewed after review");

            AddCreated(reviewedUsers.Select(u => u.Id));

            return reviewedUsers;
        }

        protected override Task Delete(string id) => Client.User.DeleteAsync(id, new UserDeleteRequest());
    }
}