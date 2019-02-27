using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Picturepark.SDK.V1.Contract;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class SchemaPermissionSetTests : IClassFixture<SchemaPermissionSetsFixture>
    {
        private readonly SchemaPermissionSetsFixture _fixture;
        private readonly IPictureparkService _client;

        public SchemaPermissionSetTests(SchemaPermissionSetsFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "SchemaPermissionSets")]
        public async Task Should_get_schema_permission_set()
        {
            // Arrange
            var permissionSet = await _fixture.CreatePermissionSet().ConfigureAwait(false);

            // Act
            var result = await _client.SchemaPermissionSet.GetAsync(permissionSet.Id).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(permissionSet.Id);
        }

        [Fact]
        [Trait("Stack", "SchemaPermissionSets")]
        public async Task Should_update_schema_permission_set()
        {
            // Arrange
            var permissionSet = await _fixture.CreatePermissionSet().ConfigureAwait(false);
            var userRoleId = (await CreateUserRole(UserRight.ManageAllShares).ConfigureAwait(false)).Id;

            // Act
            var result = await _client.SchemaPermissionSet.UpdateAsync(
                permissionSet.Id,
                new SchemaPermissionSetUpdateRequest
                {
                    Names = permissionSet.Names,
                    UserRolesRights = new[]
                    {
                        new PermissionUserRoleRightsOfMetadataRight
                            { UserRoleId = userRoleId, Rights = new[] { MetadataRight.View, MetadataRight.ManageItems } }
                    }
                }).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.UserRolesRights.Should().HaveCount(1)
                .And.Subject.First().UserRoleId.Should().Be(userRoleId);

            var verifyPermissionSet = await _client.SchemaPermissionSet.GetAsync(permissionSet.Id).ConfigureAwait(false);

            verifyPermissionSet.UserRolesRights.Should().HaveCount(1)
                .And.Subject.First().UserRoleId.Should().Be(userRoleId);
        }

        [Fact]
        [Trait("Stack", "SchemaPermissionSets")]
        public async Task Should_delete_schema_permission_set()
        {
            // Arrange
            var permissionSet = await _fixture.CreatePermissionSet().ConfigureAwait(false);

            // Act
            await _client.SchemaPermissionSet.DeleteAsync(permissionSet.Id).ConfigureAwait(false);

            // Assert
            Action verifyPermissionSet = () => _client.SchemaPermissionSet.GetAsync(permissionSet.Id).GetAwaiter().GetResult();

            verifyPermissionSet.Should().Throw<PermissionSetNotFoundException>();
        }

        [Fact]
        [Trait("Stack", "SchemaPermissionSets")]
        public async Task Should_delete_multiple_schema_permission_sets()
        {
            // Arrange
            var permissionSets = await _fixture.CreatePermissionSets(5).ConfigureAwait(false);
            var toDeleteIds = permissionSets.Take(3).Select(s => s.Id).ToArray();
            var shouldStayIds = permissionSets.Skip(3).Select(s => s.Id).ToArray();

            // Act
            await _client.SchemaPermissionSet.DeleteManyAsync(
                new PermissionSetDeleteManyRequest
                {
                    PermissionSetIds = toDeleteIds
                }).ConfigureAwait(false);

            // Assert
            var verifyDeletedPermissionSets = await _client.SchemaPermissionSet.GetManyAsync(toDeleteIds).ConfigureAwait(false);
            verifyDeletedPermissionSets.Should().BeEmpty();

            var verifyPermissionSets = await _client.SchemaPermissionSet.GetManyAsync(shouldStayIds).ConfigureAwait(false);
            verifyPermissionSets.Select(s => s.Id).Should().BeEquivalentTo(shouldStayIds);
        }

        [Fact]
        [Trait("Stack", "SchemaPermissionSets")]
        public async Task Should_transfer_ownership_of_permission_set()
        {
            // Arrange
            var permissionSet = await _fixture.CreatePermissionSet().ConfigureAwait(false);
            var userRole = await CreateUserRole(UserRight.ManagePermissions).ConfigureAwait(false);
            var user = await _fixture.CreateAndActivateUser().ConfigureAwait(false);
            user.UserRoles.Add(userRole);
            await _client.User.UpdateAsync(user.Id, user);
            var sessionUser = (await _client.Profile.GetAsync().ConfigureAwait(false)).Id;

            // Act
            await _client.SchemaPermissionSet.TransferOwnershipAsync(
                permissionSet.Id, new PermissionSetOwnershipTransferRequest { TransferUserId = user.Id });

            // Assert
            var verifyPermissionSet = await _client.SchemaPermissionSet.GetAsync(permissionSet.Id).ConfigureAwait(false);
            var ownerTokenId = user.OwnerTokens.First().Id;

            verifyPermissionSet.OwnerTokenId.Should().Be(ownerTokenId);

            // Cleanup
            await _client.SchemaPermissionSet.TransferOwnershipAsync(
                permissionSet.Id, new PermissionSetOwnershipTransferRequest { TransferUserId = sessionUser });
        }

        [Fact]
        [Trait("Stack", "SchemaPermissionSets")]
        public async Task Should_search_schema_permission_sets()
        {
            // Arrange
            var request = new PermissionSetSearchRequest { Limit = 20 };

            // Act
            var result = await _client.SchemaPermissionSet.SearchAsync(request).ConfigureAwait(false);

            // Assert
            result.Results.Should().NotBeEmpty();
        }

        private async Task<UserRole> CreateUserRole(UserRight userRight, [CallerMemberName] string testName = null)
        {
            return await _client.UserRole.CreateAsync(new UserRoleCreateRequest
            {
                Names = { { "en", testName } },
                UserRights = { userRight }
            }).ConfigureAwait(false);
        }
    }
}
