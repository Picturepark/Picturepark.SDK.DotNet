using Picturepark.SDK.V1.Contract;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests
{
	public class PermissionTests : IClassFixture<SDKClientFixture>
	{
		private SDKClientFixture _fixture;
		private PictureparkClient _client;

		public PermissionTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldGetUserPermissions()
		{
			/// Act
			var result = await _client.Permissions.GetUserPermissionsAsync(UserRight.ManageCollections);

			/// Assert
			Assert.True(result);
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldGetContentPermissions()
		{
			/// Arrange
			string permissionSetId = _fixture.GetRandomContentPermissionSetId(20);
			Assert.False(string.IsNullOrEmpty(permissionSetId));

			/// Act
			ContentPermissionSetDetail result = await _client.Permissions.GetContentPermissionsAsync(permissionSetId);

			/// Assert
			Assert.False(string.IsNullOrEmpty(result.Id));
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldGetSchemaPermissions()
		{
			/// Arrange
			string permissionSetId = _fixture.GetRandomMetadataPermissionSetId(20);
			Assert.False(string.IsNullOrEmpty(permissionSetId));

			/// Act
			SchemaPermissionSetDetail result = await _client.Permissions.GetSchemaPermissionsAsync(permissionSetId);

			/// Assert
			Assert.False(string.IsNullOrEmpty(result.Id));
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldSearchContentPermissions()
		{
			/// Arrange
			var request = new PermissionSetSearchRequest() { Limit = 20 };

			/// Act
			PermissionSetSearchResult result = await _client.Permissions.SearchContentPermissionsAsync(request);

			/// Assert
			Assert.True(result.Results.Count() > 0);
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldSearchMetadataPermissions()
		{
			/// Arrange
			var request = new PermissionSetSearchRequest() { Limit = 20 };

			/// Act
			PermissionSetSearchResult result = await _client.Permissions.SearchSchemaPermissionsAsync(request);

			/// Assert
			Assert.True(result.Results.Count() > 0);
		}
	}
}
