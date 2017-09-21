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
			string documentId = _fixture.GetRandomContentId(".jpg", 20);
			DocumentHistory result = await _client.DocumentHistory.GetAsync(documentId);
		}

		[Fact(Skip = "TODO")]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetDifferenceOfContentChange()
		{
			string contentId = _fixture.GetRandomContentId(".jpg", 20);
			var content = await _client.Contents.GetAsync(contentId);

			var updateRequest = new ContentsMetadataUpdateRequest
			{
				ContentIds = new List<string> { content.Id },
				ChangeCommands = new List<MetadataValuesChangeCommandBase>
				{
					new MetadataValuesSchemaUpsertCommand
					{
						SchemaId = "Drive",
						Value = new DataDictionary
						{
							{ "Location", "testlocation" }
						}
					}
				}
			};

			// TODO: Create ContentHelper to update and wait with one call
			var updateResult = await _client.Contents.UpdateMetadataManyAsync(updateRequest);
			var waitResult = await updateResult.Wait4MetadataAsync(_client.ListItems);
			Assert.True(waitResult.HasStateHit);

			// Refetch content and compare versions
			var updatedContent = await _client.Contents.GetAsync(contentId);
			Assert.NotEqual(1/*updatedContent.Version*/, 0);

			DocumentHistoryDifference result = await _client.DocumentHistory.GetDifferenceLatestAsync(contentId, 1);
		}

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetHistory1()
		{
			string documentId = _fixture.GetRandomContentId(".jpg", 20);
			long oldVersionId = 1;

			DocumentHistoryDifference result = await _client.DocumentHistory.GetDifferenceLatestAsync(documentId, oldVersionId);
		}

		[Fact(Skip = "TODO")]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetHistory2()
		{
			string documentId = _fixture.GetRandomContentId(".jpg", 20);
			long oldVersionId = 1;
			long newVersionId = 2;

			DocumentHistoryDifference result = await _client.DocumentHistory.GetDifferenceAsync(documentId, oldVersionId, newVersionId);
		}

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetVersion()
		{
			string documentId = _fixture.GetRandomContentId(".jpg", 20);
			string versionId = "1";

			DocumentHistory result = await _client.DocumentHistory.GetVersionAsync(documentId, versionId);
		}
	}
}
