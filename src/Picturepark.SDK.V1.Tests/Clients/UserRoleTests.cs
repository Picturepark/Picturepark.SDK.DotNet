﻿using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Picturepark.SDK.V1.Tests.FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var roles = await CreateRoles();

            try
            {
                // Act
                var res = await _client.UserRole.SearchAsync(new UserRoleSearchRequest
                {
                    Filter = FilterBase.FromExpression<UserRole>(r => r.Names, "en", new[] { JamesBond, PeterGriffin })
                });

                // Assert
                res.Results.Should().HaveCount(2)
                    .And.ContainSingle(r => r.Names.ContainsValue(JamesBond))
                        .Which.UserRights.Should().ContainSingle(u => u == UserRight.ManageTransfer);

                res.Results.Should().ContainSingle(r => r.Names.ContainsValue(PeterGriffin))
                    .Which.UserRights.Should().ContainSingle(u => u == UserRight.ManageChannels);
            }
            finally
            {
                await Delete(roles);
            }
        }

        [Fact]
        [Trait("Stack", "UserRoles")]
        public async Task ShouldUpdateSingle()
        {
            // Arrange
            var roles = await CreateRoles();

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
                var updatedRole = await _client.UserRole.UpdateAsync(role.Id, update);

                // Assert
                updatedRole.Names.Should().ContainKey("en").And.Subject["en"].Should().Be(UpdatedRoleName);
                updatedRole.UserRights.Should().BeEquivalentTo(role.UserRights);

                var updateRoleFromApi = await _client.UserRole.GetAsync(role.Id);
                updateRoleFromApi.Names.Should().ContainKey("en").And.Subject["en"].Should().Be(UpdatedRoleName);
                updateRoleFromApi.UserRights.Should().BeEquivalentTo(role.UserRights);

                updateRoleFromApi.Audit.CreatedByUser.Should().BeResolved();
                updateRoleFromApi.Audit.ModifiedByUser.Should().BeResolved();

                updatedRole.Audit.CreatedByUser.Should().BeResolved();
                updatedRole.Audit.ModifiedByUser.Should().BeResolved();
            }
            finally
            {
                await Delete(roles);
            }
        }

        [Fact]
        [Trait("Stack", "UserRoles")]
        public async Task ShouldUpdateAndGetMany()
        {
            // Arrange
            var roles = await CreateRoles();

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
                var bulkResponse = await _client.UserRole.UpdateManyAsync(update);

                // Assert
                bulkResponse.Rows.Should().OnlyContain(r => r.Succeeded);

                var getManyResult = await _client.UserRole.GetManyAsync(update.Items.Select(u => u.Id));
                var updatedRoles = getManyResult.ToDictionary(u => u.Id);

                updatedRoles[role1.Id].Names.Should().ContainKey("en").And.Subject["en"].Should().Be(UpdatedRoleName);
                updatedRoles[role1.Id].UserRights.Should().BeEquivalentTo(role1.UserRights);

                updatedRoles[role2.Id].Names.Should().ContainKey("en").And.Subject["en"].Should().Be(UpdatedRoleName2);
                updatedRoles[role2.Id].UserRights.Should().BeEquivalentTo(role2.UserRights);

                getManyResult.ToList().ForEach(userRole =>
                    {
                        userRole.Audit.CreatedByUser.Should().BeResolved();
                        userRole.Audit.ModifiedByUser.Should().BeResolved();
                    }
                );
            }
            finally
            {
                await Delete(roles);
            }
        }

        [Fact]
        [Trait("Stack", "UserRoles")]
        public async Task CreateManyShouldReturnRequestIdsWhereSpecified()
        {
            // Arrange
            var createManyRequest = new UserRoleCreateManyRequest
            {
                Items = new[]
                {
                    new UserRoleCreateRequest
                    {
                        Names = new TranslatedStringDictionary
                        {
                            { "en", "Group A" }
                        },
                        UserRights = new[] { UserRight.ManageAllShares },
                        RequestId = "A"
                    },
                    new UserRoleCreateRequest
                    {
                        Names = new TranslatedStringDictionary
                        {
                            { "en", "Group B" }
                        },
                        UserRights = new[] { UserRight.ManageAllShares }
                    },
                    new UserRoleCreateRequest
                    {
                        Names = new TranslatedStringDictionary
                        {
                            { "en", "Group C" }
                        },
                        UserRights = new[] { UserRight.ManageAllShares },
                        RequestId = "C"
                    },
                    new UserRoleCreateRequest
                    {
                        Names = new TranslatedStringDictionary
                        {
                            { "en", "Group D" }
                        },
                        UserRights = new[] { UserRight.ManageAllShares },
                        RequestId = "D"
                    }
                }
            };

            // Act
            var result = await _client.UserRole.CreateManyAsync(createManyRequest);

            try
            {
                // Assert
                var idToRequestLookup = result.Rows.ToDictionary(r => r.Id, r => r.RequestId);
                result.Rows.Should().HaveCount(4).And.Subject.Where(r => r.RequestId != null).Should().HaveCount(3);

                var roles = await _client.UserRole.GetManyAsync(idToRequestLookup.Keys);

                roles.Should().OnlyContain(ur => ur.Names["en"] == "Group B" || ur.Names["en"] == $"Group {idToRequestLookup[ur.Id]}");
            }
            finally
            {
                await Delete(result.Rows.Select(r => r.Id));
            }
        }

        private Task Delete(IEnumerable<UserRole> roles) => Delete(roles.Where(r => r != null).Select(r => r.Id));

        private Task Delete(IEnumerable<string> roleIds) =>
            _client.UserRole.DeleteManyAsync(new UserRoleDeleteManyRequest { Ids = roleIds.ToArray() });

        private async Task<UserRole[]> CreateRoles()
        {
            UserRoleDetail role1 = null;
            UserRoleDetail role2 = null;

            try
            {
                role1 = await _client.UserRole.CreateAsync(new UserRoleCreateRequest
                {
                    Names = new TranslatedStringDictionary(new Dictionary<string, string>
                    {
                        { "en", JamesBond }
                    }),
                    UserRights = new[] { UserRight.ManageTransfer }
                });

                // Asserts
                role1.Audit.CreatedByUser.Should().BeResolved();
                role1.Audit.ModifiedByUser.Should().BeResolved();
            }
            catch
            {
                await Delete(new[] { role1 });
                throw;
            }

            try
            {
                role2 = await _client.UserRole.CreateAsync(new UserRoleCreateRequest
                {
                    Names = new TranslatedStringDictionary(new Dictionary<string, string>
                    {
                        { "en", PeterGriffin }
                    }),
                    UserRights = new[] { UserRight.ManageChannels }
                });
            }
            catch
            {
                await Delete(new[] { role1, role2 } );
                throw;
            }

            return new[] { role1, role2 };
        }
    }
}