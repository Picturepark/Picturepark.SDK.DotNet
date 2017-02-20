using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.IO;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests
{
	public class AssetTests : IClassFixture<SDKClientFixture>
	{
		private SDKClientFixture _fixture;
		private PictureparkClient _client;

		public AssetTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldAggregateByChannel()
		{
			string channelId = "RootChannel";

			var request = new AssetAggregationRequest() { SearchString = string.Empty };
			ObjectAggregationResult result = await _client.Assets.AggregateByChannelAsync(request, channelId);

			request.Aggregators = new List<AggregatorBase>()
			{
				new TermsAggregator { Name = "Test", Field = "AssetType", Size = 10 }
			};

			result = await _client.Assets.AggregateByChannelAsync(request, channelId);
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldAggregateWithAggregators()
		{
			//// TODO: finds one asset, but logic is not clear. Must be extended and asserted with useful data

			var request = new AssetAggregationRequest() { SearchString = string.Empty };
			request.Aggregators = new List<AggregatorBase>();

			// First Aggregator
			request.Aggregators.Add(new TermsAggregator { Name = "Aggregator1", Field = "AssetType", Size = 10 });

			// Second Aggregator
			var ranges = new List<NumericRange>()
			{
				new NumericRange() { From = null, To = 499,  Names = new TranslatedStringDictionary { { "en", "Aggregator2a" } } },
				new NumericRange() { From = 500, To = 5000, Names = new TranslatedStringDictionary { { "en", "Aggregator2b" } } }
			};

			var numRangeAggregator = new NumericRangeAggregator()
			{
				Name = "NumberAggregator",
				Field = "Original.Width",
				Ranges = ranges
			};

			request.Aggregators.Add(numRangeAggregator);
			ObjectAggregationResult result = await _client.Assets.AggregateAsync(request);
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldAggregateWithoutAggregators()
		{
			// Todo: does not find anything
			// Check: does Aggregate() without Aggregators make any sense?
			//// TODO: Must be extended and asserted with useful data.

			var request = new AssetAggregationRequest() { SearchString = "*" };
			ObjectAggregationResult result = await _client.Assets.AggregateAsync(request);

			var numRangeFilter = new NumericRangeFilter() { Field = "AssetType", Range = new NumericRange { From = 2, To = 5 } };
			request.Filter = numRangeFilter;
			result = await _client.Assets.AggregateAsync(request);

			request.Filter = null;
			request.LifeCycleFilter = LifeCycleFilter.All;
			result = await _client.Assets.AggregateAsync(request);

			request.Aggregators = new List<AggregatorBase>();
			result = await _client.Assets.AggregateAsync(request);
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldCreateBatchAssetDownload()
		{
			var request = new AssetBatchDownloadRequest();
			request.Assets = new List<Asset>();

			string assetId1 = _fixture.GetRandomAssetId("*.jpg", 50);
			string assetId2 = _fixture.GetRandomAssetId("*.jpg", 50);
			Assert.False(string.IsNullOrEmpty(assetId1));

			request.Assets.Add(new Asset() { AssetId = assetId1, OutputFormatId = "Original" });
			if (assetId1 != assetId2)
				request.Assets.Add(new Asset() { AssetId = assetId2, OutputFormatId = "Original" });

			AssetBatchDownloadItem result = await _client.Assets.CreateBatchAssetDownloadAsync(request);
			Assert.True(result.DownloadToken != null);
		}

		// [Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldCreateVirtualAssets()
		{
			//// TODO BRO: Implement

			string assetId = _fixture.GetRandomAssetId("*.jpg", 20);
			Assert.False(string.IsNullOrEmpty(assetId));

			await Task.FromResult<object>(null);
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldDownloadMultiple()
		{
			int maxNumberOfDownloadFiles = 3;
			string searchString = string.Empty;

			AssetSearchResult result = _fixture.GetRandomAssets(searchString, maxNumberOfDownloadFiles);
			Assert.True(result.Results.Count > 0);

			await _client.Assets.DownloadFilesAsync(
				result,
				_fixture.Configuration.TempPath,
				true,
				successDelegate: (asset) =>
				{
					Console.WriteLine(asset.GetFileMetadata().FileName);
				},
				errorDelegate: (error) =>
				{
					Console.WriteLine(error);
				});
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldDownloadSingle()
		{
			string assetId = _fixture.GetRandomAssetId("*.jpg", 20);
			Assert.False(string.IsNullOrEmpty(assetId));
			AssetDetailViewItem assetDetail = await _client.Assets.GetAsync(assetId);

			var fileMetadata = assetDetail.GetFileMetadata();
			var fileName = new Random().Next(0, 999999).ToString() + "-" + fileMetadata.FileName + ".jpg";
			var filePath = Path.Combine(_fixture.Configuration.TempPath, fileName);

			if (File.Exists(filePath))
				File.Delete(filePath);

			using (var response = await _client.Assets.DownloadAsync(assetId, "Original", "bytes=0-20000000"))
			{
				var stream = response.Stream;
				Assert.Equal(true, stream.CanRead);

				response.Stream.SaveFile(filePath);
				Assert.True(File.Exists(filePath));
			}
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldDownloadSingleResized()
		{
			// Download a resized version of an image file
			string assetId = _fixture.GetRandomAssetId("*.jpg", 20);
			Assert.False(string.IsNullOrEmpty(assetId));
			AssetDetailViewItem assetDetail = await _client.Assets.GetAsync(assetId);

			var fileMetadata = assetDetail.GetFileMetadata();
			var fileName = new Random().Next(0, 999999).ToString() + "-" + fileMetadata.FileName + ".jpg";
			var filePath = Path.Combine(_fixture.Configuration.TempPath, fileName);

			if (File.Exists(filePath))
				File.Delete(filePath);

			using (var response = await _client.Assets.DownloadResizedAsync(assetId, "Original", 200, 200))
			{
				response.Stream.SaveFile(filePath);
			}

			Assert.True(File.Exists(filePath));
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldDownloadSingleThumbnail()
		{
			// Download a resized version of an image file
			string assetId = _fixture.GetRandomAssetId("*.jpg", 20);
			Assert.False(string.IsNullOrEmpty(assetId));

			var fileName = new Random().Next(0, 999999).ToString() + "-" + assetId + ".jpg";
			var filePath = Path.Combine(_fixture.Configuration.TempPath, fileName);

			if (File.Exists(filePath))
				File.Delete(filePath);

			using (var response = await _client.Assets.DownloadThumbnailAsync(assetId, ThumbnailSize.Small))
			{
				response.Stream.SaveFile(filePath);
			}

			Assert.True(File.Exists(filePath));
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldGet()
		{
			// Todo: Typed Aufruf?  CustomAssetDetailViewItem<AssetMetadata> result = Get<AssetMetadata>(assetId);
			string assetId = _fixture.GetRandomAssetId("*.jpg", 20);
			Assert.False(string.IsNullOrEmpty(assetId));

			AssetDetailViewItem result = await _client.Assets.GetAsync(assetId);
			Assert.True(result.EntityType == EntityType.Asset);
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldGetDocumentMetadata()
		{
			string assetId = _fixture.GetRandomAssetId("*.doc", 20);
			if (string.IsNullOrEmpty(assetId))
				assetId = _fixture.GetRandomAssetId("*.docx", 20);
			Assert.False(string.IsNullOrEmpty(assetId));

			AssetDetailViewItem result = await _client.Assets.GetAsync(assetId);

			FileMetadata fileMetadata = result.GetFileMetadata();
			Assert.False(string.IsNullOrEmpty(fileMetadata.FileName));
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldGetResolved()
		{
			string assetId = _fixture.GetRandomAssetId("*.jpg", 20);
			AssetDetailViewItem result = await _client.Assets.GetAsync(assetId, true);
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldSearch()
		{
			var channelIds = new List<string> { "RootChannel" };
			var languages = new List<string>();
			string searchString = "*";

			var sortInfos = new List<SortInfo>
			{
				new SortInfo { Direction = SortDirection.Asc, Field = PropertyHelper.GetName<AssetDetailViewItem>(i => i.Audit.CreationDate) }
			};

			var filter = new TermFilter { Field = "MetadataSchemaIds", Term = "Base" };
			var filter2 = new TermFilter { Field = "Audit.CreatedByUser.Id", Term = "Base" };

			// TODO BRO: Implement generic filter creator
			//// var filter = new Filter().TermFilter<AssetDetailViewItem>(i => i.MetadataSchemaIds, "Base");
			//// var filter2 = new Filter().TermFilter<AssetDetailViewItem>(i => i.Audit.CreatedByUser.Id, "Base");

			var and = new AndFilter { Filters = new List<FilterBase> { filter, filter2 } };

			var request = new AssetSearchRequest()
			{
				ChannelIds = channelIds,
				SearchString = searchString,
				Sort = sortInfos,
				Start = 0,
				Limit = 8
			};

			AssetSearchResult result = await _client.Assets.SearchAsync(request);
			Assert.True(result.Results.Count > 0);
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldSearchByChannel()
		{
			string channelId = "RootChannel";
			string searchString = "*";

			var sortInfos = new List<SortInfo>()
			{
				new SortInfo { Direction = SortDirection.Asc, Field = "Audit.CreationDate" }
			};

			var request = new AssetSearchRequest()
			{
				ChannelIds = new List<string> { channelId },
				SearchString = searchString,
				Sort = sortInfos,
				Start = 0,
				Limit = 8
			};

			AssetSearchResult result = await _client.Assets.SearchByChannelAsync(request, channelId);
			Assert.True(result.Results.Count > 0);
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldTrashAndUntrashRandomAsset()
		{
			string assetId = _fixture.GetRandomAssetId("*.jpg", 20);
			Assert.False(string.IsNullOrEmpty(assetId));

			var assetDetailViewItem = await _client.Assets.GetAsync(assetId);

			// Trash
			await _client.Assets.DeactivateAsync(assetId);

			await Assert.ThrowsAsync<ApiException<AssetNotFoundException>>(async () => await _client.Assets.GetAsync(assetId));

			// UnTrash
			var reactivatedAsset = await _client.Assets.ReactivateAsync(assetId, resolve: false, timeout: 60000);
			Assert.True(reactivatedAsset != null);
		}

		[Trait("Stack", "Assets")]
		public async Task ShouldUpdateMetadata()
		{
			string assetId = _fixture.GetRandomAssetId("*.jpg", 20);

			AssetDetailViewItem asset = await _client.Assets.GetAsync(assetId);

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

			BusinessProcessViewItem result = await _client.Assets.UpdateMetadataManyAsync(updateRequest);
			BusinessProcessWaitResult waitResult = await result.Wait4MetadataAsync(_client.MetadataObjects);

			Assert.True(waitResult.HasStateHit);
		}

		[Fact]
		[Trait("Stack", "Assets")]
		public async Task ShouldUpdatePermissions()
		{
			string assetId = _fixture.GetRandomAssetId("*.jpg", 20);

			Assert.False(string.IsNullOrEmpty(assetId));

			AssetDetailViewItem assetDetail = await _client.Assets.GetAsync(assetId);
			Assert.True(assetDetail.EntityType == EntityType.Asset);

			var assetPermissionSetIds = new List<string>() { "aaa" + new Random().Next(0, 999).ToString(), "bbb" + new Random().Next(0, 999).ToString() };
			assetDetail.AssetPermissionSetIds = assetPermissionSetIds;

			BusinessProcessViewItem result = await _client.Assets.UpdatePermissionsManyAsync(new List<UpdateAssetPermissionsRequest> { new UpdateAssetPermissionsRequest { AssetId = assetDetail.Id, AssetPermissionSetIds = assetDetail.AssetPermissionSetIds } });
			await result.Wait4StateAsync("Completed", _client.BusinessProcesses);

			assetDetail = await _client.Assets.GetAsync(assetId);
			var currentAssetPermissionSetIds = assetDetail.AssetPermissionSetIds.Select(i => i).ToList();

			Assert.True(assetPermissionSetIds.Except(currentAssetPermissionSetIds).Count() == 0);
			Assert.True(currentAssetPermissionSetIds.Except(assetPermissionSetIds).Count() == 0);
		}
	}
}
