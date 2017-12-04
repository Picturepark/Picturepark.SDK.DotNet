using Picturepark.SDK.V1.Contract;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
	public class PermissionTests : IClassFixture<ClientFixture>
	{
		private ClientFixture _fixture;
		private PictureparkClient _client;

		public PermissionTests(ClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldGetUserRights()
		{
			/// Act
			var result = await _client.Permissions.GetUserRightsAsync();

			/// Assert
			Assert.Contains(UserRight.ManageCollections, result);
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldCheckHasUserRight()
		{
			/// Act
			var result = await _client.Permissions.HasUserRightAsync(UserRight.ManageCollections);

			/// Assert
			Assert.True(result);
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldGetContentPermissionSet()
		{
			/// Arrange
			string permissionSetId = await _fixture.GetRandomContentPermissionSetId(20);
			Assert.False(string.IsNullOrEmpty(permissionSetId));

			/// Act
			ContentPermissionSetDetail result = await _client.Permissions.GetContentPermissionSetAsync(permissionSetId);

			/// Assert
			Assert.False(string.IsNullOrEmpty(result.Id));
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldGetSchemaPermissionSet()
		{
			/// Arrange
			string permissionSetId = await _fixture.GetRandomMetadataPermissionSetId(20);
			Assert.False(string.IsNullOrEmpty(permissionSetId));

			/// Act
			SchemaPermissionSetDetail result = await _client.Permissions.GetSchemaPermissionSetAsync(permissionSetId);

			/// Assert
			Assert.False(string.IsNullOrEmpty(result.Id));
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldSearchContentPermissionSets()
		{
			/// Arrange
			var request = new PermissionSetSearchRequest { Limit = 20 };

			/// Act
			PermissionSetSearchResult result = await _client.Permissions.SearchContentPermissionSetsAsync(request);

			/// Assert
			Assert.True(result.Results.Count > 0);
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldSearchSchemaPermissionSets()
		{
			/// Arrange
			var request = new PermissionSetSearchRequest { Limit = 20 };

			/// Act
			PermissionSetSearchResult result = await _client.Permissions.SearchSchemaPermissionSetsAsync(request);

			/// Assert
			Assert.True(result.Results.Count > 0);
		}
	}
}
