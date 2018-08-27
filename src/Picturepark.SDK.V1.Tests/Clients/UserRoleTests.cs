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

        private readonly IPictureparkService _client;

        public UserRoleTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
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

        private async Task Delete(IEnumerable<UserRole> roles)
        {
            await _client.UserRole.DeleteManyAsync(new UserRoleDeleteManyRequest
            {
                Ids = roles.Select(r => r.Id).ToArray()
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