using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class UserRoleTests : IClassFixture<ClientFixture>
    {
        private const string JamesBond = "James Bond";
        private const string PeterGriffin = "Peter Griffin";
        private const string UpdatedRoleName = "Updated Role";
        private const string UpdatedRoleName2 = "Updated Role 2";

        private readonly IPictureparkService _client;

        public UserRoleTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        [Trait("Stack", "UserRoles")]
        public async Task ShouldCreateSearchAndDelete()
        {
            // Arrange
            var roles = await CreateRoles().ConfigureAwait(false);

            try
            {
                // Act
                var res = await _client.UserRole.SearchAsync(new UserRoleSearchRequest
                {
                    Filter = FilterBase.FromExpression<UserRole>(r => r.Names, "en", new[] { JamesBond, PeterGriffin })
                }).ConfigureAwait(false);

                // Assert
                res.Results.Should().HaveCount(2)
                    .And.ContainSingle(r => r.Names.ContainsValue(JamesBond))
                        .Which.UserRights.Should().ContainSingle(u => u == UserRight.ManageTransfer);

                res.Results.Should().ContainSingle(r => r.Names.ContainsValue(PeterGriffin))
                    .Which.UserRights.Should().ContainSingle(u => u == UserRight.ManageChannels);
            }
            finally
            {
                await Delete(roles).ConfigureAwait(false);
            }
        }

        [Fact]
        [Trait("Stack", "UserRoles")]
        public async Task ShouldUpdateSingle()
        {
            // Arrange
            var roles = await CreateRoles().ConfigureAwait(false);

            try
            {
                var role = roles.First();
                var update = new UserRoleEditable
                {
                    Names = role.Names,
                    UserRights = role.UserRights
                };
                update.Names["en"] = UpdatedRoleName;

                // Act
                var updatedRole = await _client.UserRole.UpdateAsync(role.Id, update).ConfigureAwait(false);

                // Assert
                updatedRole.Names.Should().ContainKey("en").And.Subject["en"].Should().Be(UpdatedRoleName);
                updatedRole.UserRights.Should().BeEquivalentTo(role.UserRights);

                var updateRoleFromApi = await _client.UserRole.GetAsync(role.Id).ConfigureAwait(false);
                updateRoleFromApi.Names.Should().ContainKey("en").And.Subject["en"].Should().Be(UpdatedRoleName);
                updateRoleFromApi.UserRights.Should().BeEquivalentTo(role.UserRights);
            }
            finally
            {
                await Delete(roles).ConfigureAwait(false);
            }
        }

        [Fact]
        [Trait("Stack", "UserRoles")]
        public async Task ShouldUpdateAndGetMany()
        {
            // Arrange
            var roles = await CreateRoles().ConfigureAwait(false);

            try
            {
                var role1 = roles[0];
                var role2 = roles[1];
                var update = new UserRoleUpdateManyRequest
                {
                    Items = new[] { role1, role2 }
                };
                role1.Names["en"] = UpdatedRoleName;
                role2.Names["en"] = UpdatedRoleName2;

                // Act
                var bulkResponse = await _client.UserRole.UpdateManyAsync(update).ConfigureAwait(false);

                // Assert
                bulkResponse.Rows.Should().OnlyContain(r => r.Succeeded);

                var updatedRoles = (await _client.UserRole.GetManyAsync(update.Items.Select(u => u.Id)).ConfigureAwait(false)).ToDictionary(u => u.Id);

                updatedRoles[role1.Id].Names.Should().ContainKey("en").And.Subject["en"].Should().Be(UpdatedRoleName);
                updatedRoles[role1.Id].UserRights.Should().BeEquivalentTo(role1.UserRights);

                updatedRoles[role2.Id].Names.Should().ContainKey("en").And.Subject["en"].Should().Be(UpdatedRoleName2);
                updatedRoles[role2.Id].UserRights.Should().BeEquivalentTo(role2.UserRights);
            }
            finally
            {
                await Delete(roles).ConfigureAwait(false);
            }
        }

        private async Task Delete(IEnumerable<UserRole> roles)
        {
            await _client.UserRole.DeleteManyAsync(new UserRoleDeleteManyRequest
            {
                Items = roles.Select(r => r.Id).ToArray()
            });
        }

        private async Task<UserRole[]> CreateRoles()
        {
            var role1 = await _client.UserRole.CreateAsync(new UserRoleCreateRequest
            {
                Names = new TranslatedStringDictionary(new Dictionary<string, string>
                {
                    { "en", JamesBond }
                }),
                UserRights = new[] { UserRight.ManageTransfer }
            }).ConfigureAwait(false);

            var role2 = await _client.UserRole.CreateAsync(new UserRoleCreateRequest
            {
                Names = new TranslatedStringDictionary(new Dictionary<string, string>
                {
                    { "en", PeterGriffin }
                }),
                UserRights = new[] { UserRight.ManageChannels }
            }).ConfigureAwait(false);

            return new[] { role1, role2 };
        }
    }
}