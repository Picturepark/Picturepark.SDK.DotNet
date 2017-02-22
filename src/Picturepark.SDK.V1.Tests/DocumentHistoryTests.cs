using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.IO;
using Picturepark.SDK.V1.Contract.Extensions;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests
{
	public class DocumentHistoryTests : IClassFixture<SDKClientFixture>
	{
		private readonly SDKClientFixture _fixture;
		private readonly PictureparkClient _client;

		public DocumentHistoryTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGet()
		{
			string documentId = _fixture.GetRandomAssetId("*.jpg", 20);
			DocumentHistoryViewItem result = await _client.DocumentHistory.GetAsync(documentId);
		}

		[Fact(Skip = "TODO")]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetDifferenceOfAssetChange()
		{
			string assetId = _fixture.GetRandomAssetId("*.jpg", 20);
			var asset = await _client.Assets.GetAsync(assetId);

			var updateRequest = new AssetsMetadataUpdateRequest
			{
				AssetIds = new List<string> { asset.Id },
				ChangeCommands = new List<MetadataValuesChangeCommandBase>
				{
					new MetadataValuesSchemaUpsertCommand
					{
						MetadataSchemaId = "Drive",
						Value = new MetadataDictionary
						{
							{ "Location", "testlocation" }
						}
					}
				}
			};

			// TODO: Create AssetHelper to update and wait with one call
			var updateResult = await _client.Assets.UpdateMetadataManyAsync(updateRequest);
			var waitResult = await updateResult.Wait4MetadataAsync(_client.MetadataObjects);
			Assert.True(waitResult.HasStateHit);

			// Refetch asset and compare versions
			var updatedAsset = await _client.Assets.GetAsync(assetId);
			Assert.NotEqual(1/*updatedAsset.Version*/, 0);

			DocumentHistoryDifferenceViewItem result = await _client.DocumentHistory.GetDifferenceLatestAsync(assetId, 1);
		}

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetHistory1()
		{
			string documentId = _fixture.GetRandomAssetId("*.jpg", 20);
			long oldVersionId = 1;

			DocumentHistoryDifferenceViewItem result = await _client.DocumentHistory.GetDifferenceLatestAsync(documentId, oldVersionId);
		}

		[Fact(Skip = "TODO")]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetHistory2()
		{
			string documentId = _fixture.GetRandomAssetId("*.jpg", 20);
			long oldVersionId = 1;
			long newVersionId = 2;

			DocumentHistoryDifferenceViewItem result = await _client.DocumentHistory.GetDifferenceAsync(documentId, oldVersionId, newVersionId);
		}

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetVersion()
		{
			string documentId = _fixture.GetRandomAssetId("*.jpg", 20);
			string versionId = "1";

			DocumentHistoryViewItem result = await _client.DocumentHistory.GetVersionAsync(documentId, versionId);
		}
	}
}
