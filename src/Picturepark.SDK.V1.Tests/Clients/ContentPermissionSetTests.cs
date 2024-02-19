using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Picturepark.SDK.V1.Tests.FluentAssertions;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class ContentPermissionSetTests : IClassFixture<ContentPermissionSetsFixture>
    {
        private readonly ContentPermissionSetsFixture _fixture;
        private readonly IPictureparkService _client;

        public ContentPermissionSetTests(ContentPermissionSetsFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "ContentPermissionSets")]
        public async Task Should_get_content_permission_set()
        {
            // Arrange
            var permissionSet = await _fixture.CreatePermissionSet();

            // Act
            var result = await _client.ContentPermissionSet.GetAsync(permissionSet.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(permissionSet.Id);

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
        }

        [Fact]
        [Trait("Stack", "ContentPermissionSets")]
        public async Task Should_update_content_permission_set()
        {
            // Arrange
            var permissionSet = await _fixture.CreatePermissionSet();
            var userRoleId = (await CreateUserRole(UserRight.ManageAllShares)).Id;

            // Act
            var result = await _client.ContentPermissionSet.UpdateAsync(
                permissionSet.Id,
                new ContentPermissionSetUpdateRequest
                {
                    Names = permissionSet.Names,
                    UserRolesRights = new[]
                    {
                        new UserRoleRightsOfContentRight
                            { UserRoleId = userRoleId, Rights = new[] { ContentRight.AccessOriginal } }
                    }
                });

            // Assert
            result.Should().NotBeNull();
            result.UserRolesRights.Should().HaveCount(1)
                .And.Subject.First().UserRoleId.Should().Be(userRoleId);

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();

            var verifyPermissionSet = await _client.ContentPermissionSet.GetAsync(permissionSet.Id);

            verifyPermissionSet.UserRolesRights.Should().HaveCount(1)
                .And.Subject.First().UserRoleId.Should().Be(userRoleId);
        }

        [Fact]
        [Trait("Stack", "ContentPermissionSets")]
        public async Task Should_delete_content_permission_set()
        {
            // Arrange
            var permissionSet = await _fixture.CreatePermissionSet();

            // Act
            await _client.ContentPermissionSet.DeleteAsync(permissionSet.Id);

            // Assert
            var ex = await Record.ExceptionAsync(() => _client.ContentPermissionSet.GetAsync(permissionSet.Id));
            ex.Should().BeOfType<PermissionSetNotFoundException>();
        }

        [Fact]
        [Trait("Stack", "ContentPermissionSets")]
        public async Task Should_delete_multiple_content_permission_sets()
        {
            // Arrange
            var permissionSets = await _fixture.CreatePermissionSets(5);
            var toDeleteIds = permissionSets.Take(3).Select(s => s.Id).ToArray();
            var shouldStayIds = permissionSets.Skip(3).Select(s => s.Id).ToArray();

            // Act
            await _client.ContentPermissionSet.DeleteManyAsync(
                new PermissionSetDeleteManyRequest
                {
                    PermissionSetIds = toDeleteIds
                });

            // Assert
            var verifyDeletedPermissionSets = await _client.ContentPermissionSet.GetManyAsync(toDeleteIds);
            verifyDeletedPermissionSets.Should().BeEmpty();

            var verifyPermissionSets = await _client.ContentPermissionSet.GetManyAsync(shouldStayIds);
            verifyPermissionSets.Select(s => s.Id).Should().BeEquivalentTo(shouldStayIds);

            verifyPermissionSets.ToList().ForEach(contentPermissionSet =>
                {
                    contentPermissionSet.Audit.CreatedByUser.Should().BeResolved();
                    contentPermissionSet.Audit.ModifiedByUser.Should().BeResolved();
                }
            );
        }

        [Fact]
        [Trait("Stack", "ContentPermissionSets")]
        public async Task Should_transfer_ownership_of_permission_set()
        {
            // Arrange
            var permissionSet = await _fixture.CreatePermissionSet();
            var userRole = await CreateUserRole(UserRight.ManagePermissions);
            var user = await _fixture.Users.Create();
            user.UserRoles.Add(new UserRoleAssignment { UserRole = userRole });
            await _client.User.UpdateAsync(user.Id, user.AsUpdateRequest());
            var sessionUser = (await _client.Profile.GetAsync()).Id;

            // Act
            await _client.ContentPermissionSet.TransferOwnershipAsync(
                permissionSet.Id, new PermissionSetOwnershipTransferRequest { TransferUserId = user.Id });

            // Assert
            var verifyPermissionSet = await _client.ContentPermissionSet.GetAsync(permissionSet.Id);
            var ownerTokenId = user.OwnerTokens.First().Id;

            verifyPermissionSet.OwnerTokenId.Should().Be(ownerTokenId);

            // Cleanup
            await _client.ContentPermissionSet.TransferOwnershipAsync(
                permissionSet.Id, new PermissionSetOwnershipTransferRequest { TransferUserId = sessionUser });
        }

        [Fact]
        [Trait("Stack", "ContentPermissionSets")]
        public async Task Should_search_content_permission_sets()
        {
            // Arrange
            var request = new PermissionSetSearchRequest { Limit = 20 };

            // Act
            var result = await _client.ContentPermissionSet.SearchAsync(request);

            // Assert
            result.Results.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Stack", "ContentPermissionSets")]
        public async Task ShouldCreateSingleContentPermissionSet()
        {
            // Arrange
            var request = new ContentPermissionSetCreateRequest
            {
                Names = new TranslatedStringDictionary
                    {
                        { "en", $"Content_ps_test_{Guid.NewGuid():N}" }
                    }
            };

            // Act
            var result = await _client.ContentPermissionSet.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
        }

        private async Task<UserRole> CreateUserRole(UserRight userRight, [CallerMemberName] string testName = null)
        {
            return await _client.UserRole.CreateAsync(new UserRoleCreateRequest
            {
                Names = { { "en", testName } },
                UserRights = { userRight }
            });
        }
    }
}
