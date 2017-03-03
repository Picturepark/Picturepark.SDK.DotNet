using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.IO;
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
		public async Task ShouldGetContentPermissions()
		{
			string permissionSetId = _fixture.GetRandomContentPermissionSetId(20);
			Assert.False(string.IsNullOrEmpty(permissionSetId));

			ContentPermissionSetDetailViewItem result = await _client.Permissions.GetContentPermissionsAsync(permissionSetId);
			Assert.False(string.IsNullOrEmpty(result.Id));
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldGetMetadataPermissions()
		{
			string permissionSetId = _fixture.GetRandomMetadataPermissionSetId(20);
			Assert.False(string.IsNullOrEmpty(permissionSetId));

			MetadataPermissionSetDetailViewItem result = await _client.Permissions.GetMetadataPermissionsAsync(permissionSetId);
			Assert.False(string.IsNullOrEmpty(result.Id));
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldSearchContentPermissions()
		{
			var request = new PermissionSetSearchRequest() { Limit = 20 };
			PermissionSetSearchResult result = await _client.Permissions.SearchContentPermissionsAsync(request);

			Assert.True(result.Results.Count() > 0);
		}

		[Fact]
		[Trait("Stack", "Permissions")]
		public async Task ShouldSearchMetadataPermissions()
		{
			var request = new PermissionSetSearchRequest() { Limit = 20 };
			PermissionSetSearchResult result = await _client.Permissions.SearchMetadataPermissionsAsync(request);

			Assert.True(result.Results.Count() > 0);
		}
	}
}
